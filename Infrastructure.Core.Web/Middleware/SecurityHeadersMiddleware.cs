using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Infrastructure.Core.Web.Middleware;

public static class SecurityHeadersBuilderExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}

public interface ISecurityHeadersService
{
    void HandleHeaders(HttpContext context);
}

public class SecurityHeadersService : ISecurityHeadersService
{
    public void HandleHeaders(HttpContext context)
    {
        context.Response.Headers.ContentSecurityPolicy = new StringValues(
            "default-src 'self' blob:;" +
              "object-src 'none';" +
              "style-src 'self' 'unsafe-inline';" +
              "script-src 'self' 'unsafe-inline';" +
              "font-src 'self' https://fonts.gstatic.com https://cdn.materialdesignicons.com;");

        context.Response.Headers.XContentTypeOptions = new StringValues("nosniff");
        context.Response.Headers.XFrameOptions = new StringValues("SAMEORIGIN");
        context.Response.Headers.Append("Referrer-Policy", new StringValues("strict-origin-when-cross-origin"));
        context.Response.Headers.Append("Cache-Control", new StringValues("no-store"));
        context.Response.Headers.Append("Pragma", new StringValues("no-cache"));

        if (context.Response.Headers.ContainsKey(HeaderNames.StrictTransportSecurity))
            context.Response.Headers.StrictTransportSecurity = new StringValues("max-age=31536000; includeSubDomains; preload");

        if (context.Response.Headers.ContainsKey(HeaderNames.Server))
            context.Response.Headers.Remove(HeaderNames.Server);

        if (context.Response.Headers.ContainsKey(HeaderNames.XPoweredBy))
            context.Response.Headers.Remove(HeaderNames.XPoweredBy);
    }
}

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISecurityHeadersService _securityHeadersService;

    public SecurityHeadersMiddleware(RequestDelegate next, ISecurityHeadersService securityHeadersService)
    {
        _next = next;
        _securityHeadersService = securityHeadersService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting((c) =>
        {
            _securityHeadersService.HandleHeaders((HttpContext)c);

            return Task.CompletedTask;
        }, state: context);

        await _next(context);

        //_securityHeadersService.HandleHeaders(context);
    }
}
