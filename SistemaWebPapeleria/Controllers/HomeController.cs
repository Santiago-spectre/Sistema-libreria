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
            // Ventas de la ultima semana
            var weekDays = new List<string>();
            var weekSales = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                weekDays.Add(day.ToString("ddd", new System.Globalization.CultureInfo("es-PE")));
                var total = await _appDbContext.Sales
                    .Where(s => s.Date.Date == day)
                    .SumAsync(s => (decimal?)s.Total) ?? 0;
                weekSales.Add(total);
            }

            // Ventas por metodo de pago
            var salesEfectivo = await _appDbContext.Sales.Where(s => s.PaymentMethod == "Efectivo").SumAsync(s => (decimal?)s.Total) ?? 0;
            var salesYape = await _appDbContext.Sales.Where(s => s.PaymentMethod == "Yape").SumAsync(s => (decimal?)s.Total) ?? 0;
            var salesPlin = await _appDbContext.Sales.Where(s => s.PaymentMethod == "Plin").SumAsync(s => (decimal?)s.Total) ?? 0;
            var salesTarjeta = await _appDbContext.Sales.Where(s => s.PaymentMethod == "Tarjeta").SumAsync(s => (decimal?)s.Total) ?? 0;

            // Productos con stock bajo
            var lowStock = await _appDbContext.Products
                .Where(p => !p.IsService && p.IsActive && p.Stock <= p.MinimumStock)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            // Top 5 productos mas vendidos
            var topProducts = await _appDbContext.SaleDetails
                .GroupBy(sd => sd.Product.Name)
                .Select(g => new TopProductVM
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(sd => sd.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            var model = new DashboardVM
            {
                TotalProducts = await _appDbContext.Products.CountAsync(),
                TotalSales = await _appDbContext.Sales.CountAsync(),
                TotalUsers = await _appDbContext.Users.CountAsync(),
                TodaySales = await _appDbContext.Sales.Where(s => s.Date.Date == DateTime.Today).SumAsync(s => (decimal?)s.Total) ?? 0,
                LastSales = await _appDbContext.Sales.Include(s => s.User).OrderByDescending(s => s.Date).Take(5).ToListAsync(),
                UserRole = HttpContext.Session.GetString("UserRole"),
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

            return View(model);
        }
    }
}