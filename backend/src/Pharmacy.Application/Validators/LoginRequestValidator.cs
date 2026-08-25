using FluentValidation;
using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}
