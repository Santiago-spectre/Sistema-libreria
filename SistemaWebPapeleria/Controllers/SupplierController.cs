using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using SistemaWebPapeleria.Helpers;

namespace SistemaWebPapeleria.Controllers
{
    public class SupplierController : Controller
    {
        private readonly AppDbContext _context;

        public SupplierController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();
            return View(suppliers);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SupplierVM model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos del proveedor, hay campos inválidos.";
                return RedirectToAction("Index");
            }

            var supplier = new Supplier
            {
                Name = model.Name.Trim(),
                Phone = model.Phone,
                Address = model.Address,
                Description = model.Description,
                Status = true
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            await NotificarCRUD(userId, "Proveedor creado", $"Se creó el proveedor '{supplier.Name}'.");

            TempData["Success"] = "Proveedor creado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, SupplierVM model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos del proveedor, hay campos inválidos";
                return RedirectToAction("Index");
            }

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return RedirectToAction("Index");

            supplier.Name = model.Name.Trim();
            supplier.Phone = model.Phone;
            supplier.Address = model.Address;
            supplier.Description = model.Description;

            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            await NotificarCRUD(userId, "Proveedor actualizado", $"Se actualizó el proveedor '{supplier.Name}'.");

            TempData["Success"] = "Proveedor actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return RedirectToAction("Index");

            supplier.Status = !supplier.Status;

            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            string estado = supplier.Status ? "activado" : "desactivado";
            await NotificarCRUD(userId, "Proveedor " + estado, $"Se {estado} el proveedor '{supplier.Name}'.");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return RedirectToAction("Index");

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            string nombreProveedor = supplier.Name;

            var tieneProductos = await _context.Products.AnyAsync(p => p.SupplierId == id);
            if (tieneProductos)
            {
                TempData["Error"] = "No se puede eliminar este proveedor porque tiene productos asignados.";
                return RedirectToAction("Index");
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            await NotificarCRUD(userId, "Proveedor eliminado", $"Se eliminó el proveedor '{nombreProveedor}'.");

            return RedirectToAction("Index");
        }

        private async Task NotificarCRUD(int userId, string title, string message)
        {
            await NotificationHelper.CrearAsync(_context, userId, title, message, "Proveedor");

            var usuario = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
            if (usuario?.Role?.RoleName != "Administrador")
            {
                var admins = await _context.Users
                    .Where(u => u.Role.RoleName == "Administrador" && u.UserId != userId)
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    await NotificationHelper.CrearAsync(_context, admin.UserId, title, message, "Proveedor");
                }
            }
        }
    }
}