using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using SistemaWebPapeleria.Services;

namespace SistemaWebPapeleria.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        public LoginController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            User? usuario_encontrado = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Email == model.Email)
                .FirstOrDefaultAsync();

            if (usuario_encontrado == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario_encontrado.Password))
            {
                ViewData["Mensaje"] = "Correo o contraseña incorrectos";
                return View();
            }

            HttpContext.Session.SetString("UserId", usuario_encontrado.UserId.ToString());
            HttpContext.Session.SetString("UserName", usuario_encontrado.Name + " " + usuario_encontrado.LastName);
            HttpContext.Session.SetString("UserRole", usuario_encontrado.Role.RoleName);

            TempData["Bienvenida"] = usuario_encontrado.Role.RoleName;

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendCode(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorRecover"] = "No existe ninguna cuenta con ese correo.";
                return RedirectToAction("RecoverPassword");
            }

            //genera codigo 6 digitos
            var code = new Random().Next(100000, 999999).ToString();

            // Guardar en sesión junto con el correo
            HttpContext.Session.SetString("RecoverCode", code);
            HttpContext.Session.SetString("RecoverEmail", email);
            HttpContext.Session.SetString("RecoverCodeExpiry", DateTime.Now.AddMinutes(10).ToString());

            //Enviar correo
            var body = $@"
                <div style='font-family:Segoe UI,sans-serif; max-width:500px; margin:auto; padding:30px; background:#1a1a2e; border-radius:16px; color:#fff;'>
                    <h2 style='color:#6366f1; margin-bottom:10px;'>Papelería Sonia</h2>
                    <p style='color:rgba(255,255,255,0.7);'>Recibimos una solicitud para restablecer tu contraseña.</p>
                    <div style='background:rgba(99,102,241,0.15); border:1px solid rgba(99,102,241,0.3); border-radius:12px; padding:20px; text-align:center; margin:20px 0;'>
                        <p style='margin:0; color:rgba(255,255,255,0.5); font-size:13px;'>Tu código de verificación es:</p>
                        <h1 style='color:#818cf8; letter-spacing:8px; margin:10px 0;'>{code}</h1>
                        <p style='margin:0; color:rgba(255,255,255,0.4); font-size:12px;'>Válido por 10 minutos</p>
                    </div>
                    <p style='color:rgba(255,255,255,0.4); font-size:12px;'>Si no solicitaste esto, ignora este correo.</p>
                </div>";

            _emailService.SendEmail(email, "Código de recuperación - Papelería Sonia", body);

            TempData["InfoRecover"] = "Código enviado a tu correo.";
            return RedirectToAction("RecoverPassword");
        }

        [HttpPost]
        public IActionResult VerifyCode(string code)
        {
            var savedCode = HttpContext.Session.GetString("RecoverCode");
            var expiry = HttpContext.Session.GetString("RecoverCodeExpiry");

            if (string.IsNullOrEmpty(savedCode) || string.IsNullOrEmpty(expiry))
            {
                TempData["ErrorRecover"] = "Sesión expirada. Vuelve a intentarlo.";
                return RedirectToAction("RecoverPassword");
            }

            if (DateTime.Parse(expiry) < DateTime.Now)
            {
                TempData["ErrorRecover"] = "El código ha expirado. Solicita uno nuevo.";
                return RedirectToAction("RecoverPassword");
            }

            if (code != savedCode)
            {
                TempData["ErrorVerify"] = "Código incorrecto. Inténtalo de nuevo.";
                TempData["InfoRecover"] = "true";
                return RedirectToAction("RecoverPassword");
            }

            HttpContext.Session.SetString("CodeVerified", "true");
            TempData["CodeVerified"] = "true";
            return RedirectToAction("RecoverPassword");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string repeatPassword)
        {
            if (newPassword != repeatPassword)
            {
                TempData["ErrorReset"] = "Las contraseñas no coinciden.";
                return RedirectToAction("RecoverPassword");
            }

            var email = HttpContext.Session.GetString("RecoverEmail");
            var verified = HttpContext.Session.GetString("CodeVerified");

            if (string.IsNullOrEmpty(email) || verified != "true")
            {
                TempData["ErrorRecover"] = "Sesión inválida. Vuelve a intentarlo.";
                return RedirectToAction("RecoverPassword");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorRecover"] = "Usuario no encontrado.";
                return RedirectToAction("RecoverPassword");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            // Limpiar sesión de recuperación
            HttpContext.Session.Remove("RecoverCode");
            HttpContext.Session.Remove("RecoverEmail");
            HttpContext.Session.Remove("RecoverCodeExpiry");
            HttpContext.Session.Remove("CodeVerified");
            return RedirectToAction("RecoverPassword", new { success = "true" });
        }
    }
}