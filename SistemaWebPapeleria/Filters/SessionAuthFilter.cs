using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SistemaWebPapeleria.Filters
{
    public class SessionAuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString();

            if(controllerName == "Login")
            {
                base.OnActionExecuting(context);
                return;
            }

            var userId = context.HttpContext.Session.GetString("UserId");
            System.Diagnostics.Debug.WriteLine($"FILTRO - Controlador: {controllerName} - UserId: '{userId}'");

            if (string.IsNullOrEmpty(userId))
            {
                System.Diagnostics.Debug.WriteLine("FILTRO - Redirigiendo al login");
                context.Result = new RedirectToActionResult("Login", "Login", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
