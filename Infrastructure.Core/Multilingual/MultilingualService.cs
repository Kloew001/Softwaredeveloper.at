using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete;

using System.Data;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    public interface IMultiLingualEntity<TTranslation>
        where TTranslation : class, IEntityTranslation
    {
        ICollection<TTranslation> Translations { get; set; }
    }

    public interface IEntityTranslation
    {
        Guid CoreId { get; set; }
        MultilingualCulture Culture { get; set; }
    }

    public interface IEntityTranslation<TEntity> : IEntityTranslation
        where TEntity : Entity
    {
        TEntity Core { get; set; }
    }

    public abstract class EntityTranslation<TEntity> : Entity, IEntityTranslation<TEntity>
        where TEntity : Entity
    {
        public Guid CoreId { get; set; }
        public virtual TEntity Core { get; set; }

        public Guid CultureId { get; set; }
        public virtual MultilingualCulture Culture { get; set; }
    }

    public interface IDefaultLanguageService : IAppStatupInit, ITypedSingletonDependency<IAppStatupInit>
    {
        string CultureName { get; set; }
    }

    public class DefaultLanguageService : IDefaultLanguageService, ITypedSingletonDependency<IDefaultLanguageService>
    {
        public string CultureName { get; set; }

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DefaultLanguageService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Init()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

                CultureName = await context.Set<MultilingualCulture>()
                    .Where(_ => _.IsDefault)
                    .Select(_ => _.Name)
                    .SingleAsync();
            }
        }
    }


    public interface ICurrentLanguageService
    {
        string CurrentCultureName { get; }

        void Init();
    }

    public class CurrentLanguageService : ICurrentLanguageService, ITypedScopedDependency<ICurrentLanguageService>
    {
        public string CurrentCultureName
        {
            get
            {
                if (_currentCultureName.IsNull())
                {
                    Init();
                }

                return _currentCultureName;
            }
            private set
            {
                _currentCultureName = value;
            }
        }
        private string _currentCultureName;

        private readonly IDefaultLanguageService _defaultLanguageService;
        private readonly ICurrentUserService _currentUserService;

        public CurrentLanguageService(
            IDefaultLanguageService defaultLanguageService,
            IServiceProvider serviceProvider)
        {
            _defaultLanguageService = defaultLanguageService;
            _currentUserService = serviceProvider.GetService<ICurrentUserService>();
        }

        public void Init()
        {
            CurrentCultureName = _defaultLanguageService.CultureName;
        }
    }

    public static class MultilingualServiceExtensions
    {
        public static Task<string> GetTextAsync<TEntity>(this EntityService<TEntity> service, string key, string cultureName = null)
            where TEntity : Entity
        {
            var multilingualService = 
            service.EntityServiceDependency.ServiceProvider
                .GetRequiredService<MultilingualService>();

            return multilingualService.GetTextAsync(key, cultureName);
        }
    }

    public class MultilingualService : IScopedDependency
    {
        private readonly IDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICurrentLanguageService _currentLanguageService;

        public MultilingualService(
            IDbContext context,
            IMemoryCache memoryCache,
            IServiceScopeFactory serviceScopeFactory,
            ICurrentLanguageService currentLanguageService)
        {
            _context = context;
            _memoryCache = memoryCache;
            _serviceScopeFactory = serviceScopeFactory;
            _currentLanguageService = currentLanguageService;
        }

        public const string _cacheKey = $"{nameof(MultilingualService)}_{nameof(GetTextsAsync)}_";

        public async Task ResetCache()
        {
            var cultureNames = await _context.Set<MultilingualCulture>().Select(_ => _.Name).ToListAsync();

            foreach (var cultureName in cultureNames)
            {
                foreach (var protectionLevel in Enum.GetValues<MultilingualGlobalTextProtectionLevel>())
                {
                    _memoryCache.Remove(_cacheKey + cultureName + protectionLevel);
                }
            }
        }

        public async Task<IDictionary<string, string>> GetTextsAsync(string cultureName = null, MultilingualGlobalTextProtectionLevel multilingualGlobalTextProtectionLevel = MultilingualGlobalTextProtectionLevel.Public)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                cultureName = _currentLanguageService.CurrentCultureName;

            return await _memoryCache.GetOrCreateAsync(_cacheKey + cultureName + multilingualGlobalTextProtectionLevel, async (entry) =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(10);

                var culture = await _context.Set<MultilingualCulture>()
                    .SingleOrDefaultAsync(_ => _.Name == cultureName);

                if (culture == null)
                    culture = await _context.Set<MultilingualCulture>()
                    .FirstOrDefaultAsync(_ => _.Name.StartsWith(cultureName));

                if (culture == null)
                    throw new InvalidOperationException($"Culture '{cultureName}' not found.");

                var texts = await _context.Set<MultilingualGlobalText>()
                    .Where(_ => _.CultureId == culture.Id)
                    .Where(_=>_.ViewLevel == multilingualGlobalTextProtectionLevel)
                    .OrderBy(_ => _.ViewLevel)
                    .ThenBy(_ => _.Index)
                    .ToListAsync();

                var result = texts.ToDictionary(_ => _.Key, _ => _.Text);

                return result;
            });
        }

        public async Task<string> GetTextAsync(string key, string cultureName = null)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                cultureName = _currentLanguageService.CurrentCultureName;

            var text = await _context.Set<MultilingualGlobalText>()
                .Where(_ => _.Key == key &&
                            _.Culture.Name == cultureName)
                .Select(_ => _.Text)
                .SingleOrDefaultAsync();

            return text;
        }
    }
}
