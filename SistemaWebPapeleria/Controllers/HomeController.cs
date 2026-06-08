using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.ViewModels;

namespace SistemaWebPapeleria.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _appDbContext;
        public HomeController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var esVendedor = userRole == "Vendedor";

            // Ventas de la ultima semana
            var weekDays = new List<string>();
            var weekSales = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                weekDays.Add(day.ToString("ddd", new System.Globalization.CultureInfo("es-PE")));
                var q = _appDbContext.Sales.Where(s => s.Date.Date == day);
                if (esVendedor) q = q.Where(s => s.UserId == userId);
                var total = await q.SumAsync(s => (decimal?)s.Total) ?? 0;
                weekSales.Add(total);
            }

            // Base query según rol
            var baseQuery = esVendedor
                ? _appDbContext.Sales.Where(s => s.UserId == userId)
                : _appDbContext.Sales;

            // Ventas por metodo de pago
            var salesEfectivo = await baseQuery.Where(s => s.PaymentMethod == "Efectivo").SumAsync(s => (decimal?)s.Total) ?? 0;
            var salesYape = await baseQuery.Where(s => s.PaymentMethod == "Yape").SumAsync(s => (decimal?)s.Total) ?? 0;
            var salesPlin = await baseQuery.Where(s => s.PaymentMethod == "Plin").SumAsync(s => (decimal?)s.Total) ?? 0;
            var salesTarjeta = await baseQuery.Where(s => s.PaymentMethod == "Tarjeta").SumAsync(s => (decimal?)s.Total) ?? 0;

            // Productos con stock bajo
            var lowStock = await _appDbContext.Products
                .Where(p => !p.IsService && p.IsActive && p.Stock <= p.MinimumStock)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            // Top 5 productos mas vendidos
            var topQuery = _appDbContext.SaleDetails.AsQueryable();
            if (esVendedor) topQuery = topQuery.Where(sd => sd.Sale.UserId == userId);

            var topProducts = await topQuery
                .GroupBy(sd => sd.Product.Name)
                .Select(g => new TopProductVM
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(sd => sd.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            // Ultimas ventas
            var lastSalesQuery = _appDbContext.Sales.Include(s => s.User).AsQueryable();
            if (esVendedor) lastSalesQuery = lastSalesQuery.Where(s => s.UserId == userId);

            var model = new DashboardVM
            {
                TotalProducts = await _appDbContext.Products.CountAsync(),
                TotalSales = await baseQuery.CountAsync(),
                TotalUsers = await _appDbContext.Users.CountAsync(),
                TodaySales = await baseQuery.Where(s => s.Date.Date == DateTime.Today).SumAsync(s => (decimal?)s.Total) ?? 0,
                LastSales = await lastSalesQuery.OrderByDescending(s => s.Date).Take(5).ToListAsync(),
                UserRole = userRole,
                UserName = HttpContext.Session.GetString("UserName"),
                WeekDays = weekDays,
                WeekSales = weekSales,
                SalesEfectivo = salesEfectivo,
                SalesYape = salesYape,
                SalesPlin = salesPlin,
                SalesTarjeta = salesTarjeta,
                LowStockProducts = lowStock,
                TopProducts = topProducts
            };

            ViewBag.Usuarios = await _appDbContext.Users.Where(u => u.Status).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(int? userId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var currentUserId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            // Solo admin puede filtrar
            if (userRole != "Administrador") return Forbid();

            var query = _appDbContext.Sales.AsQueryable();
            if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);

            // Ventas última semana
            var weekDays = new List<string>();
            var weekSales = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                weekDays.Add(day.ToString("ddd", new System.Globalization.CultureInfo("es-PE")));
                var total = await query.Where(s => s.Date.Date == day).SumAsync(s => (decimal?)s.Total) ?? 0;
                weekSales.Add(total);
            }

            // Ventas por método de pago
            var efectivo = await query.Where(s => s.PaymentMethod == "Efectivo").SumAsync(s => (decimal?)s.Total) ?? 0;
            var yape = await query.Where(s => s.PaymentMethod == "Yape").SumAsync(s => (decimal?)s.Total) ?? 0;
            var plin = await query.Where(s => s.PaymentMethod == "Plin").SumAsync(s => (decimal?)s.Total) ?? 0;
            var tarjeta = await query.Where(s => s.PaymentMethod == "Tarjeta").SumAsync(s => (decimal?)s.Total) ?? 0;

            // Últimas ventas
            var lastSales = await query
                .Include(s => s.User)
                .OrderByDescending(s => s.Date)
                .Take(5)
                .Select(s => new {
                    s.SaleId,
                    Fecha = s.Date.ToString("dd/MM/yyyy HH:mm"),
                    Vendedor = s.User.Name + " " + s.User.LastName,
                    s.PaymentMethod,
                    s.Total,
                    s.ReceiptIssued
                })
                .ToListAsync();

            return Json(new
            {
                weekDays,
                weekSales,
                efectivo,
                yape,
                plin,
                tarjeta,
                lastSales
            });
        }
    }
}