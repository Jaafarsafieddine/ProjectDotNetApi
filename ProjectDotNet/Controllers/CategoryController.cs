using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectDotNet.DataServices;
using ProjectDotNet.Models;
using ProjectDotNet.Models.InputModels;
using ProjectDotNet.Models.OutPutModels;
using System.Security.Claims;

namespace ProjectDotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDataContext _context;
        private readonly IConfiguration _configuration;

        public CategoryController(AppDataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<OutcategoryDto>>> GetAllCategories()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(userRole) || userRole != "Admin")
            {
                return BadRequest("Unauthorized: Only admins can create cars.");
            }
            
            var categories = await _context.Categories
                .Select(c => new OutcategoryDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();

            if (!categories.Any())
            {
                return NotFound("No categories found.");
            }
            return Ok(categories);
        }

        [HttpPost("create-category")]
        public async Task<IActionResult> CreateCategory([FromBody] categoryDto newCategory)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(userRole) || userRole != "Admin")
            {
                return BadRequest("Unauthorized: Only admins can create cars.");
            }
            if (newCategory == null)
            {
                return BadRequest("Category data must be provided.");
            }
            var category = new Category {
            Id = newCategory.Id,
            CategoryName= newCategory.CategoryName,
            Cars = []
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, newCategory);
        }

        [HttpGet("category/{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(userRole) || userRole != "Admin")
            {
                return BadRequest("Unauthorized: Only admins can create cars.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            return Ok(category);
        }

        [HttpDelete("delete-category/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(userRole) || userRole != "Admin")
            {
                return BadRequest("Unauthorized: Only admins can create cars.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent(); // HTTP 204
        }
    }
}

