using Microsoft.AspNetCore.Mvc;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;

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
        public IActionResult Index()
        {
            var suppliers = _context.Suppliers.ToList();
            return View(suppliers);
        }

        [HttpPost]
        public IActionResult Create(Supplier supplier)
        {
            supplier.Status = true;
            _context.Suppliers.Add(supplier);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var supplier = _context.Suppliers.Find(id);
            if (supplier == null) return NotFound();
            supplier.Status = !supplier.Status;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}