using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.Controllers
{
    public class CashClosingController : Controller
    {
        private readonly AppDbContext _context;

        public CashClosingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Administrador ve todas las cajas
            if (userRole == "Administrador")
            {
                var allCajas = await _context.CashClosings
                    .Include(c => c.User)
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();
                return View(allCajas);
            }

            // Vendedor solo ve sus propias cajas
            var misCajas = await _context.CashClosings
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            return View(misCajas);
        }

        [HttpPost]
        public async Task<IActionResult> Open(decimal initialAmount)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            // Verificar si ya tiene una caja abierta
            var cajaAbierta = await _context.CashClosings
                .AnyAsync(c => c.UserId == userId && c.ClosingAmount == 0 && c.TotalSales == 0);

            if (cajaAbierta)
            {
                TempData["Error"] = "Ya tienes una caja abierta.";
                return RedirectToAction("Index");
            }

            var caja = new CashClosing
            {
                Date = DateTime.Now,
                InitialAmount = initialAmount,
                TotalCash = 0,
                TotalYape = 0,
                TotalPlin = 0,
                TotalCard = 0,
                TotalSales = 0,
                ClosingAmount = 0,
                UserId = userId
            };

            _context.CashClosings.Add(caja);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Close(int id)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var userRole = HttpContext.Session.GetString("UserRole");

            var caja = await _context.CashClosings.FindAsync(id);
            if (caja == null) return NotFound();

            // Solo el dueño o el administrador pueden cerrar
            if (caja.UserId != userId && userRole != "Administrador")
                return Forbid();

            // Calcular totales del día desde las ventas
            var fechaApertura = caja.Date;
            var ventas = await _context.Sales
                .Where(s => s.UserId == caja.UserId && s.Date >= fechaApertura)
                .ToListAsync();

            caja.TotalCash = ventas.Where(s => s.PaymentMethod == "Efectivo").Sum(s => s.Total);
            caja.TotalYape = ventas.Where(s => s.PaymentMethod == "Yape").Sum(s => s.Total);
            caja.TotalPlin = ventas.Where(s => s.PaymentMethod == "Plin").Sum(s => s.Total);
            caja.TotalCard = ventas.Where(s => s.PaymentMethod == "Tarjeta").Sum(s => s.Total);
            caja.TotalSales = ventas.Sum(s => s.Total);
            caja.ClosingAmount = caja.InitialAmount + caja.TotalSales;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var caja = await _context.CashClosings.FindAsync(id);
            if (caja == null) return NotFound();

            _context.CashClosings.Remove(caja);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}