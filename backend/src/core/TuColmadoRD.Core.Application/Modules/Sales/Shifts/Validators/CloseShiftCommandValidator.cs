using FluentValidation;
using TuColmadoRD.Core.Application.Sales.Shifts.Commands;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Validators;

public sealed class CloseShiftCommandValidator : AbstractValidator<CloseShiftCommand>
{
    public CloseShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.ActualCashAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null);
    }
}
