using System.Threading.Tasks;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace hjudge.WebHost.Middlewares
{
    public class ExceptionMiddleware : IAsyncExceptionFilter
    {
        private readonly static JsonSerializerSettings jsonSerializerOptions = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        public static async Task WriteExceptionAsync<T>(HttpContext context, T value)
        {
            context.Response.ContentType = "application/json";
            var result = JsonConvert.SerializeObject(value, jsonSerializerOptions);
            await context.Response.WriteAsync(result);
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception is InterfaceException ex)
            {
                context.ExceptionHandled = true;
                context.HttpContext.Response.StatusCode = (int)ex.Code;
                await WriteExceptionAsync(context.HttpContext, new ErrorModel
                {
                    ErrorCode = ex.Code,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
