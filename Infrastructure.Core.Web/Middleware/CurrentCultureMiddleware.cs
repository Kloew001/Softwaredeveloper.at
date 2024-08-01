using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

using System.Globalization;

namespace Infrastructure.Core.Web.Middleware
{
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
