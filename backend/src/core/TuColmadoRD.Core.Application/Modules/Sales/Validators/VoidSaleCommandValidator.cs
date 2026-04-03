using FluentValidation;
using TuColmadoRD.Core.Application.Sales.Commands;

namespace TuColmadoRD.Core.Application.Sales.Validators;

public sealed class VoidSaleCommandValidator : AbstractValidator<VoidSaleCommand>
{
    public VoidSaleCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEqual(Guid.Empty);
        RuleFor(x => x.VoidReason).NotEmpty().MaximumLength(200);
    }
}
