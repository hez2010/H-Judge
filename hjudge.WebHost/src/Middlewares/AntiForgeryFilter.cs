using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace hjudge.WebHost.Middlewares
{
    public class AntiForgeryFilter : ResultFilterAttribute
    {
        private readonly IAntiforgery antiforgery;
        public AntiForgeryFilter(IAntiforgery antiforgery)
        {
            this.antiforgery = antiforgery;
        }
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            //Send xsrf token on get requests
            if (context.HttpContext.Request.Method.ToLower() == "get")
            {
                var tokens = antiforgery.GetAndStoreTokens(context.HttpContext);
                context.HttpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
            }
            
            await next();
        }
    }
}
