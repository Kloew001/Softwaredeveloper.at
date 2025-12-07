using System.Collections.Concurrent;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Cache;

[ScopedDependency]
public class ScopedCache
{
    //https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Caching.Memory/src/MemoryCache.cs
    private readonly ConcurrentDictionary<string, object> _cache;

    public ScopedCache()
    {
        _cache = new ConcurrentDictionary<string, object>();
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
    {
        if (!_cache.TryGetValue(key, out var result))
        {
            result = await factory().ConfigureAwait(false);
            _cache.TryAdd(key, result);
        }

        return (T)result;
    }

    public T GetOrCreate<T>(string key, Func<T> factory)
    {
        if (!_cache.TryGetValue(key, out var result))
        {
            result = factory();
            _cache.TryAdd(key, result);
        }

        return (T)result;
    }
}