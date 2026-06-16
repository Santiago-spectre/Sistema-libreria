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

            var hoy = DateTime.Today;
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            var statsQuery = userRole == "Vendedor"
                ? _appDbContext.Sales.Where(s => s.UserId == userId)
                : _appDbContext.Sales;

            ViewBag.TodaySales = await statsQuery
                .Where(s => s.Date.Date == hoy)
                .CountAsync();

            ViewBag.TotalSales = await statsQuery
                .Where(s => s.Date.Month == hoy.Month && s.Date.Year == hoy.Year)
                .CountAsync();

            ViewBag.AverageSales = await statsQuery
                .Where(s => s.Date.Month == hoy.Month && s.Date.Year == hoy.Year)
                .AverageAsync(s => (double?)s.Total) is double avg ? Math.Round(avg, 2) : 0;

            ViewBag.UserRole = userRole;
            ViewBag.Usuarios = await _appDbContext.Users.Where(u => u.Status).ToListAsync();

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
                    if (item.Quantity > product.Stock)
                        return BadRequest(new { mensaje = $"Stock insuficiente para '{product.Name}'. Stock disponible: {product.Stock}" });

                    product.Stock -= item.Quantity;
                }
            }

            _appDbContext.Sales.Add(sale);
            await _appDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetSaleStats(int? userId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var hoy = DateTime.Today;
            var query = _appDbContext.Sales.AsQueryable();
            if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);

            var todaySales = await query.Where(s => s.Date.Date == hoy).CountAsync();
            var totalSales = await query.Where(s => s.Date.Month == hoy.Month && s.Date.Year == hoy.Year).CountAsync();
            var averageSales = await query.Where(s => s.Date.Month == hoy.Month && s.Date.Year == hoy.Year)
                .AverageAsync(s => (double?)s.Total) is double avg ? Math.Round(avg, 2) : 0;

            return Json(new { todaySales, totalSales, averageSales });
        }
    }
}