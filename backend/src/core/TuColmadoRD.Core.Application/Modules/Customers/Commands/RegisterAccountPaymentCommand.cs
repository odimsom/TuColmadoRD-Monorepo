using System;
using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Customers.Commands;

public sealed record RegisterAccountPaymentCommand(
    Guid CustomerId,
    decimal Amount,
    int PaymentMethodId,
    string Concept
) : IRequest<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>;
