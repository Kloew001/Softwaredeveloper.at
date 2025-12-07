using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwaredeveloperDotAt.Infrastructure.Core.Validation;
using System.Diagnostics;
using SoftwareDeveloperDotATValidation = SoftwaredeveloperDotAt.Infrastructure.Core.Validation;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Middleware;

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IWebHostEnvironment _env;

    public ValidationExceptionHandler(
        ILogger<ValidationExceptionHandler> logger,
        IProblemDetailsService problemDetailsService,
        IWebHostEnvironment env)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return true;
        }

        if (exception is SoftwareDeveloperDotATValidation.ValidationException validationException)
        {
            var problemDetails = new ValidationProblemDetails(validationException);

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            return await HandleProblemDetails(httpContext, exception, problemDetails, cancellationToken);
        }
        if (exception is FluentValidation.ValidationException fluentValidationException)
        {
            var ex = new ValidationException(fluentValidationException.Message,
            fluentValidationException.Errors.Select(e => new ValidationError
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage
            }));

            var problemDetails = new ValidationProblemDetails(ex);

            return await HandleProblemDetails(httpContext, exception, problemDetails, cancellationToken);
        }

        return false;
    }

    private async ValueTask<bool> HandleProblemDetails(HttpContext httpContext, Exception exception, ValidationProblemDetails problemDetails, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Validation failed. correlationId={correlationId}", httpContext.ResolveCorrelationId());

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        var written = await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        if (!written)
        {
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }

    public class ValidationProblemDetails : ProblemDetails
    {
        public IEnumerable<ValidationError> ValidationErrors { get; set; }

        public ValidationProblemDetails(ValidationException validationException)
        {
            Status = StatusCodes.Status412PreconditionFailed;
            Title = validationException.Message;
            Detail = validationException.Message;
            ValidationErrors = validationException.ValidationErrors;
        }
    }
}

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception. correlationId={correlationId}", httpContext.ResolveCorrelationId());

#if DEBUG
        Debugger.Break();
#endif

        var (status, clientTitle, clientDetail, typeUri) = MapException(exception);

        if (httpContext.Response.HasStarted)
        {
            return true;
        }

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = exception.Message,
            Detail = exception.Message,
            Type = typeUri,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        var written = await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        if (!written)
        {
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }
        return true;
    }

    private static (int status, string title, string detail, string type) MapException(Exception ex)
    {
        return ex switch
        {
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound,
                 "Ressource nicht gefunden.",
                 "Die angeforderte Ressource existiert nicht.",
                 "https://example.com/errors/not-found"),

            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden,
                 "Zugriff verweigert.",
                 "Sie besitzen keine Berechtigung für diese Aktion.",
                 "https://example.com/errors/forbidden"),

            InvalidOperationException =>
                (StatusCodes.Status409Conflict,
                 "Konflikt.",
                 "Die Anfrage konnte aufgrund eines Zustandskonflikts nicht verarbeitet werden.",
                 "https://example.com/errors/conflict"),

            _ =>
                (StatusCodes.Status500InternalServerError,
                 "Unerwarteter Fehler.",
                 "Es ist ein unerwarteter Fehler aufgetreten. Versuchen Sie es später erneut.",
                 "https://example.com/errors/internal-server-error")
        };
    }
}
