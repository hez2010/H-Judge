using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace hjudge.WebHost.Middlewares
{
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        static ExceptionMiddleware()
        {
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

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
            await JsonSerializer.SerializeAsync(context.Response.Body, value, jsonSerializerOptions);
        }
    }
}
