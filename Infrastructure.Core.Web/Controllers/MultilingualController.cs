using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers
{
    public class MultilingualController : BaseApiController
    {
        private readonly MultilingualService _service;

        public MultilingualController(MultilingualService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/api/multilingual/{culture}")]
        public Task<IDictionary<string, string>> GetAllGlobalText(
            [FromRoute(Name = "culture")] string cultureName = "de")
            => _service.GetAllGlobalTextsAsync(cultureName);


        [AllowAnonymous]
        [HttpGet]
        public async Task<FileContentResult> ExportExcel()
        {
            var fileInfo = await _service.ExportExcelAsync();

            return File(fileInfo.Content, fileInfo.FileContentType, fileInfo.FileName);
        }

    }
}
