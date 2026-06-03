using Microsoft.AspNetCore.Mvc;
using SistemaWebPapeleria.Data;

namespace SistemaWebPapeleria.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _appDbContext;
        public UserController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
