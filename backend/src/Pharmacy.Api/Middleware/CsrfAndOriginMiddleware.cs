using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Pharmacy.Application.Options;

namespace Pharmacy.Api.Middleware;

public sealed class CsrfAndOriginMiddleware
{
    private static readonly string[] MutatingMethods = ["POST", "PUT", "PATCH", "DELETE"];

    private readonly RequestDelegate _next;
    private readonly FrontendOptions _frontend;
    private readonly IHostEnvironment _environment;

    public CsrfAndOriginMiddleware(
        RequestDelegate next,
        IOptions<FrontendOptions> frontend,
        IHostEnvironment environment)
    {
        _next = next;
        _frontend = frontend.Value;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        EnsureCsrfCookie(context);

        if (MutatingMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase)
            && !IsExempt(context.Request.Path))
        {
            if (!IsAllowedOrigin(context))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Invalid origin",
                    status = 403,
                    detail = "The request did not come from an allowed frontend origin.",
                    correlationId = context.TraceIdentifier
                });
                return;
            }

            if (!IsValidCsrf(context))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "CSRF validation failed",
                    status = 403,
                    detail = "Missing or invalid CSRF token.",
                    correlationId = context.TraceIdentifier
                });
                return;
            }
        }

        await _next(context);
    }

    private static bool IsExempt(PathString path) =>
        path.StartsWithSegments("/openapi") || path.StartsWithSegments("/health");

    private void EnsureCsrfCookie(HttpContext context)
    {
        if (context.Request.Cookies.ContainsKey(_frontend.CsrfCookieName))
        {
            return;
        }

        var token = TokenFactory.Create();
        context.Response.Cookies.Append(_frontend.CsrfCookieName, token, new CookieOptions
        {
            HttpOnly = false,
            Secure = _environment.IsProduction(),
            SameSite = _environment.IsProduction() ? SameSiteMode.Strict : SameSiteMode.Lax,
            Path = "/",
            IsEssential = true
        });
    }

    private bool IsAllowedOrigin(HttpContext context)
    {
        var origin = context.Request.Headers.Origin.FirstOrDefault()
                     ?? context.Request.Headers.Referer.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(origin))
        {
            return false;
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var originValue = $"{uri.Scheme}://{uri.Authority}";
        return _frontend.AllowedOrigins.Contains(originValue, StringComparer.OrdinalIgnoreCase);
    }

    private bool IsValidCsrf(HttpContext context)
    {
        var cookie = context.Request.Cookies[_frontend.CsrfCookieName];
        var header = context.Request.Headers[_frontend.CsrfHeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(cookie) || string.IsNullOrEmpty(header))
        {
            return false;
        }

        var cookieBytes = Encoding.UTF8.GetBytes(cookie);
        var headerBytes = Encoding.UTF8.GetBytes(header);
        return cookieBytes.Length == headerBytes.Length
               && CryptographicOperations.FixedTimeEquals(cookieBytes, headerBytes);
    }
}
