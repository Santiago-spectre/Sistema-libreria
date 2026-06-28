using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using SistemaWebPapeleria.Helpers;

namespace SistemaWebPapeleria.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryVM model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos de la categoría, hay campos inválidos.";
                return RedirectToAction("Index", "Product");
            }

            bool existeCategoria = await _context.Categories.AnyAsync(c => c.Name.ToLower() == model.Name.Trim().ToLower());
            if (existeCategoria)
            {
                TempData["Error"] = "Ya existe una categoría con ese nombre.";
                return RedirectToAction("Index", "Product");
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description ?? ""
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            await NotificarCRUD(userId, "Categoría creada", $"Se creó la categoría '{category.Name}'.");

            TempData["Success"] = "Categoría creada correctamente.";
            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CategoryVM model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos de la categoría, hay campos inválidos.";
                return RedirectToAction("Index", "Product");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return RedirectToAction("Index", "Product");

            bool existeCategoria = await _context.Categories.AnyAsync(c => c.CategoryId != id && c.Name.ToLower() == model.Name.Trim().ToLower());
            if (existeCategoria)
            {
                TempData["Error"] = "Ya existe otra categoria con ese nombre.";
                return RedirectToAction("Index", "Product");
            }

            category.Name = model.Name.Trim();
            category.Description = model.Description ?? "";

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            await NotificarCRUD(userId, "Categoría actualizada", $"Se actualizó la categoría '{category.Name}'.");


            TempData["Success"] = "Categoría actualizada correctamente.";
            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return RedirectToAction("Index", "Product");

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            string nombreCategoria = category.Name;

            var tieneProductos = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (tieneProductos)
            {
                TempData["Error"] = "No se puede eliminar esta categoría porque tiene productos asignados.";
                return RedirectToAction("Index", "Product");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            await NotificarCRUD(userId, "Categoría eliminada", $"Se eliminó la categoría '{nombreCategoria}'.");

            return RedirectToAction("Index", "Product");
        }

        private async Task NotificarCRUD(int userId, string title, string message)
        {
            await NotificationHelper.CrearAsync(_context, userId, title, message, "Categoría");

            var usuario = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
            if (usuario?.Role?.RoleName != "Administrador")
            {
                var admins = await _context.Users
                    .Where(u => u.Role.RoleName == "Administrador" && u.UserId != userId)
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    await NotificationHelper.CrearAsync(_context, admin.UserId, title, message, "Categoría");
                }
            }
        }
    }
}