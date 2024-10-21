using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Cache;

public interface ICacheService
{
    public IMemoryCache MemoryCache { get; }
    public ScopedCache ScopedCache(IServiceProvider serviceProvider);
    public IDistributedCache DistributedCache { get; }
}

/// <summary>
/// https://medium.com/@dayanandthombare/caching-strategies-in-net-core-5c6daf9dff2e
/// </summary>
public class CacheService : ICacheService
{
    public IMemoryCache MemoryCache { get; set; }
    public ScopedCache ScopedCache (IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<ScopedCache>();
    public IDistributedCache DistributedCache { get; set; }
    
    public CacheService(
        IMemoryCache memoryCache, 
        IDistributedCache distributedCache)
    {
        MemoryCache = memoryCache;
        DistributedCache = distributedCache;
    }
}
