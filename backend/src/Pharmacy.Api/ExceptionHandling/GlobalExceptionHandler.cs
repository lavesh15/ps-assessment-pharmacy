using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.Exceptions;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IHostEnvironment environment, ILogger<GlobalExceptionHandler> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = Map(exception);

        if (status >= 500)
        {
            _logger.LogError(exception, "Unhandled exception for {CorrelationId}", httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(exception, "Request failed with {StatusCode} for {CorrelationId}", status, httpContext.TraceIdentifier);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status >= 500 && !_environment.IsDevelopment()
                ? "An unexpected error occurred."
                : detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        if (exception is RequestValidationException validation)
        {
            problem.Extensions["errors"] = validation.Errors;
        }

        if (exception is DomainException domain)
        {
            problem.Extensions["errorCode"] = domain.ErrorCode;
        }

        if (status >= 500 && _environment.IsDevelopment())
        {
            problem.Extensions["exception"] = exception.GetType().Name;
            problem.Extensions["stackTrace"] = exception.ToString();
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int Status, string Title, string Detail) Map(Exception exception) => exception switch
    {
        RequestValidationException ex => (StatusCodes.Status400BadRequest, "Validation failed", ex.Message),
        IdempotencyException ex => (StatusCodes.Status400BadRequest, "Idempotency error", ex.Message),
        InsufficientStockException ex => (StatusCodes.Status400BadRequest, "Insufficient stock", ex.Message),
        InvalidCredentialsException ex => (StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message),
        NotFoundException ex => (StatusCodes.Status404NotFound, "Not found", ex.Message),
        ConcurrencyException ex => (StatusCodes.Status409Conflict, "Concurrency conflict", ex.Message),
        _ => (StatusCodes.Status500InternalServerError, "Server error", exception.Message)
    };
}
