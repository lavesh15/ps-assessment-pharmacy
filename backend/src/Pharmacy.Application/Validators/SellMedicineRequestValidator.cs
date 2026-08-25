using FluentValidation;
using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Validators;

public sealed class SellMedicineRequestValidator : AbstractValidator<SellMedicineRequest>
{
    public SellMedicineRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
