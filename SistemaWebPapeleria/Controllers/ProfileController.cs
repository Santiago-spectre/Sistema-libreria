using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.ViewModels;
using SistemaWebPapeleria.Services;

namespace SistemaWebPapeleria.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public ProfileController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

        [HttpPost]
        public async Task<IActionResult> EnviarCodigoVerificacion(string email)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return BadRequest(new { mensaje = "Ingresa un correo válido." });

            bool existeCorreo = await _context.Users.AnyAsync(u => u.UserId != userId && u.Email.ToLower() == email.Trim().ToLower());
            if (existeCorreo)
                return BadRequest(new { mensaje = "Ya existe otro usuario con ese correo." });

            var code = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("ProfileCode", code);
            HttpContext.Session.SetString("ProfileEmail", email.Trim().ToLower());
            HttpContext.Session.SetString("ProfileCodeExpiry", DateTime.Now.AddMinutes(10).ToString());
            HttpContext.Session.Remove("ProfileEmailVerified");

            var body = $@"
                <div style='font-family:Segoe UI,sans-serif; max-width:500px; margin:auto; padding:30px; background:#1a1a2e; border-radius:16px; color:#fff;'>
                    <h2 style='color:#6366f1; margin-bottom:10px;'>Papelería Sonia</h2>
                    <p style='color:rgba(255,255,255,0.7);'>Estás actualizando el correo de tu perfil.</p>
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
        public IActionResult VerificarCodigo(string code)
        {
            var savedCode = HttpContext.Session.GetString("ProfileCode");
            var expiry = HttpContext.Session.GetString("ProfileCodeExpiry");

            if (string.IsNullOrEmpty(savedCode) || string.IsNullOrEmpty(expiry))
                return BadRequest(new { mensaje = "Solicita un código primero." });

            if (DateTime.Parse(expiry) < DateTime.Now)
                return BadRequest(new { mensaje = "El código ha expirado. Solicita uno nuevo." });

            if (code != savedCode)
                return BadRequest(new { mensaje = "Código incorrecto." });

            HttpContext.Session.SetString("ProfileEmailVerified", "true");
            return Ok(new { mensaje = "Correo verificado correctamente." });
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

            bool correoCambio = user.Email.Trim().ToLower() != model.Email.Trim().ToLower();

            if (correoCambio)
            {
                var correoVerificado = HttpContext.Session.GetString("ProfileEmailVerified");
                var correoEnSesion = HttpContext.Session.GetString("ProfileEmail");

                if (correoVerificado != "true" || correoEnSesion != model.Email.Trim().ToLower())
                    return BadRequest(new { mensaje = "Debes verificar el nuevo correo antes de guardar los cambios." });
            }

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

            HttpContext.Session.Remove("ProfileCode");
            HttpContext.Session.Remove("ProfileEmail");
            HttpContext.Session.Remove("ProfileCodeExpiry");
            HttpContext.Session.Remove("ProfileEmailVerified");

            return Ok(new { mensaje = "Perfil actualizado correctamente." });
        }
    }
}