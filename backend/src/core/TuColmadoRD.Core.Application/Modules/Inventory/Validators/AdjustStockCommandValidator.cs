using FluentValidation;
using TuColmadoRD.Core.Application.Inventory.Commands;

namespace TuColmadoRD.Core.Application.Inventory.Validators;

/// <summary>
/// Validator for adjust stock command.
/// </summary>
public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Delta)
            .NotEqual(0);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(200);
    }
}
