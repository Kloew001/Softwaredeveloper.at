using Microsoft.Extensions.Caching.Memory;

using System.Collections;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Cache;

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
