using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;

namespace SistemaWebPapeleria.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _appDbContext;
        public LoginController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            User? usuario_encontrado = await _appDbContext.Users
                .Include(u => u.Role)
                .Where(u => u.Email == model.Email && u.Password == model.Password)
                .FirstOrDefaultAsync();

            if (usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "Correo o contraseña incorrectos";
                return View();
            }

            HttpContext.Session.SetString("UserId", usuario_encontrado.UserId.ToString());
            HttpContext.Session.SetString("UserName", usuario_encontrado.Name + " " + usuario_encontrado.LastName);
            HttpContext.Session.SetString("UserRole", usuario_encontrado.Role.RoleName);

            switch (usuario_encontrado.Role.RoleName)
            {
                case "Administrador":
                    return RedirectToAction("Administrator", "Login");
                case "Vendedor":
                    return RedirectToAction("Seller", "Login");
                default:
                    return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        public IActionResult Administrator()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Seller()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}