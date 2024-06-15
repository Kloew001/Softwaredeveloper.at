using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Cache
{
    public interface ICacheService
    {
        public IMemoryCache MemoryCache { get; }
        public ScopedCache ScopedCache { get; }
        public IDistributedCache DistributedCache { get; }
    }

    /// <summary>
    /// https://medium.com/@dayanandthombare/caching-strategies-in-net-core-5c6daf9dff2e
    /// </summary>
    public class CacheService : ICacheService
    {
        public IMemoryCache MemoryCache { get; set; }
        public ScopedCache ScopedCache { get; set; }
        public IDistributedCache DistributedCache { get; set; }
        
        public CacheService(
            IMemoryCache memoryCache, 
            ScopedCache scopedCache,
            IDistributedCache distributedCache)
        {
            MemoryCache = memoryCache;
            ScopedCache = scopedCache;
            DistributedCache = distributedCache;
        }
    }
}
