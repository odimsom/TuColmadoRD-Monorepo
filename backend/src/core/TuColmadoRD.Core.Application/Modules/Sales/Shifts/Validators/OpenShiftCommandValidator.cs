using FluentValidation;
using TuColmadoRD.Core.Application.Sales.Shifts.Commands;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Validators;

public sealed class OpenShiftCommandValidator : AbstractValidator<OpenShiftCommand>
{
    public OpenShiftCommandValidator()
    {
        RuleFor(x => x.OpeningCashAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CashierName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
