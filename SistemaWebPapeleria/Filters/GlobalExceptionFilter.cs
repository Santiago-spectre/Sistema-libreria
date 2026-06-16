using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SistemaWebPapeleria.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.Result = new RedirectToActionResult("Error", "Home", null);
            context.ExceptionHandled = true;
        }
    }
}
