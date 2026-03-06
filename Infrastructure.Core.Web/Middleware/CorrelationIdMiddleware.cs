
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private const string HeaderName = "X-Correlation-ID";

    public async Task Invoke(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        string correlationId;

        if (context.Request.Headers.TryGetValue(HeaderName, out var value))
        {
            correlationId = value;
            context.TraceIdentifier = correlationId;
        }
        else
        {
            correlationId = context.TraceIdentifier ?? Guid.NewGuid().ToString();
        }

        context.Items[HeaderName] = correlationId;
        context.Response.Headers.Append(HeaderName, correlationId);

        await _next(context);
    }
}

public interface ICorrelationIdAccessor
{
    string GetCorrelationId();
}

[SingletonDependency<ICorrelationIdAccessor>]
public class CorrelationIdAccessor(IHttpContextAccessor accessor) : ICorrelationIdAccessor
{
    private const string HeaderName = "X-Correlation-ID";

    public string GetCorrelationId()
    {
        return accessor.HttpContext.Items[HeaderName].ToString();
    }
}