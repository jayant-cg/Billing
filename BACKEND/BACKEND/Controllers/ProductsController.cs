using BACKEND.Data;
using BACKEND.Data.DTOs.Products;
using BACKEND.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BACKEND.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET PRODUCTS BY CATEGORY
        // GET: api/Products/category/1
        // ==========================================
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _context.Products
                .Where(x => x.CategoryId == categoryId)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    ModelName = x.ModelName,
                    DefaultPrice = x.DefaultPrice,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(products);
        }
    }
}