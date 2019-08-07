using CacheManager.Core;
using hjudge.Shared.Utils;
using System;
using System.Threading.Tasks;

namespace hjudge.WebHost.Services
{
    public interface ICacheService
    {
        Task<(bool Succeeded, T Result)> GetObjectAsync<T>(string key);
        Task SetObjectAsync<T>(string key, T obj);
        Task<T> GetObjectAndSetAsync<T>(string key, Func<Task<T>> func);
        Task<bool> RemoveObjectAsync(string key);
    }
    public class CacheService : ICacheService
    {
        private readonly ICacheManager<string> distributedCache;

        public CacheService(ICacheManager<string> distributedCache)
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
            var v = distributedCache.Get(key);
            if (v == null) return (false, default);

            try
            {
                return (true, v.DeserializeJson<T>(false));
            }
            catch
            {
                await RemoveObjectAsync(key);
                return (false, default);
            }
        }

        public Task<bool> RemoveObjectAsync(string key)
        {
            return Task.FromResult(distributedCache.Remove(key));
        }

        public Task SetObjectAsync<T>(string key, T obj)
        {
            try
            {
                //TODO: remove exception once bugs in System.Text.Json fixed
                throw new NotImplementedException();
                distributedCache.AddOrUpdate(key, obj.SerializeJsonAsString(false), _ => obj.SerializeJsonAsString(false));
            }
            catch { /* ignored */ }
            return Task.CompletedTask;
        }
    }
}
