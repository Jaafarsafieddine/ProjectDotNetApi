    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using NuGet.Common;
    using ProjectDotNet.DataServices;
    using ProjectDotNet.Models;
    using ProjectDotNet.Models.InputModels;
using ProjectDotNet.Models.OutPutModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
    using System.Security.Cryptography;
    using BC = BCrypt.Net.BCrypt;


    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDataContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(AppDataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(x => x.Email == registerDto.Email))
            {
                return BadRequest("Email is already in use.");
            }

            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            Password = Convert.ToBase64String(passwordHash),
            PasswordSalt = Convert.ToBase64String(passwordSalt),
            PhoneNumber = registerDto.PhoneNumber.ToString(),
            IsAdmin = 0,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create an empty cart for the user
            var cart = new AddToCart
            {
                UserId = user.Id
            };

            _context.AddToCarts.Add(cart);
            await _context.SaveChangesAsync();

            var token = CreateToken(user);
            return StatusCode(201, new { user.Id, token });
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            // Convert the base64 stored password and salt back to byte arrays
            byte[] passwordHash = Convert.FromBase64String(user.Password);
            byte[] passwordSalt = Convert.FromBase64String(user.PasswordSalt);

            // Now verify using both hash and salt
            if (!VerifyPasswordHash(loginDto.Password, passwordHash, passwordSalt))
                return Unauthorized("Invalid email or password.");

            var token = CreateToken(user);
            return Ok(new { token });
        }



    [HttpGet("getUserDetails")]
    [Authorize]  
    public async Task<ActionResult<UserDto>> GetUserDetails()
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return Unauthorized("User must be logged in.");
        }

        if (!int.TryParse(currentUserId, out int userId))
        {
            return BadRequest("Invalid user ID format.");
        }

        var user = await _context.Users
            .AsNoTracking()
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            })
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    [HttpPatch("updateUserDetails")]
    [Authorize] // Ensures that only authenticated users can access this endpoint
    public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDto updateUserDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out int currentUserId))
        {
            return Unauthorized("Invalid user credentials.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Update user details
        user.FirstName = updateUserDto.FirstName ?? user.FirstName;
        user.LastName = updateUserDto.LastName ?? user.LastName;
        user.Email = updateUserDto.Email ?? user.Email;
        user.PhoneNumber = updateUserDto.PhoneNumber ?? user.PhoneNumber;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok("User updated successfully.");
    }
    [HttpGet("all-users")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(userRole) || userRole != "Admin")
        {
            return BadRequest("Unauthorized: Only admins can view user details.");
        }

        var users = await _context.Users
            .Where(u => u.IsAdmin == 0) 
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpDelete("delete-user/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrWhiteSpace(userRole) || userRole != "Admin")
        {
            return BadRequest("Unauthorized: Only admins can delete users.");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Log the exception details here to diagnose further
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting data");
        }

        return Ok($"User with ID {id} has been deleted.");
    }

    [HttpGet("user-purchases/{userId}")]
    [Authorize] 
    public async Task<ActionResult<IEnumerable<UserPurchaseDto>>> GetUserPurchases(int userId)
    {
        
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrWhiteSpace(userRole) || userRole != "Admin")
        {
            return BadRequest("Unauthorized: Only admins can access user purchase details.");
        }

        
        var userPurchases = await _context.Purchases
            .Where(p => p.UserId == userId)
            .Include(p => p.Car) 
            .Select(p => new UserPurchaseDto
            {
                PurchaseId = p.Id,
                CarId = p.CarId,
                CarName = p.Car.CarName,
                Quantity = p.Quantity,
                PurchaseDate = p.PurchaseDate,
                CarModel = p.Car.CarModel,
                CarImage = p.Car.CarImage
            })
            .ToListAsync();

        if (userPurchases == null || !userPurchases.Any())
        {
            return NotFound("No purchases found for this user.");
        }

        return Ok(userPurchases);
    }








    private string CreateToken(User user)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
         new Claim(ClaimTypes.Role, user.IsAdmin == 1 ? "Admin" : "User"),
    };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            Issuer = _configuration["Jwt:Issuer"],     // Set the Issuer from configuration
            Audience = _configuration["Jwt:Audience"], // Set the Audience from configuration
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }


    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key; // This is your salt
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt)) // Use the stored salt
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }


        

    }

