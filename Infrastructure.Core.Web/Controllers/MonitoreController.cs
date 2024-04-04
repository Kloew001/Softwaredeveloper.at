using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Monitore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers
{
    public class MonitoreController : BaseApiController
    {
        private readonly IMonitoreService _monitoreService;

        public MonitoreController(IMonitoreService monitoreService)
        {
            _monitoreService = monitoreService;
        }

        [HttpGet]
        [AllowAnonymous]
        public Task<bool> IsAlive() => _monitoreService.IsAlive();

        [HttpGet]
        [AllowAnonymous]
        public Task<DBConnectionInfo> DBConnectionInfo() => _monitoreService.DBConnectionInfo();
    }
}
