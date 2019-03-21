using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
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

        public override async Task<IdentityResult> SetEmailAsync(TUser user, string email)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.SetEmailAsync(user, email);
        }
        public override async Task<IdentityResult> SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.SetLockoutEnabledAsync(user, enabled);
        }
        public override async Task<IdentityResult> SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.SetLockoutEndDateAsync(user, lockoutEnd);
        }
        public override async Task<IdentityResult> SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.SetPhoneNumberAsync(user, phoneNumber);
        }
        public override async Task<IdentityResult> SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.SetTwoFactorEnabledAsync(user, enabled);
        }
        public override async Task<IdentityResult> SetUserNameAsync(TUser user, string userName)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.SetUserNameAsync(user, userName);
        }
        public override async Task<IdentityResult> CreateAsync(TUser user)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.CreateAsync(user);
        }
        public override async Task<IdentityResult> CreateAsync(TUser user, string password)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.CreateAsync(user, password);
        }
        public override async Task<byte[]> CreateSecurityTokenAsync(TUser user)
        {
            return await base.CreateSecurityTokenAsync(user);
        }
        public override async Task<IdentityResult> AddPasswordAsync(TUser user, string password)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.AddPasswordAsync(user, password);
        }
        public override async Task<IdentityResult> UpdateAsync(TUser user)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.UpdateAsync(user);
        }
        public override async Task UpdateNormalizedEmailAsync(TUser user)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            await base.UpdateNormalizedEmailAsync(user);
        }
        public override async Task UpdateNormalizedUserNameAsync(TUser user)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            await base.UpdateNormalizedUserNameAsync(user);
        }
        protected override async Task<IdentityResult> UpdatePasswordHash(TUser user, string newPassword, bool validatePassword)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.UpdatePasswordHash(user, newPassword, validatePassword);
        }
        public override async Task<IdentityResult> UpdateSecurityStampAsync(TUser user)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.UpdateSecurityStampAsync(user);
        }
        protected override async Task<IdentityResult> UpdateUserAsync(TUser user)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.UpdateUserAsync(user);
        }
        public override async Task<IdentityResult> RemovePasswordAsync(TUser user)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.RemovePasswordAsync(user);
        }
        public override async Task<IdentityResult> DeleteAsync(TUser user)
        {
            await cacheService.RemoveObjectAsync($"user_{GetUserIdAsync(user)}");
            return await base.DeleteAsync(user);
        }
        public override Task<TUser> FindByIdAsync(string? userId)
        {
            return cacheService.GetObjectAndSetAsync($"user_{userId}", () => base.FindByIdAsync(userId));
        }
    }
}
