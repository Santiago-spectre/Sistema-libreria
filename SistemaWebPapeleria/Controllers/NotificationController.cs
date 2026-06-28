using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;

namespace SistemaWebPapeleria.Controllers
{
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        // Devuelve las últimas notificaciones del usuario logueado (para la campanita)
        [HttpGet]
        public async Task<IActionResult> GetRecientes()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var userRole = HttpContext.Session.GetString("UserRole");

            var query = _context.Notifications.AsQueryable();

            // Administrador ve todas las notificaciones; Vendedor solo las suyas
            if (userRole != "Administrador")
                query = query.Where(n => n.UserId == userId);

            var notificaciones = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.IsRead,
                    n.CreatedAt
                })
                .ToListAsync();

            int noLeidas = userRole == "Administrador"
                ? await _context.Notifications.CountAsync(n => !n.IsRead)
                : await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

            return Json(new { notificaciones, noLeidas });
        }

        // Marca una notificación específica como leída
        [HttpPost]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Marca todas las notificaciones visibles para el usuario actual como leídas
        [HttpPost]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var userRole = HttpContext.Session.GetString("UserRole");

            var query = _context.Notifications.Where(n => !n.IsRead);

            if (userRole != "Administrador")
                query = query.Where(n => n.UserId == userId);

            var noLeidas = await query.ToListAsync();
            foreach (var n in noLeidas)
                n.IsRead = true;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // Elimina una notificación de la base de datos
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Elimina todas las notificaciones visibles para el usuario actual
        [HttpPost]
        public async Task<IActionResult> EliminarTodas()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var userRole = HttpContext.Session.GetString("UserRole");

            var query = _context.Notifications.AsQueryable();

            if (userRole != "Administrador")
                query = query.Where(n => n.UserId == userId);

            var notificaciones = await query.ToListAsync();
            _context.Notifications.RemoveRange(notificaciones);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}