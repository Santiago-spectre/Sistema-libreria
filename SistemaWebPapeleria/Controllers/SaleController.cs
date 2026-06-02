using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using System.Text.Json;

namespace SistemaWebPapeleria.Controllers
{
    public class SaleController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public SaleController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sales = await _appDbContext.Sales
                .Include(s => s.User)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var products = await _appDbContext.Products
                .Where(p => p.IsActive)
                .Select(p => new {
                    p.ProductId,
                    p.Name,
                    p.SalePrice,
                    p.IsActive,
                    p.IsService,
                    p.Stock
                })
                .ToListAsync();

            ViewBag.ProductosJson = JsonSerializer.Serialize(products, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return View(sales);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaleRequestVM request)
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
                return BadRequest();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            decimal total = request.Items.Sum(i => i.UnitPrice * i.Quantity) - request.Discount;
            if (total < 0) total = 0;

            var sale = new Sale
            {
                Date = DateTime.Now,
                PaymentMethod = request.PaymentMethod,
                Discount = request.Discount,
                ReceiptIssued = request.ReceiptIssued,
                Total = total,
                UserId = userId,
                SaleDetails = request.Items.Select(i => new SaleDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.UnitPrice * i.Quantity
                }).ToList()
            };

            // Descontar stock
            foreach (var item in request.Items)
            {
                var product = await _appDbContext.Products.FindAsync(item.ProductId);
                if (product != null && !product.IsService)
                {
                    product.Stock -= item.Quantity;
                }
            }

            _appDbContext.Sales.Add(sale);
            await _appDbContext.SaveChangesAsync();

            return Ok();
        }
    }
}