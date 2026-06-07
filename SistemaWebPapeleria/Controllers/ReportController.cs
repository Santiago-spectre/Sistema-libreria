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
            var hoy = DateTime.Today;

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

            // Meses con ventas para informes mensuales
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

            // ── Reporte del día ──
            if (userRole == "Administrador")
            {
                var usuarios = await _appDbContext.Users
                    .Where(u => u.Status)
                    .ToListAsync();

                var reporteHoy = usuarios.Select(u => new
                {
                    UserId = u.UserId,
                    NombreCompleto = u.Name + " " + u.LastName,
                    TotalVentas = sales.Where(s => s.UserId == u.UserId && s.Date.Date == hoy).Sum(s => s.Total),
                    NumVentas = sales.Count(s => s.UserId == u.UserId && s.Date.Date == hoy)
                }).ToList();

                // Cajas cerradas hoy por usuario
                var cajasCerradasHoy = await _appDbContext.CashClosings
                    .Where(c => c.Date.Date == hoy && (c.ClosingAmount != 0 || c.TotalSales != 0))
                    .Select(c => c.UserId)
                    .ToListAsync();

                ViewBag.CajasCerradasHoy = cajasCerradasHoy;
                ViewBag.ReporteHoy = reporteHoy;

                // Historial últimos 7 días por usuario
                var cajasUltimos7 = await _appDbContext.CashClosings
                    .Include(c => c.User)
                    .Where(c => c.Date.Date >= hoy.AddDays(-6) && c.Date.Date <= hoy && (c.ClosingAmount != 0 || c.TotalSales != 0))
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();

                ViewBag.Historial7 = cajasUltimos7;
            }
            else
            {
                var reporteHoy = new[]
                {
                    new
                    {
                        UserId = userId,
                        NombreCompleto = "",
                        TotalVentas = sales.Where(s => s.Date.Date == hoy).Sum(s => s.Total),
                        NumVentas = sales.Count(s => s.Date.Date == hoy)
                    }
                }.ToList();

                var cajaCerradaHoy = await _appDbContext.CashClosings
                    .AnyAsync(c => c.UserId == userId && c.Date.Date == hoy && (c.ClosingAmount != 0 || c.TotalSales != 0));

                ViewBag.CajaCerradaHoy = cajaCerradaHoy;
                ViewBag.ReporteHoy = reporteHoy;

                var cajasUltimos7Vendedor = await _appDbContext.CashClosings
                    .Include(c => c.User)
                    .Where(c => c.UserId == userId && c.Date.Date >= hoy.AddDays(-6) && c.Date.Date <= hoy && (c.ClosingAmount != 0 || c.TotalSales != 0))
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();

                ViewBag.Historial7 = cajasUltimos7Vendedor;
            }

            return View(sales);
        }
    }
}
