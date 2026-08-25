namespace Pharmacy.Application.DTOs;

public sealed record LoginRequest(string Username, string Password);

public sealed record UserDto(string Username);

public sealed record LoginResponse(string Username, string CsrfToken);

public sealed record CsrfResponse(string Token);
