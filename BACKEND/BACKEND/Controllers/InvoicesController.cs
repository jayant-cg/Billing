using BACKEND.Data;
using BACKEND.Data.DTOs.Invoices;
using BACKEND.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using System.Text;
using PuppeteerSharp.Media;

namespace BACKEND.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====================================
        // GET ALL INVOICES
        // ====================================
        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var invoices = await _context.Invoices
                .Include(x => x.Buyer)
                .Select(x => new InvoiceDto
                {
                    Id = x.Id,
                    InvoiceNo = x.InvoiceNo,
                    BuyerId = x.BuyerId,
                    BuyerName = x.Buyer.PartyName,
                    InvoiceDate = x.InvoiceDate,
                    Subtotal = x.Subtotal,
                    GstAmount = x.GstAmount,
                    TotalAmount = x.TotalAmount,
                    Status = x.Status
                })
                .ToListAsync();

            return Ok(invoices);
        }

        // ====================================
        // GET INVOICE BY ID
        // ====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(x => x.Buyer)
                .Where(x => x.Id == id)
                .Select(x => new InvoiceDto
                {
                    Id = x.Id,
                    InvoiceNo = x.InvoiceNo,
                    BuyerId = x.BuyerId,
                    BuyerName = x.Buyer.PartyName,
                    InvoiceDate = x.InvoiceDate,
                    Subtotal = x.Subtotal,
                    GstAmount = x.GstAmount,
                    TotalAmount = x.TotalAmount,
                    Status = x.Status
                })
                .FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound("Invoice not found.");

            return Ok(invoice);
        }

        // ====================================
        // CREATE INVOICE
        // ====================================
        [HttpPost]
        public async Task<IActionResult> CreateInvoice(CreateInvoiceDto dto)
        {
            var invoice = new Invoice
            {
                InvoiceNo = $"INV-{DateTime.Now.Ticks}",
                BuyerId = dto.BuyerId,
                InvoiceDate = DateOnly.FromDateTime(DateTime.Now),
                Subtotal = dto.Subtotal,
                GstAmount = dto.GstAmount,
                TotalAmount = dto.TotalAmount,
                Status = "Draft",
                CreatedAt = DateTime.Now
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(invoice);
        }

        // ====================================
        // UPDATE INVOICE
        // ====================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, UpdateInvoiceDto dto)
        {
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
                return NotFound("Invoice not found.");

            invoice.Subtotal = dto.Subtotal;
            invoice.GstAmount = dto.GstAmount;
            invoice.TotalAmount = dto.TotalAmount;
            invoice.Status = dto.Status;
            invoice.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok("Invoice updated successfully.");
        }

        // ====================================
        // DELETE INVOICE
        // ====================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
                return NotFound("Invoice not found.");

            var invoiceItems = await _context.InvoiceItems
                .Where(x => x.InvoiceId == id)
                .ToListAsync();

            _context.InvoiceItems.RemoveRange(invoiceItems);
            _context.Invoices.Remove(invoice);

            await _context.SaveChangesAsync();

            return Ok("Invoice deleted successfully.");
        }

        // ====================================
        // GENERATE INVOICE
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
{
    var invoice = await _context.Invoices
        .Include(i => i.InvoiceItems)
            .ThenInclude(ii => ii.Product)
        .Include(i => i.Buyer)
        .FirstOrDefaultAsync(i => i.Id == id);

    if (invoice == null)
        return NotFound();
var html = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>

<style>

@page {{
    size: A4;
    margin: 10mm;
}}

body {{
    margin:0;
    padding:0;
    font-family: Arial, Helvetica, sans-serif;
    color:#000;
    font-size:12px;
}}

.invoice-page {{
    width:190mm;
    min-height:277mm;
    margin:auto;
    box-sizing:border-box;
}}

.header {{
    display:flex;
    justify-content:space-between;
    align-items:flex-start;
    margin-bottom:25px;
}}

.company-name {{
    font-size:24px;
    font-weight:bold;
}}

.company-details {{
    line-height:1.5;
}}

.invoice-meta {{
    text-align:right;
}}

.invoice-meta h1 {{
    margin:0;
    color:#777;
    font-size:42px;
    letter-spacing:1px;
}}

.address-row {{
    display:flex;
    justify-content:space-between;
    margin-top:25px;
    margin-bottom:25px;
}}

.address-box {{
    width:46%;
}}

.address-box h3 {{
    margin-bottom:8px;
    font-size:15px;
}}

.info-table,
.items-table {{
    width:100%;
    border-collapse:collapse;
}}

.info-table th,
.info-table td,
.items-table th,
.items-table td {{
    border:1px solid #444;
    padding:8px;
}}

.info-table {{
    margin-bottom:25px;
}}

.info-table th {{
    background:#efefef;
}}

.items-table th {{
    background:#efefef;
}}

.items-table td {{
    height:30px;
}}

.items-table th:nth-child(1) {{
    width:8%;
}}

.items-table th:nth-child(3),
.items-table th:nth-child(4) {{
    width:15%;
}}

.total-section {{
    width:320px;
    margin-left:auto;
    margin-top:15px;
}}

.total-section table {{
    width:100%;
    border-collapse:collapse;
}}

.total-section td {{
    border:1px solid #444;
    padding:8px;
}}

.grand-total {{
    font-size:16px;
    font-weight:bold;
}}

.footer-box {{
    margin-top:40px;
    border:1px solid #777;
    padding:15px;
    line-height:1.7;
}}

.signature-section {{
    margin-top:30px;
    display:flex;
    justify-content:space-between;
}}

.signature-box {{
    width:250px;
    text-align:center;
}}

.signature-line {{
    border-top:1px solid #000;
    margin-top:50px;
    padding-top:5px;
}}

</style>

</head>

<body>

<div class='invoice-page'>

    <div class='header'>

        <div class='company-details'>

            <div class='company-name'>
                CG Infinity Sanitary & Bath Solutions
            </div>

            <br>

            101 Business Tower<br>
            Connaught Place<br>
            New Delhi - 110001<br><br>

            GSTIN: 07ABCDE1234F1Z5<br>
            Phone: +91-9876543210<br>
            Email: billing@cginfinity.com

        </div>

        <div class='invoice-meta'>

            <h1>INVOICE</h1>

            <br>

            <strong>Invoice #</strong><br>
            {invoice.InvoiceNo}

            <br><br>

            <strong>Date</strong><br>
            {invoice.InvoiceDate:dd-MM-yyyy}

        </div>

    </div>

    <div class='address-row'>

        <div class='address-box'>

            <h3>Bill To</h3>

            <strong>{invoice.Buyer.PartyName}</strong><br>

            {invoice.Buyer.BillingAddress}<br>

            {invoice.Buyer.City}<br>

            Mobile: {invoice.Buyer.Mobile}<br>

            GSTIN: {invoice.Buyer.Gstin}

        </div>

        <div class='address-box'>

            <h3>Ship To</h3>

            <strong>{invoice.Buyer.PartyName}</strong><br>

            {invoice.Buyer.BillingAddress}<br>

            {invoice.Buyer.City}<br>

            Mobile: {invoice.Buyer.Mobile}

        </div>

    </div>

    <table class='info-table'>

        <tr>
            <th>Salesperson</th>
            <th>P.O. Number</th>
            <th>Requisitioner</th>
            <th>Shipped Via</th>
            <th>Terms</th>
        </tr>

        <tr>
            <td>-</td>
            <td>-</td>
            <td>-</td>
            <td>-</td>
            <td>Immediate</td>
        </tr>

    </table>

    <table class='items-table'>

        <thead>

            <tr>
                <th>Qty</th>
                <th>Description</th>
                <th>Unit Price</th>
                <th>Total</th>
            </tr>

        </thead>

        <tbody>

        {string.Join("", invoice.InvoiceItems.Select(item => $@"
            <tr>
                <td>{item.Qty}</td>
                <td>{item.Product.ModelName}</td>
                <td>₹{item.Product.DefaultPrice:N2}</td>
                <td>₹{(item.Product.DefaultPrice * item.Qty):N2}</td>
            </tr>
        "))}

        </tbody>

    </table>

    <div class='total-section'>

        <table>

            <tr>
                <td>Subtotal</td>
                <td>₹{invoice.Subtotal:N2}</td>
            </tr>

            <tr>
                <td>GST</td>
                <td>₹{invoice.GstAmount:N2}</td>
            </tr>

            <tr class='grand-total'>
                <td>Total Due</td>
                <td>₹{invoice.TotalAmount:N2}</td>
            </tr>

        </table>

    </div>

    <div class='signature-section'>

        <div class='signature-box'>

            <div class='signature-line'>
                Customer Signature
            </div>

        </div>

        <div class='signature-box'>

            <div style='height:70px'></div>

            <div class='signature-line'>
                Authorized Signatory
            </div>

        </div>

    </div>

    <div class='footer-box'>

        Make all payments payable to
        <strong>CG Infinity Sanitary & Bath Solutions</strong>.

        <br>

        Payment is due within 30 days.

        <br>

        For invoice related queries contact:
        billing@cginfinity.com | +91-9876543210

        <br><br>

        <strong>Thank you for your business!</strong>

    </div>

</div>

</body>
</html>
";
    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true,
        ExecutablePath =
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
    });

    var page = await browser.NewPageAsync();

    await page.SetContentAsync(html);

    var pdfBytes = await page.PdfDataAsync(new PdfOptions
    {
        Width = "210mm",
        Height = "297mm",
        PrintBackground = true
    });

    await browser.CloseAsync();

    return File(
        pdfBytes,
        "application/pdf",
        $"Invoice_{invoice.InvoiceNo}.pdf");
}
    }
}
