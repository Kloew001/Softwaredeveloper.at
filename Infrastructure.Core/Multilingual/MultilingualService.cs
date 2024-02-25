using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
using SoftwaredeveloperDotAt.Infrastructure.Core.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.Extensions.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using System.Data;
using DocumentFormat.OpenXml.InkML;

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
                    .ToListAsync();

                var result = texts.ToDictionary(_ => _.Key, _ => _.Text);

                return result;
            });
        }

        public async Task ImportExcel(byte[] excelContent)
        {
            var dataTable = ExcelUtility.GetDataSetFromExcel(excelContent).Tables[0];

            foreach (DataRow row in dataTable.Rows)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var multilingualService = scope.ServiceProvider.GetRequiredService<MultilingualService>();
                    await multilingualService.HandleDataRowImport(row);
                }
            }
        }

        private async Task HandleDataRowImport(DataRow row)
        {
            var cultureName = row.Field<string>("Culture");

            var cultureId = await _memoryCache.GetOrCreateAsync($"{HandleDataRowImport}_{cultureName}", async (entry) =>
            {
                return await _context.Set<MultilingualCulture>()
                    .Where(_ => _.Name == cultureName)
                    .Select(_ => _.Id)
                    .SingleAsync();
            });

            var key = row.Field<string>("TextKey");

            var multilingualText = await
                _context.Set<MultilingualGlobalText>()
                .Where(_ => _.CultureId == cultureId &&
                            _.Key == key)
                .SingleOrDefaultAsync();

            if (multilingualText == null)
            {
                multilingualText = await _context.CreateEntity<MultilingualGlobalText>();

                multilingualText.CultureId = cultureId;
                multilingualText.Key = key;
            }

            multilingualText.Text = row.Field<string>("Text");

            await _context.SaveChangesAsync();
        }
    }
}
