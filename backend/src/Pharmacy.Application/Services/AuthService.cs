using FluentValidation;
using Microsoft.Extensions.Options;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Exceptions;
using Pharmacy.Application.Options;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Services;

public interface IAuthService
{
    UserDto Authenticate(LoginRequest request);
}

public sealed class AuthService : IAuthService
{
    private readonly DemoAuthOptions _options;
    private readonly IValidator<LoginRequest> _validator;

    public AuthService(IOptions<DemoAuthOptions> options, IValidator<LoginRequest> validator)
    {
        _options = options.Value;
        _validator = validator;
    }

    public UserDto Authenticate(LoginRequest request)
    {
        var result = _validator.Validate(request);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new RequestValidationException(errors);
        }

        if (!string.Equals(request.Username, _options.Username, StringComparison.Ordinal)
            || !string.Equals(request.Password, _options.Password, StringComparison.Ordinal))
        {
            throw new InvalidCredentialsException();
        }

        return new UserDto(request.Username);
    }
}
