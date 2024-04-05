using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

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

        public async Task ResetCache()
        {
            var cultureNames = await _context.Set<MultilingualCulture>().Select(_ => _.Name).ToListAsync();

            foreach (var cultureName in cultureNames)
            {
                _memoryCache.Remove(_cacheKey + cultureName);
            }
        }

        public async Task<IDictionary<string, string>> GetAllGlobalTextsAsync(string cultureName)
        {
            return await _memoryCache.GetOrCreateAsync(_cacheKey + cultureName, async (entry) =>
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
                    .OrderBy(_ => _.IsInitialData)
                    .ThenBy(_ => _.Index)
                    .ToListAsync();

                var result = texts.ToDictionary(_ => _.Key, _ => _.Text);

                return result;
            });
        }
    }
}
