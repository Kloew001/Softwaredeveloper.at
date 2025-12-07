using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Middleware;

public static class CorrelationIdBuilderExtensions
{
    public static IApplicationBuilder UseSerilogCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("Area", "Web"))
        using (LogContext.PushProperty("CorrelationId", context.ResolveCorrelationId()))
        using (LogContext.PushProperty("AccountId", context.ResolveAccountIdOrAnon()))
        using (LogContext.PushProperty("ClientIP", context.ResolveIp()))
        {
            await _next(context);
        }
    }
}
