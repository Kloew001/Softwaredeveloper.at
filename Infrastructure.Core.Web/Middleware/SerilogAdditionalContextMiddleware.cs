using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using Serilog.Context;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Middleware;

public static class SerilogAdditionalContextBuilderExtensions
{
    public static IApplicationBuilder UseSerilogAdditionalContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SerilogAdditionalContextMiddleware>();
    }

    public static IApplicationBuilder UseSerilogAccountContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SerilogAccountContextMiddleware>();
    }
}

public class SerilogAdditionalContextMiddleware
{
    private readonly RequestDelegate _next;

    public SerilogAdditionalContextMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty(Core.Utility.SerilogUtility.Area, Utility.SerilogUtility.Area_Web))
        using (LogContext.PushProperty("CorrelationId", context.ResolveCorrelationId()))
        using (LogContext.PushProperty("ClientIP", context.ResolveIp()))
        {
            await _next(context);
        }
    }
}

public class SerilogAccountContextMiddleware
{
    private readonly RequestDelegate _next;

    public SerilogAccountContextMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("AccountId", context.ResolveAccountIdOrAnon()))
        {
            await _next(context);
        }
    }
}