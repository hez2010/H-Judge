﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using hjudge.WebHost.Data;
using hjudge.WebHost.Exceptions;
using hjudge.WebHost.Models;
using hjudge.WebHost.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace hjudge.WebHost.Middlewares
{
    public static class PrivilegeAuthentication
    {
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        [ProducesResponseType(401, Type = typeof(ErrorModel))]
        public sealed class RequireSignedInAttribute : Attribute, IAsyncActionFilter
        {
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var dbContext = context.HttpContext.RequestServices.GetService<WebHostDbContext>();
                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userInfo = await dbContext.Users.FirstOrDefaultAsync(i => i.Id == userId);

                if (userInfo is null) throw new AuthenticationException("没有登录账户");
                if (userInfo.Privilege == 5) throw new AuthenticationException("账户已被加入黑名单");

                await next();
            }
        }

        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        [ProducesResponseType(401, Type = typeof(ErrorModel))]
        [ProducesResponseType(403, Type = typeof(ErrorModel))]
        public sealed class RequireAdminAttribute : Attribute, IAsyncActionFilter
        {
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var dbContext = context.HttpContext.RequestServices.GetService<WebHostDbContext>();

                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userInfo = await dbContext.Users.FirstOrDefaultAsync(i => i.Id == userId);

                if (userInfo is null) throw new AuthenticationException("没有登录账户");
                if (!PrivilegeHelper.IsAdmin(userInfo?.Privilege ?? 0)) throw new ForbiddenException();

                await next();
            }
        }

        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        [ProducesResponseType(401, Type = typeof(ErrorModel))]
        [ProducesResponseType(403, Type = typeof(ErrorModel))]
        public sealed class RequireTeacherAttribute : Attribute, IAsyncActionFilter
        {
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var dbContext = context.HttpContext.RequestServices.GetService<WebHostDbContext>();

                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userInfo = await dbContext.Users.FirstOrDefaultAsync(i => i.Id == userId);

                if (userInfo is null) throw new AuthenticationException("没有登录账户");
                if (!PrivilegeHelper.IsTeacher(userInfo?.Privilege ?? 0)) throw new ForbiddenException();

                await next();
            }
        }
    }
}
