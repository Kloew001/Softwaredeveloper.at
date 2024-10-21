using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

public interface ICurrentLanguageService
{
    Guid CurrentCultureId { get; }
}

[ScopedDependency<ICurrentLanguageService>]
public class CurrentLanguageService : ICurrentLanguageService
{
    public Guid CurrentCultureId => GetCurrentCultureId();

    private readonly IDefaultLanguageService _defaultLanguageService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _memoryCache;

    public CurrentLanguageService(
        IDefaultLanguageService defaultLanguageService,
        IServiceProvider serviceProvider,
        IMemoryCache memoryCache)
    {
        _defaultLanguageService = defaultLanguageService;
        _serviceProvider = serviceProvider;
        _memoryCache = memoryCache;
    }

    public const string _getCurrentCultureIdCacheKey = $"{nameof(GetCurrentCultureId)}_";

    public void RemoveCache(Guid currentUserId)
    {
        _memoryCache.Remove(_getCurrentCultureIdCacheKey + currentUserId);
    }

    public Guid GetCurrentCultureId()
    {
        var currentUserId = _serviceProvider.GetService<ICurrentUserService>()
            .GetCurrentUserId();

        return _memoryCache.GetOrCreate(_getCurrentCultureIdCacheKey + currentUserId, (entry) =>
        {
            var preferedCultureId = _serviceProvider.GetService<IDbContext>()
                .Set<ApplicationUser>()
                .Where(_ => _.Id == currentUserId)
                .Select(_ => _.PreferedCultureId)
                .SingleOrDefault();

            if (preferedCultureId.HasValue)
            {
                return preferedCultureId.Value;
            }
            else
            {
                return _defaultLanguageService.Culture.Id.Value;
            }
        });
    }
}
