using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext _appDbContext;
        public ReportController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            // Si es vendedor, solo ve sus propias ventas
            List<Sale> sales;
            if (userRole == "Vendedor")
            {
                sales = await _appDbContext.Sales
                    .Include(s => s.User)
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.Date)
                    .ToListAsync();
            }
            else
            {
                sales = await _appDbContext.Sales
                    .Include(s => s.User)
                    .OrderByDescending(s => s.Date)
                    .ToListAsync();
            }

            // Lista de vendedores para el filtro (solo admin)
            ViewBag.Users = await _appDbContext.Users
                .Where(u => u.Status)
                .ToListAsync();

            //Meses con ventas para informes mensuales
            var monthlyGroups = sales
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new System.Globalization.CultureInfo("es-PE").DateTimeFormat.GetMonthName(g.Key.Month),
                    Total = g.Sum(s => s.Total),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Year)
                .ThenByDescending(g => g.Month)
                .ToList();

            ViewBag.MonthlyGroups = monthlyGroups;

            return View(sales);
        }
    }
}
