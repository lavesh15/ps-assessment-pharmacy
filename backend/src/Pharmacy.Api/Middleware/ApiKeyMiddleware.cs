using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Pharmacy.Application.Options;

namespace Pharmacy.Api.Middleware;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiKeyOptions _options;

    public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresApiKey(context.Request.Path) || HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var provided = context.Request.Cookies[_options.CookieName];
        if (string.IsNullOrEmpty(provided) || !ApiKeyMatches(provided, _options.Value))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "API key required",
                status = 401,
                detail = "A valid API key cookie is required.",
                correlationId = context.TraceIdentifier
            });
            return;
        }

        await _next(context);
    }

    private static bool RequiresApiKey(PathString path)
    {
        return path.StartsWithSegments("/api")
               && !path.StartsWithSegments("/api/v1/auth")
               && !path.StartsWithSegments("/api/auth");
    }

    private static bool ApiKeyMatches(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return providedBytes.Length == expectedBytes.Length
               && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}
