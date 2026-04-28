using FluentValidation;
using TuColmadoRD.Core.Application.Sales.Commands;

namespace TuColmadoRD.Core.Application.Sales.Validators;

public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithErrorCode("sale.items_required");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEqual(Guid.Empty);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
            });

        RuleFor(x => x.Payments)
            .NotEmpty()
            .WithErrorCode("sale.payments_required");

        RuleForEach(x => x.Payments)
            .ChildRules(payment =>
            {
                payment.RuleFor(p => p.PaymentMethodId).InclusiveBetween(1, 5);
                payment.RuleFor(p => p.Amount).GreaterThan(0);
                payment.RuleFor(p => p.Reference).MaximumLength(50).When(p => !string.IsNullOrEmpty(p.Reference));
                payment.RuleFor(p => p.CustomerId)
                    .NotEmpty()
                    .When(p => p.PaymentMethodId == 4 || p.PaymentMethodId == 5)
                    .WithErrorCode("sale.credit_payment_customer_required");
            });

        RuleFor(x => x.Notes)
            .MaximumLength(300)
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
