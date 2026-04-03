using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Customers.Commands;

public sealed record CreateCustomerAddressRequest(
    string Province,
    string Sector,
    string Street,
    string Reference,
    string? HouseNumber);

public sealed record CreateCustomerCommand(
    string FullName,
    string DocumentId,
    string? Phone,
    CreateCustomerAddressRequest? Address,
    decimal? CreditLimit
) : IRequest<OperationResult<CreateCustomerResult, DomainError>>;

public sealed record CreateCustomerResult(
    Guid CustomerId,
    Guid AccountId,
    decimal Balance,
    decimal CreditLimit,
    bool IsActive,
    DateTime CreatedAt);
