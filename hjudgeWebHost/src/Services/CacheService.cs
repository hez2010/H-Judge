using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface ICacheService
    {
        Task<T?> GetObjectAsync<T>(string key) where T : class;
        Task SetObjectAsync<T>(string key, T obj) where T : class;
        Task<T> GetObjectAndSetAsync<T>(string key, Func<Task<T>> func) where T : class;
    }
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache distributedCache;

        public CacheService(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public async Task<T> GetObjectAndSetAsync<T>(string key, Func<Task<T>> func) where T : class
        {
            var result = await GetObjectAsync<T>(key);
            if (result == null)
            {
                result = await func();
                if (result != null)
                {
                    await SetObjectAsync(key, result);
                }
            }
            return result;
        }

        public async Task<T?> GetObjectAsync<T>(string key) where T : class
        {
            var v = await distributedCache.GetAsync(key);
            if (v == null) return null;
            return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<T>(v);
        }

        public Task SetObjectAsync<T>(string key, T obj) where T : class
        {
            return distributedCache.SetAsync(key, SpanJson.JsonSerializer.Generic.Utf8.Serialize(obj));
        }

    }
}
