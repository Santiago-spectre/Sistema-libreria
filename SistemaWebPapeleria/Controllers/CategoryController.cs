using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public CategoryController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string description)
        {
            var category = new Category
            {
                Name = name,
                Description = description ?? ""
            };

            await _appDbContext.Categories.AddAsync(category);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, string description)
        {
            var category = await _appDbContext.Categories.FindAsync(id);
            if (category == null) return RedirectToAction("Index", "Product");

            category.Name = name;
            category.Description = description ?? "";

            _appDbContext.Categories.Update(category);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _appDbContext.Categories.FindAsync(id);
            if (category == null) return RedirectToAction("Index", "Product");

            _appDbContext.Categories.Remove(category);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }
    }
}