using System.Data;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

[ScopedDependency]
public class JsonMultilingualService : MultilingualImportExportHandlerService
{
    public JsonMultilingualService(IDbContext context, IMemoryCache memoryCache, IServiceScopeFactory serviceScopeFactory, IApplicationSettings applicationSettings)
        : base(context, memoryCache, serviceScopeFactory, applicationSettings)
    {
    }

    public override Task ImportAsync(byte[] content)
    {
        CheckEnabledAndThrow();

        var dataSet = GetDataSetFromContent(content);
        return ImportDataSet(dataSet);
    }

    public override async Task<Utility.FileInfo> ExportToFileAsync()
    {
        CheckEnabledAndThrow();

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
        var json = System.Text.Encoding.UTF8.GetString(jsonContent);

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
                var dataRow = dataTable.NewRow();

                dataRow["TextKey"] = text.Key;
                dataRow["Text"] = text.Value;

                dataTable.Rows.Add(dataRow);
            }

            dataSet.Tables.Add(dataTable);
        }

        return dataSet;
    }
}