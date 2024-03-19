using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

    public class ICurrentLanguageService : IScopedDependency
    {
        MultilingualCulture Current { get; set; }
    }

    public class CurrentLanguageService : ICurrentLanguageService
    {
        public MultilingualCulture Current { get; set; }
    }

    public class MultilingualService : IScopedDependency
    {
        private readonly IDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MultilingualService(IDbContext context, IMemoryCache memoryCache, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _memoryCache = memoryCache;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public const string _cacheKey = $"{nameof(MultilingualService)}_{nameof(GetAllGlobalTextsAsync)}_";

        public async Task<IDictionary<string, string>> GetAllGlobalTextsAsync(string cultureName)
        {
            return await _memoryCache.GetOrCreateAsync(_cacheKey + cultureName, async (entry) =>
            {
#if DEBUG
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
#else
                entry.SlidingExpiration = TimeSpan.FromHours(10);
#endif
                var culture = await _context.Set<MultilingualCulture>()
                .SingleOrDefaultAsync(_ => _.Name == cultureName);

                if (culture == null)
                    culture = await _context.Set<MultilingualCulture>()
                    .FirstOrDefaultAsync(_ => _.Name.StartsWith(cultureName));

                if (culture == null)
                    throw new InvalidOperationException($"Culture '{cultureName}' not found.");

                var texts = await _context.Set<MultilingualGlobalText>()
                    .Where(_ => _.CultureId == culture.Id)
                    .OrderBy(_ => _.IsInitialData)
                    .ThenBy(_ => _.Index)
                    .ToListAsync();

                var result = texts.ToDictionary(_ => _.Key, _ => _.Text);

                return result;
            });
        }
    }

    /* 
    HOW TO USE:
     
    public class MultilingualDataSeedHostedService : BaseMultilingualDataSeedHostedService
    {
        public MultilingualDataSeedHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<BaseMultilingualDataSeedHostedService> logger, IApplicationSettings applicationSettings, IHostApplicationLifetime appLifetime) : base(serviceScopeFactory, logger, applicationSettings, appLifetime)
        {
        }

        protected override string GetFileName()
        {
            return $"{Path.GetDirectoryName(typeof(MultilingualDataSeedHostedService).Assembly.Location)}\\Sections\\MultilingualSection\\Content\\{"Multilingual.json"}";
        }
    }
     */
    public abstract class BaseMultilingualDataSeedHostedService : TimerHostedService
    {
        public BaseMultilingualDataSeedHostedService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<BaseMultilingualDataSeedHostedService> logger,
            IApplicationSettings applicationSettings,
            IHostApplicationLifetime appLifetime)
            : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
        {
            BackgroundServiceInfoEnabled = false;

            string filePath = GetFileName();

            _watcher = new FileSystemWatcher(System.IO.Path.GetDirectoryName(filePath));

            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            _watcher.Filter = "*.json";
            _watcher.EnableRaisingEvents = true;

            _watcher.Changed += (sender, e) =>
            {
                _reloadJson = true;
            };
        }

        private FileSystemWatcher _watcher;
        private bool _reloadJson = true;

        //return $"{Path.GetDirectoryName(typeof(MultilingualDataSeedHostedService).Assembly.Location)}\\Sections\\MultilingualSection\\Content\\{"Multilingual.json"}";
        protected abstract string GetFileName();

        protected override async Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            if (_reloadJson == false)
                return;

            string filePath = GetFileName();

            var multilingualService = scope.ServiceProvider.GetService<JsonMultilingualService>();

            await multilingualService.ImportAsync(FileUtiltiy.GetContent(filePath));

            _reloadJson = false;
        }
    }
}
