using hjudgeWebHost.Utils;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface ICacheService
    {
        Task<(bool Succeeded, T Result)> GetObjectAsync<T>(string key);
        Task SetObjectAsync<T>(string key, T obj);
        Task<T> GetObjectAndSetAsync<T>(string key, Func<Task<T>> func);
        Task RemoveObjectAsync(string key);
        Task RefreshObjectAsync<T>(string key);
    }
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache distributedCache;

        public CacheService(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public async Task<T> GetObjectAndSetAsync<T>(string key, Func<Task<T>> func)
        {
            var (succeeded, result) = await GetObjectAsync<T>(key);
            if (!succeeded)
            {
                result = await func();
                if (result != null)
                {
                    await SetObjectAsync(key, result);
                }
            }
            return result;
        }

        public async Task<(bool Succeeded, T Result)> GetObjectAsync<T>(string key)
        {
            var v = await distributedCache.GetAsync(key);
#nullable disable
            if (v == null) return (false, default);
#nullable enable
            return (true, v.DeserializeJson<T>(false));
        }

        public Task RefreshObjectAsync<T>(string key)
        {
            return distributedCache.RefreshAsync(key);
        }

        public Task RemoveObjectAsync(string key)
        {
            return distributedCache.RemoveAsync(key);
        }

        public Task SetObjectAsync<T>(string key, T obj)
        {
            return distributedCache.SetAsync(key, obj.SerializeJson(false), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4) });
        }
    }
}
