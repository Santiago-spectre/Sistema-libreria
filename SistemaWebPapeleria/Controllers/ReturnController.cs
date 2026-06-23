using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.Helpers;
using SistemaWebPapeleria.ViewModels;

namespace SistemaWebPapeleria.Controllers
{
    public class ReturnController : Controller
    {
        private readonly AppDbContext _context;

        public ReturnController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReturnRequestVM request)
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
                return BadRequest(new { mensaje = "No se enviaron productos para devolver." });

            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            // Verificar que la venta existe
            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .FirstOrDefaultAsync(s => s.SaleId == request.SaleId);

            if (sale == null)
                return NotFound(new { mensaje = "Venta no encontrada." });

            // Crear la devolución
            var returnRecord = new Return
            {
                Date = DateTimeHelper.AhoraEnPeru(),
                Reason = request.Reason,
                SaleId = request.SaleId,
                UserId = userId,
                ReturnDetails = new List<ReturnDetail>()
            };

            // Procesar cada item
            foreach (var item in request.Items)
            {
                // Verificar que la cantidad a devolver no supere la vendida
                var saleDetail = sale.SaleDetails.FirstOrDefault(sd => sd.ProductId == item.ProductId);
                if (saleDetail == null)
                    return BadRequest(new { mensaje = $"El producto no pertenece a esta venta." });

                if (item.Quantity > saleDetail.Quantity)
                    return BadRequest(new { mensaje = $"La cantidad a devolver supera la cantidad vendida." });

                // Restituir stock
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null && !product.IsService)
                    product.Stock += item.Quantity;

                returnRecord.ReturnDetails.Add(new ReturnDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = saleDetail.UnitPrice
                });
            }

            _context.Returns.Add(returnRecord);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Devolución registrada correctamente." });
        }
    }
}