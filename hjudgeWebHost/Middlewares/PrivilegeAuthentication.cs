using hjudgeWebHost.Data.Identity;
using hjudgeWebHost.Models;
using hjudgeWebHost.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace hjudgeWebHost.Middlewares
{
    public class PrivilegeAuthentication : Attribute, IAsyncActionFilter
    {
        //private static 
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!(context.HttpContext.RequestServices.GetService(typeof(UserManager<UserInfo>)) is UserManager<UserInfo> userManager) ||
                !(context.HttpContext.RequestServices.GetService(typeof(SignInManager<UserInfo>)) is SignInManager<UserInfo> signInManager))
                throw new NullReferenceException("UserManager<UserInfo> or SignInManager<UserInfo> is null");

            var attributes = context.ActionDescriptor.EndpointMetadata;

            var userInfo = await userManager.GetUserAsync(context.HttpContext.User);

            foreach (var attribute in attributes)
            {
                if (attribute is RequireSignedInAttribute)
                {
                    if (!signInManager.IsSignedIn(context.HttpContext.User) || userInfo == null)
                    {
                        context.Result = new JsonResult(new ResultModel
                        {
                            ErrorCode = 403,
                            ErrorMessage = "User authentication failed",
                            Succeeded = false
                        });
                        return;
                    }
                    continue;
                }
                if (attribute is RequireTeacherAttribute)
                {
                    if (!PrivilegeHelper.IsTeacher(userInfo?.Privilege ?? 0))
                    {
                        context.Result = new JsonResult(new ResultModel
                        {
                            ErrorCode = 403,
                            ErrorMessage = "User authentication failed",
                            Succeeded = false
                        });
                        return;
                    }
                    continue;
                }
                if (attribute is RequireAdminAttribute)
                {
                    if (!PrivilegeHelper.IsAdmin(userInfo?.Privilege ?? 0))
                    {
                        context.Result = new JsonResult(new ResultModel
                        {
                            ErrorCode = 403,
                            ErrorMessage = "User authentication failed",
                            Succeeded = false
                        });
                        return;
                    }
                    continue;
                }
            }

            await next();
        }

        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        public class RequireSignedInAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        public class RequireAdminAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        public class RequireTeacherAttribute : Attribute { }
    }
}
