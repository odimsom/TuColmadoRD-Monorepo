using FluentValidation;
using TuColmadoRD.Core.Application.Inventory.Commands;

namespace TuColmadoRD.Core.Application.Inventory.Validators;

/// <summary>
/// Validator for update product price command.
/// </summary>
public sealed class UpdateProductPriceCommandValidator : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.NewCostPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.NewSalePrice)
            .GreaterThanOrEqualTo(0);
    }
}
