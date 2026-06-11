using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SistemaWebPapeleria.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConverter _converter;
        public ReportController(AppDbContext appDbContext, IConverter converter)
        {
            _appDbContext = appDbContext;
            _converter = converter;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var hoy = DateTime.Today;

            List<Sale> sales;
            if (userRole == "Vendedor")
            {
                sales = await _appDbContext.Sales
                    .Include(s => s.User)
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.Date)
                    .ToListAsync();
            }
            else
            {
                sales = await _appDbContext.Sales
                    .Include(s => s.User)
                    .OrderByDescending(s => s.Date)
                    .ToListAsync();
            }

            // Lista de vendedores para el filtro (solo admin)
            ViewBag.Users = await _appDbContext.Users
                .Where(u => u.Status)
                .ToListAsync();

            // Meses con ventas para informes mensuales
            var monthlyGroups = sales
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new System.Globalization.CultureInfo("es-PE").DateTimeFormat.GetMonthName(g.Key.Month),
                    Total = g.Sum(s => s.Total),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Year)
                .ThenByDescending(g => g.Month)
                .ToList();

            ViewBag.MonthlyGroups = monthlyGroups;

            // ── Reporte del día ──
            if (userRole == "Administrador")
            {
                var usuarios = await _appDbContext.Users
                    .Where(u => u.Status)
                    .ToListAsync();

                var reporteHoy = usuarios.Select(u => new
                {
                    UserId = u.UserId,
                    NombreCompleto = u.Name + " " + u.LastName,
                    TotalVentas = sales.Where(s => s.UserId == u.UserId && s.Date.Date == hoy).Sum(s => s.Total),
                    NumVentas = sales.Count(s => s.UserId == u.UserId && s.Date.Date == hoy)
                }).ToList();

                // Cajas cerradas hoy por usuario
                var cajasCerradasHoy = await _appDbContext.CashClosings
                    .Where(c => c.Date.Date == hoy && (c.ClosingAmount != 0 || c.TotalSales != 0))
                    .Select(c => c.UserId)
                    .ToListAsync();

                ViewBag.CajasCerradasHoy = cajasCerradasHoy;
                ViewBag.ReporteHoy = reporteHoy;

                // Historial últimos 7 días por usuario
                var cajasUltimos7 = await _appDbContext.CashClosings
                    .Include(c => c.User)
                    .Where(c => c.Date.Date >= hoy.AddDays(-6) && c.Date.Date <= hoy && (c.ClosingAmount != 0 || c.TotalSales != 0))
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();

                ViewBag.Historial7 = cajasUltimos7;
            }
            else
            {
                var reporteHoy = new[]
                {
                    new
                    {
                        UserId = userId,
                        NombreCompleto = "",
                        TotalVentas = sales.Where(s => s.Date.Date == hoy).Sum(s => s.Total),
                        NumVentas = sales.Count(s => s.Date.Date == hoy)
                    }
                }.ToList();

                var cajaCerradaHoy = await _appDbContext.CashClosings
                    .AnyAsync(c => c.UserId == userId && c.Date.Date == hoy && (c.ClosingAmount != 0 || c.TotalSales != 0));

                ViewBag.CajaCerradaHoy = cajaCerradaHoy;
                ViewBag.ReporteHoy = reporteHoy;

                var cajasUltimos7Vendedor = await _appDbContext.CashClosings
                    .Include(c => c.User)
                    .Where(c => c.UserId == userId && c.Date.Date >= hoy.AddDays(-6) && c.Date.Date <= hoy && (c.ClosingAmount != 0 || c.TotalSales != 0))
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();

                ViewBag.Historial7 = cajasUltimos7Vendedor;
            }

            return View(sales);
        }

        [HttpGet]
        public async Task<IActionResult> GenerarComprobante(int id)
        {
            var sale = await _appDbContext.Sales
                .Include(s => s.User)
                .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Product)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null) return NotFound();

            // Construir el HTML del comprobante
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8' />
                <style>
                    * {{ margin: 0; padding: 0; box-sizing: border-box; }}
                    body {{ font-family: Arial, sans-serif; font-size: 12px; color: #000; padding: 20px; }}
                    .header {{ text-align: center; margin-bottom: 15px; border-bottom: 2px solid #000; padding-bottom: 10px; }}
                    .header h1 {{ font-size: 18px; font-weight: bold; text-transform: uppercase; }}
                    .header p {{ font-size: 11px; color: #333; }}
                    .comprobante-titulo {{ text-align: center; margin: 10px 0; padding: 8px; background: #1a1a2e; color: #ffffff; border-radius: 5px; }}
                    .comprobante-titulo h2 {{ font-size: 14px; font-weight: bold; }}
                    .datos {{ margin: 10px 0; border: 1px solid #ddd; border-radius: 5px; padding: 10px; }}
                    .datos p {{ margin-bottom: 4px; font-size: 11px; }}
                    .datos span {{ font-weight: bold; }}
                    table {{ width: 100%; border-collapse: collapse; margin: 10px 0; }}
                    thead tr {{ background: #1a1a2e; color: #ffffff; }}
                    thead th {{ padding: 8px; text-align: left; font-size: 11px; }}
                    tbody tr {{ border-bottom: 1px solid #eee; }}
                    tbody td {{ padding: 8px; font-size: 11px; }}
                    .totales {{ text-align: right; margin-top: 10px; border-top: 2px solid #000; padding-top: 10px; }}
                    .total-final {{ font-size: 15px; font-weight: bold; }}
                    .footer {{ text-align: center; margin-top: 15px; border-top: 1px solid #ddd; padding-top: 10px; font-size: 10px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Papelería Sonia</h1>
                    <p>Jr. Ejemplo 123 - Cajamarca, Perú</p>
                    <p>Tel: 987654321 | papeleriasonia@gmail.com</p>
                </div>
                <div class='comprobante-titulo'>
                    <h2>BOLETA DE VENTA</h2>
                    <p>B001-{sale.SaleId:D8}</p>
                </div>
                <div class='datos'>
                    <p><span>Fecha de emisión:</span> {sale.Date:dd/MM/yyyy HH:mm}</p>
                    <p><span>Vendedor:</span> {sale.User?.Name} {sale.User?.LastName}</p>
                    <p><span>Método de pago:</span> {sale.PaymentMethod}</p>
                    <p><span>Condición:</span> CONTADO</p>
                </div>
                <table>
                    <thead>
                        <tr>
                            <th>Descripción</th>
                            <th style='text-align:center'>Cant.</th>
                            <th style='text-align:right'>P. Unit.</th>
                            <th style='text-align:right'>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", sale.SaleDetails.Select(d => $@"
                        <tr>
                            <td>{d.Product?.Name}</td>
                            <td style='text-align:center'>{d.Quantity}</td>
                            <td style='text-align:right'>S/ {d.UnitPrice:0.00}</td>
                            <td style='text-align:right'>S/ {d.Subtotal:0.00}</td>
                        </tr>"))}
                    </tbody>
                </table>
                <div class='totales'>
                    {(sale.Discount > 0 ? $"<p>Descuento: S/ {sale.Discount:0.00}</p>" : "")}
                    <p class='total-final'>TOTAL A PAGAR: S/ {sale.Total:0.00}</p>
                </div>
                <div class='footer'>
                    <p>Representación impresa del comprobante de venta</p>
                    <p>Gracias por su compra en Papelería Sonia</p>
                </div>
            </body>
            </html>";

            var pdf = _converter.Convert(new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A5,
                },
                Objects = {
                    new ObjectSettings {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            });

            return File(pdf, "application/pdf", $"Comprobante-{sale.SaleId}.pdf");
        }
    }
}
