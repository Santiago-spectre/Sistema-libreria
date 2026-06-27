using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.ViewModels;

namespace SistemaWebPapeleria.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        // Devuelve los datos del usuario logueado (para llenar el modal)
        [HttpGet]
        public async Task<IActionResult> GetMisDatos()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            return Json(new
            {
                user.Name,
                user.LastName,
                user.Email
            });
        }

        // Guarda los cambios del perfil del usuario logueado
        [HttpPost]
        public async Task<IActionResult> Edit(ProfileVM model)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            if (!ModelState.IsValid)
                return BadRequest(new { mensaje = "Revisa los datos ingresados, hay campos inválidos." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            bool existeCorreo = await _context.Users.AnyAsync(u => u.UserId != userId && u.Email.ToLower() == model.Email.Trim().ToLower());
            if (existeCorreo)
                return BadRequest(new { mensaje = "Ya existe otro usuario con ese correo." });

            user.Name = model.Name.Trim();
            user.LastName = model.LastName.Trim();
            user.Email = model.Email.Trim();

            if (!string.IsNullOrWhiteSpace(model.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            await _context.SaveChangesAsync();

            // Actualizar el nombre en sesión para que se refleje al instante en la barra superior
            HttpContext.Session.SetString("UserName", $"{user.Name} {user.LastName}");

            return Ok(new { mensaje = "Perfil actualizado correctamente." });
        }
    }
}