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



    }
}
