﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers
{
    public class MultilingualController : BaseApiController
    {
        private readonly MultilingualService _service;
        private readonly ExcelMultilingualService _excelMultilingualService;
        private readonly JsonMultilingualService _jsonMultilingualService;

        public MultilingualController(
            MultilingualService service, 
            ExcelMultilingualService excelMultilingualService,
            JsonMultilingualService jsonMultilingualService)
        {
            _service = service;
            _excelMultilingualService = excelMultilingualService;
            _jsonMultilingualService = jsonMultilingualService;
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
            var fileInfo = await _excelMultilingualService.ExportToFileAsync();

            return File(fileInfo.Content, fileInfo.FileContentType, fileInfo.FileName);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<FileContentResult> ExportJson()
        {
            var fileInfo = await _jsonMultilingualService.ExportToFileAsync();

            return File(fileInfo.Content, fileInfo.FileContentType, fileInfo.FileName);
        }

    }
}
