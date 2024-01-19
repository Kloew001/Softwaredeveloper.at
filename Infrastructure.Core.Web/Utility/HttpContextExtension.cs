using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Infrastructure.Core.Web.Utility
{
    public static class HttpContextExtension
    {
        public static string ResolveIpAddress(this HttpContext httpContext)
        {
            httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor);

            var ipAddress = forwardedFor.FirstOrDefault() ?? 
                httpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString();

            return ipAddress ?? null;
        }
    }
}
