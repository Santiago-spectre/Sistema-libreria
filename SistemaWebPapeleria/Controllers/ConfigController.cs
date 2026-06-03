using Microsoft.AspNetCore.Mvc;

namespace SistemaWebPapeleria.Controllers
{
    public class ConfigController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
