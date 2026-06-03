using Microsoft.AspNetCore.Mvc;

namespace SistemaWebPapeleria.Controllers
{
    public class SupplierController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
