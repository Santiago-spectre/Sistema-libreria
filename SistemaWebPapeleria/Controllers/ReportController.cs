using Microsoft.AspNetCore.Mvc;

namespace SistemaWebPapeleria.Controllers
{
    public class ReportController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
