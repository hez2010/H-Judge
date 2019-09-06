using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace hjudge.WebHost.Services
{
    public class CachedUserManager<TUser> : UserManager<TUser> where TUser : class
    {
        private readonly ICacheService cacheService;

        public CachedUserManager(
            IUserStore<TUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<CachedUserManager<TUser>> logger,
            ICacheService cacheService) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.cacheService = cacheService;
        }

        // public override async Task<IdentityResult> SetEmailAsync(TUser user, string email)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.SetEmailAsync(user, email);
        // }
        // public override async Task<IdentityResult> SetLockoutEnabledAsync(TUser user, bool enabled)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.SetLockoutEnabledAsync(user, enabled);
        // }
        // public override async Task<IdentityResult> SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.SetLockoutEndDateAsync(user, lockoutEnd);
        // }
        // public override async Task<IdentityResult> SetPhoneNumberAsync(TUser user, string phoneNumber)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.SetPhoneNumberAsync(user, phoneNumber);
        // }
        // public override async Task<IdentityResult> SetTwoFactorEnabledAsync(TUser user, bool enabled)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.SetTwoFactorEnabledAsync(user, enabled);
        // }
        // public override async Task<IdentityResult> SetUserNameAsync(TUser user, string userName)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.SetUserNameAsync(user, userName);
        // }
        // public override async Task<IdentityResult> CreateAsync(TUser user)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.CreateAsync(user);
        // }
        // public override async Task<IdentityResult> CreateAsync(TUser user, string password)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.CreateAsync(user, password);
        // }
        // public override async Task<byte[]> CreateSecurityTokenAsync(TUser user)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.CreateSecurityTokenAsync(user);
        // }
        // public override async Task<IdentityResult> AddPasswordAsync(TUser user, string password)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.AddPasswordAsync(user, password);
        // }
        // public override async Task<IdentityResult> UpdateAsync(TUser user)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.UpdateAsync(user);
        // }
        // public override async Task UpdateNormalizedEmailAsync(TUser user)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     await base.UpdateNormalizedEmailAsync(user);
        // }
        // public override async Task UpdateNormalizedUserNameAsync(TUser user)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     await base.UpdateNormalizedUserNameAsync(user);
        // }
        // public override async Task<IdentityResult> UpdateSecurityStampAsync(TUser user)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.UpdateSecurityStampAsync(user);
        // }
        // public override async Task<IdentityResult> RemovePasswordAsync(TUser user)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.RemovePasswordAsync(user);
        // }
        // public override async Task<IdentityResult> DeleteAsync(TUser user)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.DeleteAsync(user);
        // }
        // public override Task<TUser> FindByIdAsync(string? userId)
        // {
        //     return cacheService.GetObjectAndSetAsync($"user_{userId}", () => base.FindByIdAsync(userId));
        // }
        // public override async Task<IdentityResult> ChangeEmailAsync(TUser user, string newEmail, string token)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.ChangeEmailAsync(user, newEmail, token);
        // }
        // public override async Task<IdentityResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.ChangePasswordAsync(user, currentPassword, newPassword);
        // }
        // public override async Task<IdentityResult> ChangePhoneNumberAsync(TUser user, string phoneNumber, string token)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.ChangePhoneNumberAsync(user, phoneNumber, token);
        // }
        // public override async Task<IdentityResult> ResetPasswordAsync(TUser user, string token, string newPassword)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.ResetPasswordAsync(user, token, newPassword);
        // }
        // public override async Task<IdentityResult> ConfirmEmailAsync(TUser user, string token)
        // {
        //     var userId = await GetUserIdAsync(user);
        //     await cacheService.RemoveObjectAsync($"user_{userId}");
        //     return await base.ConfirmEmailAsync(user, token);
        // }

        // public override async Task<TUser> GetUserAsync(ClaimsPrincipal principal)
        // {
        //     var userId = GetUserId(principal);
        //     var userInfo = await cacheService.GetObjectAndSetAsync($"user_{userId}", () => base.GetUserAsync(principal));
        //     return userInfo;
        // }
    }
}
