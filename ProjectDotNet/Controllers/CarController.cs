using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectDotNet.DataServices;
using ProjectDotNet.Models;
using ProjectDotNet.Models.OutPutModels;
using System.Security.Claims;

namespace ProjectDotNet.Controllers
{
    public class CarController : Controller
    {
        private readonly AppDataContext _context;
        private readonly IConfiguration _configuration;

        public CarController(AppDataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpGet("cars")]
        [Authorize]
        public async Task<ActionResult<List<Car>>> GetAllCars()
        {
            var cars = await _context.Cars.ToListAsync();
            if (cars == null || !cars.Any())
            {
                return NotFound("No cars found.");
            }
            return Ok(cars);
        }


        [HttpGet("carById")]
        [Authorize]
        public async Task<ActionResult<Car>> GetCarById(int id)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound($"Car with ID {id} not found.");
            }
            return Ok(car);
        }


        [HttpGet("top-purchased-cars")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TopPurchasedCarDto>>> GetTopPurchasedCars()
        {
            var topCars = await _context.Purchases
                .Include(p => p.Car)  // Include Car data
                .GroupBy(p => p.CarId)
                .Select(group => new TopPurchasedCarDto
                {
                    CarId = group.Key,
                    CarName = group.First().Car.CarName,  // Assuming CarName is a property of Car
                    TotalQuantity = group.Sum(g => g.Quantity),
                    CarModel = group.First().Car.CarModel,  // Assuming CarModel is a property of Car
                    CarImage = group.First().Car.CarImage  // Assuming CarImage is a property of Car
                })
                .OrderByDescending(dto => dto.TotalQuantity)
                .Take(5)
                .ToListAsync();

            return Ok(topCars);
        }


        [HttpGet("user-purchases")]
        [Authorize] // Ensures that only authenticated users can access this endpoint
        public async Task<ActionResult<IEnumerable<UserPurchaseDto>>> GetUserPurchases()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return Unauthorized("User must be logged in.");
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user ID format.");
            }

            var userPurchases = await _context.Purchases
                .Where(p => p.UserId == userId)
                .Include(p => p.Car)  // Ensure Car details are loaded
                .Select(p => new UserPurchaseDto
                {
                    PurchaseId = p.Id,
                    CarId = p.CarId,
                    CarName = p.Car.CarName,  // Assuming CarName is a property of Car
                    Quantity = p.Quantity,
                    PurchaseDate = p.PurchaseDate,
                    CarModel = p.Car.CarModel,  // Assuming CarModel is a property of Car
                    CarImage = p.Car.CarImage  // Assuming CarImage is a property of Car
                })
                .ToListAsync();

            return Ok(userPurchases);
        }


        [HttpPost("create-car")]
        [Authorize] 
        public async Task<IActionResult> CreateCar([FromBody] Car newCar)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(userRole) || userRole != "Admin")
            {
                return BadRequest("Unauthorized: Only admins can create cars.");
            }

            if (newCar == null)
            {
                return BadRequest("Car data must be provided.");
            }

            _context.Cars.Add(newCar);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCarById), new { id = newCar.Id }, newCar);
        }





        [HttpPut("update-car/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car carUpdate)
        {
            if (carUpdate == null)
            {
                return BadRequest("Car update data must be provided.");
            }

            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);
            if (car == null)
            {
                return NotFound($"Car with ID {id} not found.");
            }

            car.CarName = carUpdate.CarName;
            car.CarModel = carUpdate.CarModel;
            // Add other fields as necessary

            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
            return NoContent();
        }




        [HttpDelete("delete-car/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound($"Car with ID {id} not found.");
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return NoContent();
        }




    }
}
