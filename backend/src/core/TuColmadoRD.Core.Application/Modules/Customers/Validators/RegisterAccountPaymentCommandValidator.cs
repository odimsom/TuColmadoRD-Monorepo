using FluentValidation;
using TuColmadoRD.Core.Application.Customers.Commands;

namespace TuColmadoRD.Core.Application.Customers.Validators;

public sealed class RegisterAccountPaymentCommandValidator : AbstractValidator<RegisterAccountPaymentCommand>
{
    public RegisterAccountPaymentCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("El Id del cliente es requerido.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto del abono debe ser mayor a cero.");

        RuleFor(x => x.PaymentMethodId)
            .Must(id => id == 1 || id == 2 || id == 3)
            .WithMessage("Metodo de pago invalido. Un abono no puede ser realizado a credito.");

        RuleFor(x => x.Concept)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("El concepto es requerido y debe tener menos de 500 caracteres.");
    }
}
