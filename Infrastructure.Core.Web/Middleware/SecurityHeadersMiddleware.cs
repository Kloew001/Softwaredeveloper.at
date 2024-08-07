﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using SoftwaredeveloperDotAt.Infrastructure.Core;

namespace Infrastructure.Core.Web.Middleware
{
    public static class SecurityHeadersBuilderExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }

    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.ContentSecurityPolicy = new StringValues(
                "default-src 'self';" +
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

            await _next(context);
        }
    }
}
