using FluentValidation;
using TuColmadoRD.Core.Application.Inventory.Commands;

namespace TuColmadoRD.Core.Application.Inventory.Validators;

/// <summary>
/// Validator for create product command.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SalePrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ItbisRate)
            .InclusiveBetween(0, 1);

        RuleFor(x => x.UnitType)
            .InclusiveBetween(1, 4);
    }
}
