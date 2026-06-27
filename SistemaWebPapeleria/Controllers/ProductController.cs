using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;
using SistemaWebPapeleria.Helpers;

namespace SistemaWebPapeleria.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // Muestra la lista de productos
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).Include(p => p.Supplier).OrderBy(p => p.Name).ToListAsync();

            // Tarjetas
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.LowStock = await _context.Products.Where(p => !p.IsService && p.IsActive && p.Stock <= p.MinimumStock && p.Stock > 0).CountAsync();
            ViewBag.OutOfStock = await _context.Products.Where(p => !p.IsService && p.IsActive && p.Stock == 0).CountAsync();

            // Lista de categorias para la tarjeta
            ViewBag.CategoriasList = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            // Agregar categorías y proveedores para el modal
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            ViewBag.Suppliers = new SelectList(await _context.Suppliers.Where(s => s.Status).ToListAsync(), "SupplierId", "Name");

            return View(products);
        }

        //Procesa el formulario para agregar producto
        [HttpPost]
        public async Task<IActionResult> Create(ProductVM model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos del producto, hay campos inválidos.";
                return RedirectToAction("Index");
            }

            Product product = new Product()
            {
                Name = model.Name,
                Description = model.Description ?? "",
                SalePrice = model.SalePrice,
                PurchasePrice = model.PurchasePrice,
                Stock = model.Stock,
                MinimumStock = model.MinimumStock,
                IsService = model.IsService,
                IsActive = true,
                CategoryId = model.CategoryId,
                SupplierId = model.IsService ? null : model.SupplierId,
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            await NotificarCRUD(userId, "Producto creado", $"Se creó el producto '{product.Name}'.");

            TempData["Success"] = "Producto guardado correctamente.";
            return RedirectToAction("Index", "Product");
        }

        //Procesa el formulario de edición
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductVM model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos del producto, hay campos inválidos.";
                return RedirectToAction("Index");
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null) return RedirectToAction("Index");

            // Actualiza los datos del producto
            product.Name = model.Name;
            product.Description = model.Description ?? "";
            product.SalePrice = model.SalePrice;
            product.PurchasePrice = model.PurchasePrice;
            product.Stock = model.Stock;
            product.MinimumStock = model.MinimumStock;
            product.IsService = model.IsService;
            product.IsActive = model.IsActive;
            product.CategoryId = model.CategoryId;
            product.SupplierId = model.IsService ? null : model.SupplierId;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            await NotificarCRUD(userId, "Producto actualizado", $"Se actualizó el producto '{product.Name}'.");

            TempData["Success"] = "Producto actualizado correctamente.";
            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var product = await _context.Products.FindAsync(id);

            if (product == null) return RedirectToAction("Index");

            //Cambia el estado activo/inactivo
            product.IsActive = !product.IsActive;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            string estado = product.IsActive ? "activado" : "desactivado";
            await NotificarCRUD(userId, "Producto " + estado, $"Se {estado} el producto '{product.Name}'.");


            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            var product = await _context.Products.FindAsync(id);

            if (product == null) return RedirectToAction("Index");

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            string nombreProducto = product.Name;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await NotificarCRUD(userId, "Producto eliminado", $"Se eliminó el producto '{nombreProducto}'.");

            return RedirectToAction("Index", "Product");
        }

        private async Task NotificarCRUD(int userId, string title, string message)
        {
            await NotificationHelper.CrearAsync(_context, userId, title, message, "Producto");

            var usuario = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
            if (usuario?.Role?.RoleName != "Administrador")
            {
                var admins = await _context.Users
                    .Where(u => u.Role.RoleName == "Administrador" && u.UserId != userId)
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    await NotificationHelper.CrearAsync(_context, admin.UserId, title, message, "Producto");
                }
            }
        }
    }
}
