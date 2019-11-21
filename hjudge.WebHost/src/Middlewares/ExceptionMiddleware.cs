using System.Threading.Tasks;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace hjudge.WebHost.Middlewares
{
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly static JsonSerializerSettings jsonSerializerOptions = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (InterfaceException ex)
            {
                context.Response.StatusCode = (int)ex.Code;
                await WriteExceptionAsync(context, new ErrorModel
                {
                    ErrorCode = ex.Code,
                    ErrorMessage = ex.Message
                });
            }
        }

        public static async Task WriteExceptionAsync<T>(HttpContext context, T value)
        {
            context.Response.ContentType = "application/json";
            var result = JsonConvert.SerializeObject(value, jsonSerializerOptions);
            await context.Response.WriteAsync(result);
        }
    }
}
