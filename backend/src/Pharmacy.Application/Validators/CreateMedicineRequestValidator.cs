using System.Text.RegularExpressions;
using FluentValidation;
using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Validators;

public sealed class CreateMedicineRequestValidator : AbstractValidator<CreateMedicineRequest>
{
    private static readonly Regex HtmlPattern = new("<[^>]*>", RegexOptions.Compiled);

    public CreateMedicineRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200)
            .Must(BePlainText).WithMessage("Full name must not contain HTML.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .Must(notes => notes is null || BePlainText(notes))
            .WithMessage("Notes must not contain HTML.");

        RuleFor(x => x.Brand)
            .NotEmpty()
            .MaximumLength(120)
            .Must(BePlainText).WithMessage("Brand must not contain HTML.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .Must(HaveTwoDecimalPlaces).WithMessage("Price must have at most 2 decimal places.");

        RuleFor(x => x.ExpiryDate)
            .NotEmpty();
    }

    private static bool BePlainText(string value) => !HtmlPattern.IsMatch(value);

    private static bool HaveTwoDecimalPlaces(decimal price) => decimal.Round(price, 2) == price;
}
