using hjudge.WebHost.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace hjudge.WebHost.Middlewares
{
    public static class FunctionalControl
    {
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        public sealed class DisabledApiAttribute : Attribute, IAsyncActionFilter
        {
            public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                throw new NotFoundException();
            }
        }
    }
}
