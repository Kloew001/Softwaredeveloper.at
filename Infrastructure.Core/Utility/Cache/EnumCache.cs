using Microsoft.Extensions.Caching.Memory;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Cache
{
    public class EnumCacheItem
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string EnumValue { get; set; }

        public T GetEnumType<T>()
            where T : struct
        {
            return Enum.Parse<T>(EnumValue);
        }
    }

    [SingletonDependency]
    public class EnumCache
    {
        private readonly IMemoryCache _memoryCache;

        public EnumCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public EnumCacheItem[] GetEnums<TEnum>()
            where TEnum : struct, Enum
        {
            var cacheKey = $"{nameof(EnumCacheItem)}_{nameof(GetEnums)}_{typeof(TEnum)}";

            return _memoryCache.GetOrCreate(cacheKey, (Func<ICacheEntry, EnumCacheItem[]>)((entry) =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(24);

                return Enum.GetValues<TEnum>()
                    .Select((Func<TEnum, EnumCacheItem>)(_ => new EnumCacheItem()
                    {
                        Id = _.GetId(),
                        DisplayName = _.GetDisplayName(),
                        EnumValue = _.ToString()
                    }))
                    .ToArray();
            }));
        }
    }
}
