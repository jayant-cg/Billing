using BACKEND.Data;
using BACKEND.Data.DTOs.InvoiceItems;
using BACKEND.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BACKEND.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InvoiceItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================================
        // CREATE INVOICE ITEM
        // POST: api/InvoiceItems
        // ======================================
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateInvoiceItemDto dto)
        {
            decimal amount = dto.Qty * dto.Rate;

            decimal gstAmount =
                amount * dto.GstRate / 100;

            decimal totalAmount =
                amount + gstAmount;

            var item = new InvoiceItem
            {
                InvoiceId = dto.InvoiceId,
                ProductId = dto.ProductId,
                Qty = dto.Qty,
                Rate = dto.Rate,
                Amount = amount,
                GstRate = dto.GstRate,
                GstAmount = gstAmount,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.Now
            };

            _context.InvoiceItems.Add(item);

            await _context.SaveChangesAsync();

            return Ok(item);
        }

    }
}
