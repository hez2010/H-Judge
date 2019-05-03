using CacheManager.Core;
using CacheManager.Core.Internal;
using hjudge.Shared.Utils;
using SpanJson;
using System;

namespace hjudge.Shared.Caching
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
    public class CacheItemJsonSerializer : CacheSerializer
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
}
