using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.ViewModels;

namespace SistemaWebPapeleria.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public ProductController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // Muestra la lista de productos
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _appDbContext.Products.Include(p => p.Category).Include(p => p.Supplier).OrderBy(p => p.Name).ToListAsync();

            // Tarjetas
            ViewBag.TotalProducts = await _appDbContext.Products.CountAsync();
            ViewBag.LowStock = await _appDbContext.Products.Where(p => !p.IsService && p.IsActive && p.Stock <= p.MinimumStock && p.Stock > 0).CountAsync();
            ViewBag.OutOfStock = await _appDbContext.Products.Where(p => !p.IsService && p.IsActive && p.Stock == 0).CountAsync();

            // Lista de categorias para la tarjeta
            ViewBag.CategoriasList = await _appDbContext.Categories.OrderBy(c => c.Name).ToListAsync();

            // Agregar categorías y proveedores para el modal
            ViewBag.Categories = new SelectList(await _appDbContext.Categories.ToListAsync(), "CategoryId", "Name");
            ViewBag.Suppliers = new SelectList(await _appDbContext.Suppliers.Where(s => s.Status).ToListAsync(), "SupplierId", "Name");

            return View(products);
        }

        //muestra el formulario para agregar productos
        [HttpGet]
        public async Task<IActionResult> Create()       //nuevo
        {
            //Carga las categorias para el dropdow
            ViewBag.Categories = new SelectList(await _appDbContext.Categories.ToListAsync(), "CategoryId", "Name");

            //Carga los proveedores para el dropdown (opcional)
            ViewBag.Suppliers = new SelectList(
                await _appDbContext.Suppliers.Where(s => s.Status).ToListAsync(), "SupplierId", "Name");

            return View();
        }

        //Procesa el formulario para agregar producto
        [HttpPost]
        public async Task<IActionResult> Create(ProductVM model)
        {
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

            await _appDbContext.Products.AddAsync(product);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }

        //Muestra el formulario para editar producto
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Busca el producto por ID
            var product = await _appDbContext.Products.FindAsync(id);
            if (product == null) return RedirectToAction("Index");

            ViewBag.Categories = new SelectList(await _appDbContext.Categories.ToListAsync(), "CategoryId", "Name", product.CategoryId);

            ViewBag.Suppliers = new SelectList(await _appDbContext.Suppliers.Where(s => s.Status).ToListAsync(), "SupplierId", "Name", product.SupplierId);

            //Mapea el producto al ViewModel
            ProductVM model = new ProductVM()
            {
                Name = product.Name,
                Description = product.Description,
                SalePrice = product.SalePrice,
                PurchasePrice = product.PurchasePrice,
                Stock = product.Stock,
                MinimumStock = product.MinimumStock,
                IsService = product.IsService,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
            };

            return View(model);
        }

        //Procesa el formulario de edición
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductVM model)
        {
            var product = await _appDbContext.Products.FindAsync(id);

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

            _appDbContext.Products.Update(product);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var product = await _appDbContext.Products.FindAsync(id);

            if (product == null) return RedirectToAction("Index");

            //Cambia el estado activo/inactivo
            product.IsActive = !product.IsActive;

            _appDbContext.Products.Update(product);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _appDbContext.Products.FindAsync(id);

            if (product == null) return RedirectToAction("Index");

            _appDbContext.Products.Remove(product);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }
    }
}
