using hjudgeWebHost.Controllers;
using hjudgeWebHost.Data.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace hjudgeWebHostTest
{
    [TestClass]
    public class AccountTest
    {
        private readonly TestService service;

        public AccountTest()
        {
            service = new TestService();
        }
        [TestMethod]
        public async Task Register()
        {
            var signInManager = service.Provider.GetRequiredService<SignInManager<UserInfo>>();
            var userManager = service.Provider.GetService<UserManager<UserInfo>>();
            var context = new DefaultHttpContext
            {
                RequestServices = service.Provider
            };
            signInManager.Context = context;
            var controller = new AccountController(userManager, signInManager)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = context
                },
                ObjectValidator = service.Provider.GetRequiredService<IObjectModelValidator>()
            };

            hjudgeWebHost.Models.ResultModel registerResult;

            registerResult = await controller.Register(new AccountController.RegisterModel
            {
                Email = "test@test.com",
                Name = "test",
                UserName = "test",
                Password = "qweRTY#123456",
                ConfirmPassword = "qweRTY#123456"
            });
            Assert.IsTrue(registerResult.Succeeded);

            registerResult = await controller.Register(new AccountController.RegisterModel
            {
                Email = "test@test.com",
                Name = "test",
                UserName = "test",
                Password = "qweRTY#123456",
                ConfirmPassword = "qweRTY#123456"
            });
            Assert.IsFalse(registerResult.Succeeded);

            registerResult = await controller.Register(new AccountController.RegisterModel
            {
                Email = "test2@test.com",
                Name = "test",
                UserName = "test",
                Password = "qweRTY#123456",
                ConfirmPassword = "qweRTY#123456"
            });
            Assert.IsFalse(registerResult.Succeeded);

            registerResult = await controller.Register(new AccountController.RegisterModel
            {
                Email = "test@test.com",
                Name = "test",
                UserName = "test2",
                Password = "qweRTY#123456",
                ConfirmPassword = "qweRTY#123456"
            });
            Assert.IsFalse(registerResult.Succeeded);

            registerResult = await controller.Register(new AccountController.RegisterModel
            {
                Email = "test2@test.com",
                Name = "test",
                UserName = "test2",
                Password = "qweRTY#123456",
                ConfirmPassword = "123456"
            });
            Assert.IsFalse(registerResult.Succeeded);
        }
    }
}
