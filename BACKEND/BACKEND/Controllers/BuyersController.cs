using BACKEND.Data;
using BACKEND.Data.DTOs.Buyers;
using BACKEND.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BACKEND.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BuyersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL BUYERS
        // GET: api/Buyers
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetAllBuyers()
        {
            var buyers = await _context.Buyers
                .Select(x => new BuyerDto
                {
                    Id = x.Id,
                    PartyName = x.PartyName,
                    Gstin = x.Gstin,
                    Mobile = x.Mobile,
                    Email = x.Email,
                    BillingAddress = x.BillingAddress,
                    State = x.State,
                    City = x.City,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(buyers);
        }
    }
}
