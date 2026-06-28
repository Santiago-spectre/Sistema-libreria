using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using System.Text.Json;
using SistemaWebPapeleria.Helpers;

namespace SistemaWebPapeleria.Controllers
{
    public class SaleController : Controller
    {
        private readonly AppDbContext _context;

        public SaleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.User)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var products = await _context.Products
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
                ? _context.Sales.Where(s => s.UserId == userId)
                : _context.Sales;

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
            ViewBag.Usuarios = await _context.Users.Where(u => u.Status).ToListAsync();

            return View(sales);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaleRequestVM request)
        {
            if (request == null || request.Items ==null || request.Items.Count == 0)
                return BadRequest(new { mensaje = "Debe agregar al menos un producto a la venta." });

            if (!ModelState.IsValid)
                return BadRequest(new { mensaje = "Revisa los datos de la venta, hay campos inválidos." });

            if (request.Items.Any(i => i.Quantity <= 0))
                return BadRequest(new { mensaje = "La cantidad de cada producto debe ser mayor a cero." });

            decimal subtotalVenta = request.Items.Sum(i => i.UnitPrice * i.Quantity);
            if (request.Discount > subtotalVenta)
                return BadRequest(new { mensaje = "El descuento no puede ser mayor al total de la venta." });

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            decimal total = subtotalVenta - request.Discount;

            if (total < 0) total = 0;

            var sale = new Sale
            {
                Date = DateTimeHelper.AhoraEnPeru(),
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
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null && !product.IsService)
                {
                    if (item.Quantity > product.Stock)
                        return BadRequest(new { mensaje = $"Stock insuficiente para '{product.Name}'. Stock disponible: {product.Stock}" });

                    product.Stock -= item.Quantity;
                }
            }

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            var vendedor = await _context.Users.FindAsync(userId);
            string nombreVendedor = vendedor != null ? $"{vendedor.Name} {vendedor.LastName}" : "Usuario";

            // Notificación para el vendedor que hizo la venta
            await NotificationHelper.CrearAsync(_context, userId, "Venta Registrada", $"Registraste una venta por S/. {total:F2}.", "Venta");

            // Notificación para todos los administradores
            var admins = await _context.Users.Where(u => u.Role.RoleName == "Administrador" && u.UserId != userId).ToListAsync();

            foreach (var admin in admins)
            {
                await NotificationHelper.CrearAsync(_context, admin.UserId, "Nueva Venta registrada", $"{nombreVendedor} registró una venta por S/ {total:F2}.", "Venta");
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetSaleStats(int? userId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var hoy = DateTime.Today;
            var query = _context.Sales.AsQueryable();
            if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);

            var todaySales = await query.Where(s => s.Date.Date == hoy).CountAsync();
            var totalSales = await query.Where(s => s.Date.Month == hoy.Month && s.Date.Year == hoy.Year).CountAsync();
            var averageSales = await query.Where(s => s.Date.Month == hoy.Month && s.Date.Year == hoy.Year)
                .AverageAsync(s => (double?)s.Total) is double avg ? Math.Round(avg, 2) : 0;

            return Json(new { todaySales, totalSales, averageSales });
        }
    }
}