using CacheManager.Core;
using CacheManager.Core.Internal;
using hjudgeShared.Utils;
using SpanJson;
using System;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
#nullable disable
    class JsonCacheItem<T> : SerializerCacheItem<T>
    {
        [JsonConstructor]
        public JsonCacheItem() { }

        public JsonCacheItem(ICacheItemProperties properties, object value) : base(properties, value) { }

        public override long CreatedUtc { get; set; }
        public override ExpirationMode ExpirationMode { get; set; }
        public override double ExpirationTimeout { get; set; }
        public override string Key { get; set; } = string.Empty;
        public override long LastAccessedUtc { get; set; }
        public override string Region { get; set; } = string.Empty;
        public override bool UsesExpirationDefaults { get; set; }
        public override string ValueType { get; set; } = string.Empty;
        public override T Value { get; set; }
    }
#nullable enable
    public class JsonSerializer : CacheSerializer
    {
        private static readonly Type _openGenericItemType = typeof(JsonCacheItem<>);
        public override object Deserialize(byte[] data, Type target)
        {
            return data.DeserializeJson(target, false);
        }

        public override byte[] Serialize<T>(T value)
        {
            return value.SerializeJson(false);
        }

        protected override object CreateNewItem<TCacheValue>(ICacheItemProperties properties, object value)
        {
            return new JsonCacheItem<TCacheValue>(properties, value);
        }

        protected override Type GetOpenGeneric()
        {
            return _openGenericItemType;
        }
    }
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
            if (v == null)
#nullable disable
                return (false, default);
#nullable enable

            try
            {
                return (true, v.DeserializeJson<T>(false));
            }
            catch
            {
#nullable disable
                await RemoveObjectAsync(key);
                return (false, default);
#nullable enable
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
                distributedCache.AddOrUpdate(key, obj.SerializeJsonAsString(false), _ => obj.SerializeJsonAsString(false));
            }
            catch { /* ignored */ }
            return Task.CompletedTask;
        }
    }
}
