using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace hjudge.Shared.Utils
{
    public class JsonNonStringKeyDictionaryConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>>
    {
        public override IDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var convertedType = typeof(Dictionary<,>)
                .MakeGenericType(typeof(string), typeToConvert.GenericTypeArguments[1]);
            var value = JsonSerializer.Deserialize(ref reader, convertedType, options);
            var instance = (Dictionary<TKey, TValue>)Activator.CreateInstance(
                typeToConvert,
                BindingFlags.Instance | BindingFlags.Public,
                null,
                null,
                CultureInfo.CurrentCulture);
            var enumerator = (IEnumerator)convertedType.GetMethod("GetEnumerator")!.Invoke(value, null);
            var parse = typeof(TKey).GetMethod("Parse", 0, BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, new[] { typeof(string) }, null);
            if (parse is null) throw new NotSupportedException($"{typeof(TKey)} as TKey in IDictionary<TKey, TValue> is not supported.");
            while (enumerator.MoveNext())
            {
                var element = (KeyValuePair<string?, TValue>)enumerator.Current;
                instance.Add((TKey)parse.Invoke(null, new[] { element.Key }), element.Value);
            }
            return instance;
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue> value, JsonSerializerOptions options)
        {
            var convertedDictionary = new Dictionary<string?, TValue>(value.Count);
            foreach (var (k, v) in value) convertedDictionary[k?.ToString()] = v;
            JsonSerializer.Serialize(writer, convertedDictionary, options);
            convertedDictionary.Clear();
        }
    }
    public class JsonNonStringKeyDictionaryConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType) return false;
            if (typeToConvert.GenericTypeArguments[0] == typeof(string)) return false;
            return typeToConvert.GetInterface("IDictionary") != null;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(JsonNonStringKeyDictionaryConverter<,>)
                .MakeGenericType(typeToConvert.GenericTypeArguments[0], typeToConvert.GenericTypeArguments[1]);
            var converter = (JsonConverter)Activator.CreateInstance(
                converterType,
                BindingFlags.Instance | BindingFlags.Public,
                null,
                null,
                CultureInfo.CurrentCulture);
            return converter;
        }
    }
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions camelOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            IgnoreNullValues = true
        };

        private static readonly JsonSerializerOptions pascalOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            AllowTrailingCommas = true,
            IgnoreNullValues = true
        };

        static JsonHelper()
        {
            camelOptions.Converters.Add(new JsonNonStringKeyDictionaryConverterFactory());
            pascalOptions.Converters.Add(new JsonNonStringKeyDictionaryConverterFactory());
        }

        public static byte[] SerializeJson<T>(this T obj, bool camel = true)
        {
            return Encoding.UTF8.GetBytes(camel switch
            {
                true => JsonSerializer.Serialize(obj, camelOptions),
                false => JsonSerializer.Serialize(obj, pascalOptions)
            });
        }
        public static string SerializeJsonAsString<T>(this T obj, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Serialize(obj, camelOptions),
                false => JsonSerializer.Serialize(obj, pascalOptions)
            };
        }
        public static T DeserializeJson<T>(this ReadOnlySpan<byte> data, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize<T>(data, camelOptions),
                false => JsonSerializer.Deserialize<T>(data, pascalOptions)
            };
        }
        public static T DeserializeJson<T>(this byte[] data, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize<T>(data, camelOptions),
                false => JsonSerializer.Deserialize<T>(data, pascalOptions)
            };
        }
        public static T DeserializeJson<T>(this string str, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize<T>(str, camelOptions),
                false => JsonSerializer.Deserialize<T>(str, pascalOptions)
            };
        }

        public static object DeserializeJson(this ReadOnlySpan<byte> data, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize(data, type, camelOptions),
                false => JsonSerializer.Deserialize(data, type, pascalOptions)
            };
        }
        public static object DeserializeJson(this byte[] data, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize(data, type, camelOptions),
                false => JsonSerializer.Deserialize(data, type, pascalOptions)
            };
        }
        public static object DeserializeJson(this string str, Type type, bool camel = true)
        {
            return camel switch
            {
                true => JsonSerializer.Deserialize(str, type, camelOptions),
                false => JsonSerializer.Deserialize(str, type, pascalOptions)
            };
        }
    }
}
