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
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices;

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
                    .OrderBy(_ => _.Key)
                    .ToListAsync();

                var result = texts.ToDictionary(_ => _.Key, _ => _.Text);

                return result;
            });
        }

        public Task ImportJson(byte[] jsonContent)
        {
            string json = System.Text.Encoding.UTF8.GetString(jsonContent);

            var cultures = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("TextKey"));
            dataTable.Columns.Add(new DataColumn("Culture"));
            dataTable.Columns.Add(new DataColumn("Text"));

            foreach (var culture in cultures)
            {
                foreach (var text in culture.Value)
                {
                    DataRow dataRow = dataTable.NewRow();

                    dataRow["TextKey"] = text.Key;
                    dataRow["Culture"] = culture.Key;
                    dataRow["Text"] = text.Value;

                    dataTable.Rows.Add(dataRow);
                }

            }

            return ImportDataTable(dataTable);
        }

        public Task ImportExcel(byte[] excelContent)
        {
            var dataTable = ExcelUtility.GetDataSetFromExcel(excelContent).Tables[0];

            return ImportDataTable(dataTable);
        }

        public async Task ImportDataTable(DataTable dataTable)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
                await context.Set<MultilingualGlobalText>().ExecuteDeleteAsync();
            }

            foreach (var rowBatch in dataTable.Rows.Convert<DataRow>().Batch(100))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

                    var multilingualService = scope.ServiceProvider.GetRequiredService<MultilingualService>();

                    foreach (var row in rowBatch)
                    {
                        await multilingualService.HandleDataRowImport(row);
                    }

                    await context.SaveChangesAsync();
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

            var key = row.Field<string>("TextKey");//.ReformatToUpper();

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

            var multilingualService = scope.ServiceProvider.GetService<MultilingualService>();

            await multilingualService.ImportJson(FileUtiltiy.GetContent(filePath));

            _reloadJson = false;
        }
    }
}
