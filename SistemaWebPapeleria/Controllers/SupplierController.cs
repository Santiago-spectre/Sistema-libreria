using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.Controllers
{
    public class SupplierController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public SupplierController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var suppliers = await _appDbContext.Suppliers.OrderBy(s => s.Name).ToListAsync();
            return View(suppliers);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Supplier model)
        {
            var supplier = new Supplier
            {
                Name = model.Name,
                Phone = model.Phone,
                Address = model.Address,
                Description = model.Description,
                Status = true
            };

            await _appDbContext.Suppliers.AddAsync(supplier);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Supplier model)
        {
            var supplier = await _appDbContext.Suppliers.FindAsync(id);
            if (supplier == null) return RedirectToAction("Index");

            supplier.Name = model.Name;
            supplier.Phone = model.Phone;
            supplier.Address = model.Address;
            supplier.Description = model.Description;

            _appDbContext.Suppliers.Update(supplier);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var supplier = await _appDbContext.Suppliers.FindAsync(id);
            if (supplier == null) return RedirectToAction("Index");

            supplier.Status = !supplier.Status;

            _appDbContext.Suppliers.Update(supplier);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _appDbContext.Suppliers.FindAsync(id);
            if (supplier == null) return RedirectToAction("Index");

            _appDbContext.Suppliers.Remove(supplier);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}