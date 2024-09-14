using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SoftwaredeveloperDotAt.Infrastructure.Core.Validation;

using System.Diagnostics;

namespace Infrastructure.Core.Web.Middleware
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ValidationExceptionHandler> _logger;
        public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ValidationException validationException)
            {
                var problemDetails = new ValidationProblemDetails(validationException);

                httpContext.Response.StatusCode = problemDetails.Status.Value;

                await httpContext.Response
                    .WriteAsJsonAsync(problemDetails, cancellationToken);


                return true;
            }
            return false;
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
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception, "Error: {Message}", exception.Message);

#if DEBUG
            Debugger.Break();
#endif

            var statusCode = httpContext.Response?.StatusCode == null
                ? StatusCodes.Status500InternalServerError
                : httpContext.Response.StatusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = "An unexpected error occurred",
                Detail = exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
