using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using SistemaWebPapeleria.Helpers;
using SistemaWebPapeleria.Services;

namespace SistemaWebPapeleria.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public UserController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();

            ViewBag.Roles = _context.Roles
                .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName
                }).ToList();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarCodigoVerificacion(string email)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return BadRequest(new { mensaje = "Ingresa un correo válido." });

            bool existeCorreo = await _context.Users.AnyAsync(u => u.Email.ToLower() == email.Trim().ToLower());
            if (existeCorreo)
                return BadRequest(new { mensaje = "Ya existe un usuario con ese correo." });

            var code = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("CreateUserCode", code);
            HttpContext.Session.SetString("CreateUserEmail", email.Trim().ToLower());
            HttpContext.Session.SetString("CreateUserCodeExpiry", DateTime.Now.AddMinutes(10).ToString());
            HttpContext.Session.Remove("CreateUserEmailVerified");

            var body = $@"
                <div style='font-family:Segoe UI,sans-serif; max-width:500px; margin:auto; padding:30px; background:#1a1a2e; border-radius:16px; color:#fff;'>
                    <h2 style='color:#6366f1; margin-bottom:10px;'>Papelería Sonia</h2>
                    <p style='color:rgba(255,255,255,0.7);'>Estás creando una cuenta de usuario en el sistema.</p>
                    <div style='background:rgba(99,102,241,0.15); border:1px solid rgba(99,102,241,0.3); border-radius:12px; padding:20px; text-align:center; margin:20px 0;'>
                        <p style='margin:0; color:rgba(255,255,255,0.5); font-size:13px;'>Tu código de verificación es:</p>
                        <h1 style='color:#818cf8; letter-spacing:8px; margin:10px 0;'>{code}</h1>
                        <p style='margin:0; color:rgba(255,255,255,0.4); font-size:12px;'>Válido por 10 minutos</p>
                    </div>
                    <p style='color:rgba(255,255,255,0.4); font-size:12px;'>Si no solicitaste esto, ignora este correo.</p>
                </div>";

            _emailService.SendEmail(email.Trim(), "Código de verificación - Papelería Sonia", body);

            return Ok(new { mensaje = "Código enviado correctamente." });
        }

        [HttpPost]
        public IActionResult VerificarCodigoCreacion(string code)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var savedCode = HttpContext.Session.GetString("CreateUserCode");
            var expiry = HttpContext.Session.GetString("CreateUserCodeExpiry");

            if (string.IsNullOrEmpty(savedCode) || string.IsNullOrEmpty(expiry))
                return BadRequest(new { mensaje = "Solicita un código primero." });

            if (DateTime.Parse(expiry) < DateTime.Now)
                return BadRequest(new { mensaje = "El código ha expirado. Solicita uno nuevo." });

            if (code != savedCode)
                return BadRequest(new { mensaje = "Código incorrecto." });

            HttpContext.Session.SetString("CreateUserEmailVerified", "true");
            return Ok(new { mensaje = "Correo verificado correctamente." });
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserVM model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos del usuario, hay campos inválidos.";
                return RedirectToAction("Index");
            }

            var correoVerificado = HttpContext.Session.GetString("CreateUserEmailVerified");
            var correoEnSesion = HttpContext.Session.GetString("CreateUserEmail");

            if (correoVerificado != "true" || correoEnSesion != model.Email.Trim().ToLower())
            {
                TempData["Error"] = "Debes verificar el correo antes de crear el usuario.";
                return RedirectToAction("Index");
            }

            bool existeCorreo = await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());
            if (existeCorreo)
            {
                TempData["Error"] = "Ya existe un usuario con ese correo.";
                return RedirectToAction("Index");
            }

            var user = new User
            {
                Name = model.Name.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RoleId = model.RoleId,
                Status = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await NotificarCRUD(int.Parse(HttpContext.Session.GetString("UserId") ?? "0"),
                "Usuario creado", $"Se creó el usuario '{user.Name} {user.LastName}'.");

            HttpContext.Session.Remove("CreateUserCode");
            HttpContext.Session.Remove("CreateUserEmail");
            HttpContext.Session.Remove("CreateUserCodeExpiry");
            HttpContext.Session.Remove("CreateUserEmailVerified");

            TempData["Success"] = "Usuario creado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserVM model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos del usuario, hay campos inválidos.";
                return RedirectToAction("Index");
            }

            var existing = await _context.Users.FindAsync(model.UserId);
            if (existing == null) return NotFound();

            bool existeCorreo = await _context.Users.AnyAsync(u => u.UserId != model.UserId && u.Email.ToLower() == model.Email.Trim().ToLower());
            if (existeCorreo)
            {
                TempData["Error"] = "Ya existe otro usuario con ese correo.";
                return RedirectToAction("Index");
            }

            existing.Name = model.Name.Trim();
            existing.LastName = model.LastName.Trim();
            existing.Email = model.Email.Trim();
            existing.RoleId = model.RoleId;

            if (!string.IsNullOrEmpty(model.Password))
                existing.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            await _context.SaveChangesAsync();

            await NotificarCRUD(int.Parse(HttpContext.Session.GetString("UserId") ?? "0"),
                "Usuario actualizado", $"Se actualizó el usuario '{existing.Name} {existing.LastName}'.");

            TempData["Success"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Status = !user.Status;
            await _context.SaveChangesAsync();

            string estado = user.Status ? "activado" : "desactivado";
            await NotificarCRUD(int.Parse(HttpContext.Session.GetString("UserId") ?? "0"),
                "Usuario " + estado, $"Se {estado} el usuario '{user.Name} {user.LastName}'.");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            string nombreUsuario = $"{user.Name} {user.LastName}";

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await NotificarCRUD(int.Parse(HttpContext.Session.GetString("UserId") ?? "0"),
                "Usuario eliminado", $"Se eliminó el usuario '{nombreUsuario}'.");

            return RedirectToAction("Index");
        }

        private async Task NotificarCRUD(int userId, string title, string message)
        {
            await NotificationHelper.CrearAsync(_context, userId, title, message, "Usuario");

            var admins = await _context.Users
                .Where(u => u.Role.RoleName == "Administrador" && u.UserId != userId)
                .ToListAsync();

            foreach (var admin in admins)
            {
                await NotificationHelper.CrearAsync(_context, admin.UserId, title, message, "Usuario");
            }
        }
    }
}