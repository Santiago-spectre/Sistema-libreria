using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;

namespace SistemaWebPapeleria.Controllers
{
    public class UserController : Controller
    {
        //conexion con la base de datos
        private readonly AppDbContext _appDbContext;
        public UserController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        //Mostrar el formulario de registro
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //Procesa el formulario de registro de login
        [HttpPost]
        public async Task<IActionResult> Login(UserVM model)
        {
            // Busca el usuario (correo y contraseña)
            User? usuario_encontrado = await _appDbContext.Users.Include(u => u.Role).Where(u => u.Email == model.Email && u.Password == model.Password).FirstOrDefaultAsync();

            if (usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "Correo o contraseña incorrectos";
                return View();
            }

            //Guarda los datos del usuario en sesion
            HttpContext.Session.SetString("UserId", usuario_encontrado.UserId.ToString());
            HttpContext.Session.SetString("UserName", usuario_encontrado.Name + " " + usuario_encontrado.LastName);
            HttpContext.Session.SetString("UserRole", usuario_encontrado.Role.RoleName);

            //redirige al inicio si el login es correcto
            return RedirectToAction("Index", "Home");
        }
    }
}
