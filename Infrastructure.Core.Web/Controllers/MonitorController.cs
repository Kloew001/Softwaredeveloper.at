using Infrastructure.Core.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Monitor;

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
    public string Version() => _monitoreService.GetApplicationVersion();

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
    public Task<DBConnectionInfo> DBConnectionInfo() => _monitoreService.DBConnectionInfo(); [AllowAnonymous]

    [HttpGet("detail")]
    public IActionResult Detail([FromServices] IApplicationSettings applicationSettings)
    {
        if (applicationSettings.FeatureToggles.MonitorDetails != true)
            return Forbid();

        var request = HttpContext.Request;
        var headers = HttpContext.Request.Headers
                .ToDictionary(h => h.Key, h => h.Value.ToString());

        return Ok(new
        {
            Ip = HttpContext.ResolveIpOrAnon(),
            BucketIp = HttpContext.ResolveBucketIp(),
            AccountId = HttpContext.ResolveAccountId(),
            ApplicationName = _monitoreService.GetApplicationName(),

            Server = new
            {
                Environment = _monitoreService.GetEnvironmentName(),
                AppVersion = _monitoreService.GetApplicationVersion()
            },

            Request = new
            {
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                Scheme = request.Scheme,
                Protocol = request.Protocol,
                Host = request.Host.ToString(),
                Headers = headers
            },

            Auth = new
            {
                IsAuthenticated = HttpContext.User?.Identity?.IsAuthenticated ?? false,
                UserName = HttpContext.User?.Identity?.Name,
                Claims = HttpContext.User?.Claims?.Select(c => new { c.Type, c.Value })
            },

            TimestampUtc = DateTime.UtcNow,
            Timestamp = DateTime.Now,
        });
    }
}
