using FluentValidation;
using TuColmadoRD.Core.Application.Customers.Commands;

namespace TuColmadoRD.Core.Application.Customers.Validators;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DocumentId)
            .NotEmpty();

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.CreditLimit)
            .GreaterThan(0)
            .When(x => x.CreditLimit.HasValue);

        RuleFor(x => x.Address)
            .Must(address =>
                address is null ||
                (!string.IsNullOrWhiteSpace(address.Province)
                 && !string.IsNullOrWhiteSpace(address.Sector)
                 && !string.IsNullOrWhiteSpace(address.Street)
                 && !string.IsNullOrWhiteSpace(address.Reference)))
            .WithErrorCode("customer.address_incomplete")
            .WithMessage("La direccion debe incluir provincia, sector, calle y referencia.");
    }
}
