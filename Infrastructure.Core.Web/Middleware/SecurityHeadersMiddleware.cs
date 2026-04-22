using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Middleware;

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
    public virtual void HandleHeaders(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        SetContentSecurityPolicy(context);
        SetXContentTypeOptions(context);
        SetXFrameOptions(context);
        SetReferrerPolicy(context);
        SetCacheControl(context);
        SetPragma(context);
        SetStrictTransportSecurity(context);
        RemoveResponseHeaders(context);
    }

    protected virtual void SetContentSecurityPolicy(HttpContext context)
    {
        context.Response.Headers.ContentSecurityPolicy = new StringValues(string.Concat(GetContentSecurityPolicyDirectives(context)));
    }

    protected virtual IEnumerable<string> GetContentSecurityPolicyDirectives(HttpContext context)
    {
        yield return "default-src 'self';";
        yield return "object-src 'none';";
        yield return "base-uri 'self';";
        yield return "frame-ancestors 'self';";
        yield return "form-action 'self';";
        yield return "script-src 'self';";
        yield return "style-src 'self';";
        yield return "font-src 'self' https://fonts.gstatic.com https://cdn.materialdesignicons.com;";
        yield return "img-src 'self' blob: data:;";
    }

    protected virtual void SetXContentTypeOptions(HttpContext context)
    {
        context.Response.Headers.XContentTypeOptions = new StringValues("nosniff");
    }

    protected virtual void SetXFrameOptions(HttpContext context)
    {
        context.Response.Headers.XFrameOptions = new StringValues("SAMEORIGIN");
    }

    protected virtual void SetReferrerPolicy(HttpContext context)
    {
        context.Response.Headers.Append("Referrer-Policy", new StringValues("strict-origin-when-cross-origin"));
    }

    protected virtual void SetCacheControl(HttpContext context)
    {
        context.Response.Headers.Append("Cache-Control", new StringValues("no-store"));
    }

    protected virtual void SetPragma(HttpContext context)
    {
        context.Response.Headers.Append("Pragma", new StringValues("no-cache"));
    }

    protected virtual void SetStrictTransportSecurity(HttpContext context)
    {
        if (context.Response.Headers.ContainsKey(HeaderNames.StrictTransportSecurity))
            context.Response.Headers.StrictTransportSecurity = new StringValues("max-age=31536000; includeSubDomains; preload");
    }

    protected virtual void RemoveResponseHeaders(HttpContext context)
    {
        foreach (var header in GetResponseHeadersToRemove(context))
        {
            if (context.Response.Headers.ContainsKey(header))
                context.Response.Headers.Remove(header);
        }
    }

    protected virtual IEnumerable<string> GetResponseHeadersToRemove(HttpContext context)
    {
        yield return HeaderNames.Server;
        yield return HeaderNames.XPoweredBy;
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