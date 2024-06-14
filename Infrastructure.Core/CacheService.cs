using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface ICacheService
    {
        public IMemoryCache MemoryCache { get; }
        public IDistributedCache DistributedCache { get; }
    }

    /// <summary>
    /// https://medium.com/@dayanandthombare/caching-strategies-in-net-core-5c6daf9dff2e
    /// </summary>
    public class CacheService : ICacheService
    {
        public IDistributedCache DistributedCache { get; set; }
        public IMemoryCache MemoryCache { get; set; }

        public CacheService(IMemoryCache memoryCache, IDistributedCache distributedCache)
        {
            MemoryCache = memoryCache;
            DistributedCache = distributedCache;
        }
    }

    public static class DistributedCacheExtensions
    {
        public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache distributedCache, string key, Func<DistributedCacheEntryOptions, Task<T>> factory)
        {
            var value = await distributedCache.GetValueAsync<T>(key);

            if (value == null)
            {
                var cacheOptions = new DistributedCacheEntryOptions();

                value = await factory(cacheOptions);

                await distributedCache.SetAsync(key, value, cacheOptions);
            }

            return value;
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value)
        {
            return SetAsync(cache, key, value, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, GetJsonSerializerOptions()));
            return cache.SetAsync(key, bytes, options);
        }

        public static async Task<T> GetValueAsync<T>(this IDistributedCache cache, string key)
        {
            var val = await cache.GetAsync(key);

            if (val == null)
                return default;

            var value = JsonSerializer.Deserialize<T>(val, GetJsonSerializerOptions());
            return value;
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions()
            {
                PropertyNamingPolicy = null,
                WriteIndented = true,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
        }
    }
}
