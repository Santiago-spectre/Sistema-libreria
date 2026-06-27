using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Helpers;
using SistemaWebPapeleria.Models;
using SistemaWebPapeleria.Services;
using SistemaWebPapeleria.ViewModels;

namespace SistemaWebPapeleria.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConverter _converter;
        private readonly ViewRenderService _viewRender;

        public ReportController(AppDbContext context, IConverter converter, ViewRenderService viewRender)
        {
            _context = context;
            _converter = converter;
            _viewRender = viewRender;
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
                sales = await _context.Sales
                    .Include(s => s.User)
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.Date)
                    .ToListAsync();
            }
            else
            {
                sales = await _context.Sales
                    .Include(s => s.User)
                    .OrderByDescending(s => s.Date)
                    .ToListAsync();
            }

            // Lista de vendedores para el filtro (solo admin)
            ViewBag.Users = await _context.Users
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
                var usuarios = await _context.Users
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
                var cajasCerradasHoy = await _context.CashClosings
                    .Where(c => c.Date.Date == hoy && (c.ClosingAmount != 0 || c.TotalSales != 0))
                    .Select(c => c.UserId)
                    .ToListAsync();

                ViewBag.CajasCerradasHoy = cajasCerradasHoy;
                ViewBag.ReporteHoy = reporteHoy;

                // Historial últimos 7 días por usuario
                var cajasUltimos7 = await _context.CashClosings
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

                var cajaCerradaHoy = await _context.CashClosings
                    .AnyAsync(c => c.UserId == userId && c.Date.Date == hoy && (c.ClosingAmount != 0 || c.TotalSales != 0));

                ViewBag.CajaCerradaHoy = cajaCerradaHoy;
                ViewBag.ReporteHoy = reporteHoy;

                var cajasUltimos7Vendedor = await _context.CashClosings
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
            var sale = await _context.Sales
                .Include(s => s.User)
                .Include(s => s.Receipt)
                .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Product)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null) return NotFound();

            // Construir el HTML del comprobante
            var html = await _viewRender.RenderToStringAsync("~/Views/PDF/Comprobante.cshtml", sale);

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

            //Registrar comprobante en BD si no existe
            if (sale.Receipt == null)
            {
                var receipt = new Receipt
                {
                    SaleId = sale.SaleId,
                    IssueDate = DateTimeHelper.AhoraEnPeru(),
                    Observations = ""
                };
                _context.Receipts.Add(receipt);
                sale.ReceiptIssued = true;
                await _context.SaveChangesAsync();
            }

            return File(pdf, "application/pdf", $"Comprobante-{sale.SaleId}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GenerarReporteMensual(int mes, int anio)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            var query = _context.Sales
                .Include(s => s.User)
                .Where(s => s.Date.Month == mes && s.Date.Year == anio);

            if (userRole == "Vendedor")
                query = query.Where(s => s.UserId == userId);

            var sales = await query.OrderBy(s => s.Date).ToListAsync();

            var nombreMes = new System.Globalization.CultureInfo("es-PE").DateTimeFormat.GetMonthName(mes);

            var reporteVM = new ReporteMensualVM
            {
                MonthName = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1),
                Year = anio,
                Sales = sales
            };

            var html = await _viewRender.RenderToStringAsync("~/Views/PDF/ReporteMensual.cshtml", reporteVM);

            var pdf = _converter.Convert(new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                },
                Objects = {
                    new ObjectSettings {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            });

            // Notificar que se descargó el reporte
            await NotificationHelper.CrearAsync(_context, userId,
                "Reporte descargado",
                $"Se descargó el reporte de ventas de {reporteVM.MonthName} {anio}.",
                "Reporte");

            if (userRole != "Administrador")
            {
                var admins = await _context.Users
                    .Where(u => u.Role.RoleName == "Administrador" && u.UserId != userId)
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    await NotificationHelper.CrearAsync(_context, admin.UserId,
                        "Reporte descargado",
                        $"Se descargó el reporte de ventas de {reporteVM.MonthName} {anio}.",
                        "Reporte");
                }
            }

            return File(pdf, "application/pdf", $"Reporte-{mes}-{anio}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GetSaleDetail(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Product)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null) return NotFound();

            var detalle = new
            {
                saleId = sale.SaleId,
                fecha = sale.Date.ToString("dd/MM/yyyy HH:mm"),
                paymentMethod = sale.PaymentMethod,
                discount = sale.Discount,
                total = sale.Total,
                items = sale.SaleDetails.Select(d => new
                {
                    ProductId = d.ProductId,
                    producto = d.Product?.Name,
                    cantidad = d.Quantity,
                    precioUnitario = d.UnitPrice,
                    subtotal = d.Subtotal
                }).ToList()
            };

            return Json(detalle);
        }
    }
}
