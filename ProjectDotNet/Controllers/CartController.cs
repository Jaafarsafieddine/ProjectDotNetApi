using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectDotNet.DataServices;
using ProjectDotNet.Models;
using ProjectDotNet.Models.InputModels;
using System.Drawing;
using System.Security.Claims;

namespace ProjectDotNet.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDataContext _context;
        private readonly IConfiguration _configuration;

        public CartController(AppDataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("cartDetails")]
        [Authorize] // Ensure this endpoint requires authentication
        public async Task<ActionResult> GetCartDetails()
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

            // Retrieve the user's cart with all related cart details and cars
            var cart = await _context.AddToCarts
                                     .Include(c => c.AddToCartDetails)
                                        .ThenInclude(cd => cd.Car)
                                     .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found for this user.");
            }

            // Map the data to a more friendly format if necessary or return directly
            var cartDetails = cart.AddToCartDetails.Select(cd => new
            {
                CarId = cd.CarId,
                CarName = cd.Car.CarName,
                Quantity = cd.Quantity,
                Price = cd.Car.CarPrice,
                Image = cd.Car.CarImage,
                TotalPrice = cd.Quantity * cd.Car.CarPrice,
            }).ToList();

            return Ok(cartDetails);
        }


        [HttpPost("addToCart")]
        /*[Authorize]*/
        public async Task<ActionResult> AddToCart(AddToCartDto addToCartDto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return Unauthorized("User must be logged in.");
            }

            // Attempt to parse the user ID outside the query
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user ID format.");
            }

            // Now use the parsed integer in your query
            var cart = await _context.AddToCarts
                                     .Include(c => c.AddToCartDetails)
                                     .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found for this user.");
            }

            // Verify if the car exists and has enough quantity available
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == addToCartDto.CarId);
            if (car == null || car.CarQuantity < addToCartDto.Quantity)
            {
                return BadRequest("Car not available or insufficient quantity.");
            }

            // Add the car to the cart or update existing quantity
            var cartDetail = cart.AddToCartDetails.FirstOrDefault(cd => cd.CarId == addToCartDto.CarId);
            if (cartDetail == null)
            {
                cart.AddToCartDetails.Add(new AddToCartDetails
                {
                    CarId = addToCartDto.CarId,
                    Quantity = addToCartDto.Quantity
                });
            }
            else
            {
                cartDetail.Quantity += addToCartDto.Quantity;
            }

            await _context.SaveChangesAsync();
            return Ok("Car added to cart successfully.");
        }



        [HttpDelete("removeFromCart")]
        [Authorize] // Ensure this endpoint requires authentication
        public async Task<ActionResult> RemoveFromCart(int carId)
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

            // Retrieve the user's cart
            var cart = await _context.AddToCarts
                                     .Include(c => c.AddToCartDetails)
                                     .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found for this user.");
            }

            // Find the cart detail that includes the car to remove
            var cartDetail = cart.AddToCartDetails.FirstOrDefault(cd => cd.CarId == carId);
            if (cartDetail == null)
            {
                return NotFound("Car not found in cart.");
            }

            // Remove the cart detail from the database
            _context.AddToCartDetails.Remove(cartDetail);
            await _context.SaveChangesAsync();

            return Ok("Car removed from cart successfully.");
        }



        [HttpPost("purchaseCart")]
        [Authorize] // Ensures that only authenticated users can access this endpoint
        public async Task<ActionResult> PurchaseCart()
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

            // Retrieve the user's cart with all related cart details and cars
            var cart = await _context.AddToCarts
                                     .Include(c => c.AddToCartDetails)
                                     .ThenInclude(cd => cd.Car)
                                     .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found for this user.");
            }

            // Process each cart item
            foreach (var detail in cart.AddToCartDetails)
            {
                var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == detail.CarId);
                if (car == null || car.CarQuantity < detail.Quantity)
                {
                    return BadRequest($"Car not available or insufficient quantity for car ID {detail.CarId}.");
                }

                // Create purchase record
                var purchase = new Purchase
                {
                    UserId = userId,
                    CarId = detail.CarId,
                    Quantity = detail.Quantity,
                    PurchaseDate = DateTime.UtcNow
                };
                _context.Purchases.Add(purchase);

                // Update car quantity
                car.CarQuantity -= detail.Quantity;
            }

            // Save all changes to the database
            await _context.SaveChangesAsync();

            // Clear the cart details after purchase
            _context.AddToCartDetails.RemoveRange(cart.AddToCartDetails);
            await _context.SaveChangesAsync();

            return Ok("Purchase successful.");
        }


    }
}
