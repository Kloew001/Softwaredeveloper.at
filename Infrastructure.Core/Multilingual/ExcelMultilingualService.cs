using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

[ScopedDependency]
public class ExcelMultilingualService : MultilingualImportExportHandlerService
{
    public ExcelMultilingualService(IDbContext context, IMemoryCache memoryCache, IServiceScopeFactory serviceScopeFactory, IApplicationSettings applicationSettings)
        : base(context, memoryCache, serviceScopeFactory, applicationSettings)
    {
    }

    public override Task ImportAsync(byte[] content)
    {
        CheckEnabledAndThrow();

        var dataSet = ExcelUtility.GetDataSetFromExcel(content);
        return ImportDataSet(dataSet);
    }

    public override async Task<Utility.FileInfo> ExportToFileAsync()
    {
        CheckEnabledAndThrow();

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
