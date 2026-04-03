using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Customers.Queries;

public sealed record GetCustomerByIdQuery(Guid CustomerId)
    : IRequest<OperationResult<CustomerDetailResult, DomainError>>;

public sealed record CustomerDetailResult(
    Guid CustomerId,
    string FullName,
    string DocumentId,
    string? Phone,
    bool IsActive,
    DateTime CreatedAt,
    Guid AccountId,
    decimal Balance,
    decimal CreditLimit,
    DateTime LastActivity,
    string CreditStatus);
