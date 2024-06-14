using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Cache
{
    public static class MemoryCacheExtensions
    {
        private static IDictionary GetEntries(this MemoryCache memoryCache)
        {
            var _coherentState =
                typeof(MemoryCache)
                .GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(memoryCache);

            var _entries = (IDictionary)
                _coherentState
                .GetType()
                .GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_coherentState);

            return _entries;
        }

        public static IEnumerable GetKeys(this IMemoryCache memoryCache) =>
            GetEntries((MemoryCache)memoryCache).Keys;

        public static IEnumerable<T> GetKeys<T>(this IMemoryCache memoryCache) =>
            GetKeys(memoryCache).OfType<T>();

        public static void RemoveStartsWith(this IMemoryCache memoryCache, string key) =>
            memoryCache.GetKeys<string>()
                .Where(_ => _.StartsWith(key))
                .ToList()
                .ForEach(_ => memoryCache.Remove(_));
    }

    public class ScopedCache : IScopedDependency
    {
        //https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Caching.Memory/src/MemoryCache.cs
        private readonly ConcurrentDictionary<string, object> _cache;

        public ScopedCache()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
        {
            if (!_cache.TryGetValue(key, out object result))
            {
                result = await factory().ConfigureAwait(false);
                _cache.TryAdd(key, result);
            }

            return (T)result;
        }

        public T GetOrCreate<T>(string key, Func<T> factory)
        {
            if (!_cache.TryGetValue(key, out object result))
            {
                result = factory();
                _cache.TryAdd(key, result);
            }

            return (T)result;
        }
    }

}
