using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using SoftwaredeveloperDotAt.Infrastructure.Core;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

using System.Globalization;

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


    public static class UseCurrentCultureBuilderExtensions
    {
        public static IApplicationBuilder UseCurrentCulture(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CurrentCultureMiddleware>();
        }
    }

    public class CurrentCultureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _memoryCache;

        public CurrentCultureMiddleware(RequestDelegate next,
            IMemoryCache memoryCache)
        {
            _next = next;
            _memoryCache = memoryCache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var currentLanguageService = context.RequestServices.GetService<ICurrentLanguageService>();

            var currentCultureId = currentLanguageService.CurrentCultureId;

            var cultureName =
                _memoryCache.GetOrCreate(nameof(CurrentCultureMiddleware) + "_" + currentCultureId, (entry) =>
            {
                var cultureName = context.RequestServices.GetService<IDbContext>()
                    .Set<MultilingualCulture>()
                    .Where(_ => _.Id == currentCultureId)
                    .Select(_ => _.Name)
                    .SingleOrDefault();

                return cultureName;
            });

            var culture = new CultureInfo(cultureName);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            await _next(context);
        }
    }
}
