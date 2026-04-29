using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Customers.Queries;

public sealed record CustomerSummaryDto(
    Guid CustomerId, 
    string FullName, 
    string Phone, 
    decimal Balance, 
    decimal CreditLimit, 
    bool IsActive,
    string? Province = null,
    string? Sector = null,
    string? Street = null,
    string? HouseNumber = null,
    string? Reference = null,
    double? Latitude = null,
    double? Longitude = null);

public sealed record GetCustomersWithBalanceQuery() : IRequest<OperationResult<IReadOnlyList<CustomerSummaryDto>, DomainError>>;

public sealed record CustomerStatementDto(Guid TransactionId, DateTime Date, string Type, decimal Amount, string Concept);

public sealed record GetCustomerStatementQuery(Guid CustomerId) : IRequest<OperationResult<IReadOnlyList<CustomerStatementDto>, DomainError>>;
