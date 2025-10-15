using Infrastructure.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Monitor;

using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

public class MonitorController : BaseApiController
{
    private readonly IMonitorService _monitoreService;

    public MonitorController(IMonitorService monitoreService)
    {
        _monitoreService = monitoreService;
    }

    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy.Fixed5per1Hour)]
    public string Version() => Assembly.GetEntryAssembly().GetName().Version.ToString();

    [HttpGet]
    [AllowAnonymous]
    public string Environment() => _monitoreService.GetEnvironmentName();

    [HttpGet]
    [AllowAnonymous]
    public string ApplicationName() => _monitoreService.GetApplicationName();

    [HttpGet]
    [AllowAnonymous]
    public string Now([FromServices] IDateTimeService dateTimeService) => dateTimeService.Now().ToString();

    [HttpGet]
    [AllowAnonymous]
    public Task<bool> IsAlive() => _monitoreService.IsAlive();

    [HttpGet]
    [AllowAnonymous]
    public Task<DBConnectionInfo> DBConnectionInfo() => _monitoreService.DBConnectionInfo();
}
