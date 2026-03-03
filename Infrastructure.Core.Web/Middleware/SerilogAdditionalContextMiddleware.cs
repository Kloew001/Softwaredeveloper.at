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

public class SerilogAdditionalContextMiddleware(
    RequestDelegate next,
    ICorrelationIdAccessor correlationIdAccessor)
{
    public async Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty(Core.Utility.SerilogUtility.Area, Utility.SerilogUtility.Area_Web))
        using (LogContext.PushProperty("CorrelationId", correlationIdAccessor.GetCorrelationId()))
        using (LogContext.PushProperty("ClientIP", context.ResolveIp()))
        {
            await next(context);
        }
    }
}

public class SerilogAccountContextMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("AccountId", context.ResolveAccountIdOrAnon()))
        {
            await next(context);
        }
    }
}