using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using System.Data;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    public interface IMultilingualImportExportHandlerService
    {
        Task ImportAsync(byte[] content);
        Task<Utility.FileInfo> ExportToFileAsync();
    }

    public class ExcelMultilingualService : MultilingualImportExportHandlerService, IScopedDependency
    {
        public ExcelMultilingualService(IDbContext context, IMemoryCache memoryCache, IServiceScopeFactory serviceScopeFactory)
            : base(context, memoryCache, serviceScopeFactory)
        {
        }

        public override Task ImportAsync(byte[] content)
        {
            var dataSet = ExcelUtility.GetDataSetFromExcel(content);
            return ImportDataSet(dataSet);
        }

        public override async Task<Utility.FileInfo> ExportToFileAsync()
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
    }

    public class JsonMultilingualService : MultilingualImportExportHandlerService, IScopedDependency
    {
        public JsonMultilingualService(IDbContext context, IMemoryCache memoryCache, IServiceScopeFactory serviceScopeFactory)
            : base(context, memoryCache, serviceScopeFactory)
        {
        }

        public override Task ImportAsync(byte[] content)
        {
            var dataSet = GetDataSetFromContent(content);
            return ImportDataSet(dataSet);
        }

        public override async Task<Utility.FileInfo> ExportToFileAsync()
        {
            var jsonObj = new Dictionary<string, Dictionary<string, string>>();

            var dataSet = await GetDataSetFromDB();

            foreach (DataTable dataTable in dataSet.Tables)
            {
                var culture = new Dictionary<string, string>();

                foreach (DataRow row in dataTable.Rows)
                {
                    culture.Add(row.Field<string>("TextKey"), row.Field<string>("Text"));
                }

                jsonObj.Add(dataTable.TableName, culture);
            }

            var jsonContent = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

            return new Utility.FileInfo()
            {
                Content = System.Text.Encoding.UTF8.GetBytes(jsonContent),
                FileContentType = "application/json",
                FileName = "multilingual.json"
            };
        }

        private DataSet GetDataSetFromContent(byte[] jsonContent)
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

    }

    public abstract class MultilingualImportExportHandlerService : IMultilingualImportExportHandlerService
    {
        private readonly IDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MultilingualImportExportHandlerService(IDbContext context, IMemoryCache memoryCache, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _memoryCache = memoryCache;
            _serviceScopeFactory = serviceScopeFactory;
        }


        public abstract Task ImportAsync(byte[] content);
        public abstract Task<Utility.FileInfo> ExportToFileAsync();

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

        public async Task ImportDataSet(DataSet dataSet)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
                await context.Set<MultilingualGlobalText>()
                    .Where(_ => _.IsInitialData == false)
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
            multilingualText.IsInitialData = false;
        }
    }
}
