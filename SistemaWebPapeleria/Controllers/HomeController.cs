using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.ViewModels;
using System.Diagnostics;

namespace SistemaWebPapeleria.Controllers
{
    public class HomeController : Controller
    {
        //conexion con la base de datos
        private readonly AppDbContext _appDbContext;
        public HomeController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        //Muestra el dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new DashboardVM
            {
                //Total de productos del sistema
                TotalProducts = await _appDbContext.Products.CountAsync(),

                //total de ventas registradas
                TotalSales = await _appDbContext.Sales.CountAsync(),

                //Total de usuarios registrados
                TotalUsers = await _appDbContext.Users.CountAsync(),

                //suma del total de ventas del dia
                TodaySales = await _appDbContext.Sales.Where(s => s.Date.Date == DateTime.Today).SumAsync(s => s.Total),

                LastSales = await _appDbContext.Sales.Include(s => s.User).OrderByDescending(s => s.Date).Take(5).ToListAsync(),

                // Rol y nombre del usuario logueado desde la sesion
                UserRole = HttpContext.Session.GetString("UserRole"),
                UserName = HttpContext.Session.GetString("UserName"),
            };

            return View(model);
        }
    }
}
