using FluentValidation;
using TuColmadoRD.Core.Application.Sales.Commands;

namespace TuColmadoRD.Core.Application.Sales.Validators;

public sealed class RegisterExpenseCommandValidator : AbstractValidator<RegisterExpenseCommand>
{
    public RegisterExpenseCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto del gasto debe ser mayor a cero.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("La categoria del gasto es requerida.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("La descripcion es requerida y debe tener menos de 500 caracteres.");
    }
}
