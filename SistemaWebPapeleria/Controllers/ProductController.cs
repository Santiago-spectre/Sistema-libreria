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
                var errores = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["Error"] = errores;
                return RedirectToAction("Index");
            }

            Product product = new Product()
            {
                Name = model.Name,
                Description = model.Description ?? "",
                SalePrice = model.SalePrice,
                Stock = model.IsService ? 0 : (model.Stock ?? 0),
                MinimumStock = model.IsService ? 0 : (model.MinimumStock ?? 5),
                PurchasePrice = model.IsService ? 0 : (model.PurchasePrice ?? 0),
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

        [HttpPost]
        public async Task<IActionResult> CreateService(ProductVM model)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador") return Forbid();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["Error"] = "El nombre del servicio es obligatorio.";
                return RedirectToAction("Index");
            }

            if (model.SalePrice <= 0)
            {
                TempData["Error"] = "El precio de venta debe ser mayor a 0.";
                return RedirectToAction("Index");
            }

            if (model.CategoryId == 0)
            {
                TempData["Error"] = "Debe seleccionar una categoría.";
                return RedirectToAction("Index");
            }

            var product = new Product()
            {
                Name = model.Name,
                Description = model.Description ?? "",
                SalePrice = model.SalePrice,
                PurchasePrice = 0,
                Stock = 0,
                MinimumStock = 0,
                IsService = true,
                IsActive = true,
                CategoryId = model.CategoryId,
                SupplierId = null,
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            await NotificarCRUD(userId, "Servicio creado", $"Se creó el servicio '{product.Name}'.");

            TempData["Success"] = "Servicio guardado correctamente.";
            return RedirectToAction("Index");
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
            product.PurchasePrice = model.IsService ? 0 : (model.PurchasePrice ?? 0);
            product.Stock = model.IsService ? 0 : (model.Stock ?? 0);
            product.MinimumStock = model.IsService ? 0 : (model.MinimumStock ?? 5);
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
