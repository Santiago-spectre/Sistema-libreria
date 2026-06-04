using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create(User user)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            user.Status = true;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var existing = await _context.Users.FindAsync(user.UserId);
            if (existing == null) return NotFound();

            existing.Name = user.Name;
            existing.LastName = user.LastName;
            existing.Email = user.Email;
            existing.RoleId = user.RoleId;

            if (!string.IsNullOrEmpty(user.Password))
                existing.Password = user.Password;

            await _context.SaveChangesAsync();
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}