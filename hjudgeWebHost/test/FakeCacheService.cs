using hjudgeWebHost.Services;
using System;
using System.Threading.Tasks;

namespace hjudgeWebHostTest
{
    public class FakeCacheService : ICacheService
    {
        public Task<T> GetObjectAndSetAsync<T>(string key, Func<Task<T>> func)
        {
            return func();
        }

        public Task<(bool Succeeded, T Result)> GetObjectAsync<T>(string key)
        {
            return Task.FromResult((false, default(T)));
        }

        public Task RefreshObjectAsync(string key)
        {
            return Task.CompletedTask;
        }

        public Task<bool> RemoveObjectAsync(string key)
        {
            return Task.FromResult(true);
        }

        public Task SetObjectAsync<T>(string key, T obj)
        {
            return Task.CompletedTask;
        }
    }
}
