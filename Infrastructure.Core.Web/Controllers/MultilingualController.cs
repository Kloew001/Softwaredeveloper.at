using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

public class MultilingualController : BaseApiController
{
    [AllowAnonymous]
    [HttpGet]
    [Route("/api/multilingual/{culture}")]
    public IDictionary<string, string> GetAllGlobalText(
        [FromServices] MultilingualGlobalTextCacheService cacheService,
        [FromRoute(Name = "culture")] string cultureName = "de")
        => cacheService.GetTexts(cultureName, MultilingualProtectionLevel.Public)
            .ToDictionary(_=>_.Key, _=>_.Text);

    [AllowAnonymous]
    [HttpGet]
    public async Task<FileContentResult> ExportExcel([FromServices] ExcelMultilingualService service)
    {
        var fileInfo = await service.ExportToFileAsync();

        return File(fileInfo.Content, fileInfo.FileContentType, fileInfo.FileName);
    }

    [AllowAnonymous]
    [HttpPost]
    [EnableRateLimiting("largeFileUpload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task ImportExcel(IFormFile file, [FromServices] ExcelMultilingualService service)
    {
        await service.ImportAsync(file.GetContent());
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<FileContentResult> ExportJson([FromServices] JsonMultilingualService service)
    {
        var fileInfo = await service.ExportToFileAsync();

        return File(fileInfo.Content, fileInfo.FileContentType, fileInfo.FileName);
    }

    [AllowAnonymous]
    [HttpPost]
    [EnableRateLimiting("largeFileUpload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task ImportJson(IFormFile file, [FromServices] JsonMultilingualService service)
    {
        await service.ImportAsync(file.GetContent());
    }
}
