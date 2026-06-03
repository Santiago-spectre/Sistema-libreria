using Microsoft.AspNetCore.Mvc;

namespace SistemaWebPapeleria.Controllers
{
    public class CashClosingController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
