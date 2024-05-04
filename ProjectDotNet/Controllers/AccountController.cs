using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using ProjectDotNet.DataServices;
using ProjectDotNet.Models;
using ProjectDotNet.Models.InputModels;
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
            PhoneNumber = registerDto.PhoneNumber
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


    [HttpGet("getUserById")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users
            .AsNoTracking()  // Use AsNoTracking for read-only operations for better performance
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber
            })
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }



    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
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

