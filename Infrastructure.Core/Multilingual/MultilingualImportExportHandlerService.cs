using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Data;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    public interface IMultilingualImportExportHandlerService
    {
        Task ImportAsync(byte[] content);
        Task<Utility.FileInfo> ExportToFileAsync();
    }

    public class MultilingualConfiguration
    {
        public bool EnabledAdministration { get; set; } = true;
    }

    public abstract class MultilingualImportExportHandlerService : IMultilingualImportExportHandlerService
    {
        private readonly IDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        protected readonly IApplicationSettings _applicationSettings;

        public MultilingualImportExportHandlerService(
            IDbContext context,
            IMemoryCache memoryCache,
            IServiceScopeFactory serviceScopeFactory,
            IApplicationSettings applicationSettings)
        {
            _context = context;
            _memoryCache = memoryCache;
            _serviceScopeFactory = serviceScopeFactory;
            _applicationSettings = applicationSettings;
        }

        public abstract Task ImportAsync(byte[] content);
        public abstract Task<Utility.FileInfo> ExportToFileAsync();

        protected void CheckEnabledAndThrow()
        {
            if (_applicationSettings.Multilingual?.EnabledAdministration != true)
                throw new InvalidOperationException("Multilingual is not enabled");
        }

        protected async Task<DataSet> GetDataSetFromDB()
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
                    .Where(_ => _.EditLevel == MultilingualGlobalTextProtectionLevel.Public)
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

        public async Task ImportDataSet(DataSet dataSet)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
                await context.Set<MultilingualGlobalText>()
                    .Where(_ => _.EditLevel != MultilingualGlobalTextProtectionLevel.Private)
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

                        var multilingualService = scope.ServiceProvider.GetRequiredService(this.GetType()) as MultilingualImportExportHandlerService;

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

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var multilingualService = scope.ServiceProvider.GetRequiredService<MultilingualService>();
                await multilingualService.ResetCache();
            }
        }

        public class MultilingualRow
        {
            public string TextKey { get; set; }
            public string cultureName { get; set; }
            public string Text { get; set; }
            public int Index { get; set; }
        }

        protected async Task HandleRowImport(MultilingualRow row)
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
            multilingualText.ViewLevel =  MultilingualGlobalTextProtectionLevel.Public;
            multilingualText.EditLevel = MultilingualGlobalTextProtectionLevel.Public;
        }
    }
}
