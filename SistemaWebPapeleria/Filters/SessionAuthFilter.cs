using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SistemaWebPapeleria.Filters
{
    public class SessionAuthFilter : ActionFilterAttribute
    {
        private static readonly string[] _soloAdmin = new[]
        {
            "Product", "User", "Supplier"
        };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString();

            if (controllerName == "Login")
            {
                base.OnActionExecuting(context);
                return;
            }

            var userId = context.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Login", null);
                return;
            }

            var userRole = context.HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrador" && _soloAdmin.Contains(controllerName))
            {
                context.HttpContext.Session.SetString("AccesoNegado", "No tienes permisos para acceder a esa sección.");
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}