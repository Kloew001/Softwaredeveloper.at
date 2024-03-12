using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using System.Data;
using Newtonsoft.Json;
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
                    .OrderBy(_=>_.IsInitialData)
                    .ThenBy(_ => _.Index)
                    .ToListAsync();

                var result = texts.ToDictionary(_ => _.Key, _ => _.Text);

                return result;
            });
        }

        public async Task<Utility.FileInfo> ExportExcelAsync()
        {
            var dataSet = await GetDataSetFromDB();

            var excelContent = ExcelUtility.GetExcelFromDataSet(dataSet);

            return new Utility.FileInfo()
            {
                Content = excelContent,
                FileContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = "Multilingual.xlsx"
            };
        }

        private async Task<DataSet> GetDataSetFromDB()
        {
            var cultures = await _context.Set<MultilingualCulture>()
                .Where(_ => _.IsActive)
                .OrderBy(_ => _.IsDefault)
                .ThenBy(_ => _.Name)
                .Select(_ => _.Name)
                .ToListAsync();

            var dataSet = new DataSet();

            foreach (var culture in cultures)
            {
                var dataTable = new DataTable();
                dataTable.TableName = culture;
                dataTable.Columns.Add(new DataColumn("TextKey"));
                dataTable.Columns.Add(new DataColumn("Text"));

                var texte = await _context.Set<MultilingualGlobalText>()
                    .Where(_ => _.Culture.Name == culture)
                    .Where(_ => _.IsInitialData == false)
                    .OrderBy(_ => _.Index)
                    .ToListAsync();

                foreach (var text in texte)
                {
                    DataRow dataRow = dataTable.NewRow();

                    dataRow["TextKey"] = text.Key;
                    dataRow["Text"] = text.Text;

                    dataTable.Rows.Add(dataRow);
                }

                dataSet.Tables.Add(dataTable);
            }

            return dataSet;
        }

        private DataSet GetDataSetFromJson(byte[] jsonContent)
        {
            string json = System.Text.Encoding.UTF8.GetString(jsonContent);

            var cultures = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

            var dataSet = new DataSet();

            foreach (var culture in cultures)
            {
                var dataTable = new DataTable();
                dataTable.TableName = culture.Key;

                dataTable.Columns.Add(new DataColumn("TextKey"));
                dataTable.Columns.Add(new DataColumn("Text"));

                foreach (var text in culture.Value)
                {
                    DataRow dataRow = dataTable.NewRow();

                    dataRow["TextKey"] = text.Key;
                    dataRow["Text"] = text.Value;

                    dataTable.Rows.Add(dataRow);
                }

                dataSet.Tables.Add(dataTable);
            }

            return dataSet;
        }

        public Task ImportExcel(byte[] excelContent)
        {
            var dataSet = ExcelUtility.GetDataSetFromExcel(excelContent);
            return ImportDataSet(dataSet);
        }

        public Task ImportJson(byte[] jsonContent)
        {
            var dataSet = GetDataSetFromJson(jsonContent);
            return ImportDataSet(dataSet);
        }

        public async Task ImportDataSet(DataSet dataSet)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
                await context.Set<MultilingualGlobalText>()
                    .Where(_=> _.IsInitialData == false)
                    .ExecuteDeleteAsync();
            }

            foreach (var dataTable in dataSet.Tables.Convert<DataTable>())
            {
                var i = 0;
                foreach (var rowBatch in dataTable.Rows.Convert<DataRow>().Batch(100))
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

                        var multilingualService = scope.ServiceProvider.GetRequiredService<MultilingualService>();

                        foreach (var row in rowBatch)
                        {
                            await multilingualService.HandleRowImport(new MultilingualRow()
                            {
                                cultureName = dataTable.TableName,
                                TextKey = row.Field<string>("TextKey"),
                                Text = row.Field<string>("Text"),
                                Index = i++,
                            });
                        }

                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public class MultilingualRow
        {
            public string TextKey { get; set; }
            public string cultureName { get; set; }
            public string Text { get; set; }
            public int Index { get; set; }
        }

        private async Task HandleRowImport(MultilingualRow row)
        {
            var cultureName = row.cultureName;

            var cultureId = await _memoryCache.GetOrCreateAsync($"{HandleRowImport}_{cultureName}", async (entry) =>
            {
                return await _context.Set<MultilingualCulture>()
                    .Where(_ => _.Name == cultureName)
                    .Select(_ => _.Id)
                    .SingleAsync();
            });

            var textKey = row.TextKey;

            var multilingualText = await
                _context.Set<MultilingualGlobalText>()
                    .Where(_ => _.CultureId == cultureId &&
                                _.Key == textKey)
                    .SingleOrDefaultAsync();

            if (multilingualText == null)
            {
                multilingualText = await _context.CreateEntity<MultilingualGlobalText>();

                multilingualText.CultureId = cultureId;
                multilingualText.Key = textKey;
            }

            multilingualText.Text = row.Text;
            multilingualText.Index = row.Index;
            multilingualText.IsInitialData = false;
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

            var multilingualService = scope.ServiceProvider.GetService<MultilingualService>();

            await multilingualService.ImportJson(FileUtiltiy.GetContent(filePath));

            _reloadJson = false;
        }
    }
}
