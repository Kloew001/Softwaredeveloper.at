using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Infrastructure.Core.Web.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; object-src 'none'; frame-ancestors 'self'; frame-action 'self'; ");

            context.Response.Headers.Append("X-Content-Type-Options", new StringValues("nosniff"));
            context.Response.Headers.Append("X-Frame-Options", new StringValues("SAMEORIGIN"));
            context.Response.Headers.Append("Referrer-Policy", new StringValues("strict-origin-when-cross-origin"));
            context.Response.Headers.Append("Cache-Control", new StringValues("no-store"));
            context.Response.Headers.Append("Pragma", new StringValues("no-cache"));

            context.Response.OnStarting(state =>
            {
                var ctx = (HttpContext)state;

                if (ctx.Response.Headers.ContainsKey("Server"))
                    ctx.Response.Headers.Remove("Server");

                if (ctx.Response.Headers.ContainsKey("x-powered-by") || ctx.Response.Headers.ContainsKey("X-Powered-By"))
                {
                    ctx.Response.Headers.Remove("x-powered-by");
                    ctx.Response.Headers.Remove("X-Powered-By");
                }

                return Task.FromResult(0);
            }, context);

            await _next(context);
        }
    }
}
