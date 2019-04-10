using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Models;
using hjudgeWebHost.Services;
using hjudgeWebHost.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace hjudgeWebHost.Middlewares
{
    public static class PrivilegeAuthentication
    {
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        public class RequireSignedInAttribute : Attribute, IAsyncActionFilter
        {
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (!(context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) is ApplicationDbContext dbContext))
                    throw new InvalidOperationException("DbContext is null");

                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userInfo = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(i => i.Id == userId);

                if (userInfo == null)
                {
                    context.Result = new JsonResult(new ResultModel
                    {
                        ErrorCode = ErrorDescription.NotSignedIn
                    });
                    return;
                }

                if (userInfo.Privilege == 5)
                {
                    context.Result = new JsonResult(new ResultModel
                    {
                        ErrorCode = ErrorDescription.AuthenticationFailed
                    });
                    return;
                }

                await next();
            }
        }

        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        public class RequireAdminAttribute : Attribute, IAsyncActionFilter
        {
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (!(context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) is ApplicationDbContext dbContext))
                    throw new InvalidOperationException("DbContext is null");

                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userInfo = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(i => i.Id == userId);

                if (!PrivilegeHelper.IsAdmin(userInfo?.Privilege ?? 0))
                {
                    context.Result = new JsonResult(new ResultModel
                    {
                        ErrorCode = ErrorDescription.NoEnoughPrivilege
                    });
                    return;
                }

                await next();
            }
        }

        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        public class RequireTeacherAttribute : Attribute, IAsyncActionFilter
        {
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (!(context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) is ApplicationDbContext dbContext))
                    throw new InvalidOperationException("DbContext is null");

                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userInfo = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(i => i.Id == userId);

                if (!PrivilegeHelper.IsTeacher(userInfo?.Privilege ?? 0))
                {
                    context.Result = new JsonResult(new ResultModel
                    {
                        ErrorCode = ErrorDescription.NoEnoughPrivilege
                    });
                    return;
                }

                await next();
            }
        }
    }
}
