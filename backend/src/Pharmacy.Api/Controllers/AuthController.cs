using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Options;
using Pharmacy.Application.Services;

namespace Pharmacy.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ApiKeyOptions _apiKey;
    private readonly FrontendOptions _frontend;
    private readonly IHostEnvironment _environment;

    public AuthController(
        IAuthService authService,
        IOptions<ApiKeyOptions> apiKey,
        IOptions<FrontendOptions> frontend,
        IHostEnvironment environment)
    {
        _authService = authService;
        _apiKey = apiKey.Value;
        _frontend = frontend.Value;
        _environment = environment;
    }

    [HttpGet("csrf")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    public ActionResult<CsrfResponse> Csrf()
    {
        var token = Request.Cookies[_frontend.CsrfCookieName];
        if (string.IsNullOrEmpty(token))
        {
            token = CreateCsrfToken();
            Response.Cookies.Append(_frontend.CsrfCookieName, token, CreateCookieOptions(httpOnly: false));
        }

        return Ok(new CsrfResponse(token));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = _authService.Authenticate(request);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Username)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        Response.Cookies.Append(_apiKey.CookieName, _apiKey.Value, CreateCookieOptions(httpOnly: true));

        var csrf = CreateCsrfToken();
        Response.Cookies.Append(_frontend.CsrfCookieName, csrf, CreateCookieOptions(httpOnly: false));

        return Ok(new LoginResponse(user.Username, csrf));
    }

    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete(_apiKey.CookieName, new CookieOptions { Path = "/" });
        Response.Cookies.Delete(_frontend.CsrfCookieName, new CookieOptions { Path = "/" });
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public ActionResult<UserDto> Me()
    {
        var username = User.Identity?.Name ?? string.Empty;
        return Ok(new UserDto(username));
    }

    private static string CreateCsrfToken() => TokenFactory.Create();

    private CookieOptions CreateCookieOptions(bool httpOnly) => new()
    {
        HttpOnly = httpOnly,
        Secure = _environment.IsProduction(),
        SameSite = _environment.IsProduction() ? SameSiteMode.Strict : SameSiteMode.Lax,
        Path = "/",
        IsEssential = true
    };
}
