using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

public sealed record FundTransactionPagedDto(
    Guid Id,
    Guid FundId,
    int Type,
    string TypeName,
    decimal Amount,
    int? Category,
    string? CategoryName,
    string Description,
    string? JustificationNote,
    Guid? ReferenceId,
    decimal BalanceAfter,
    DateTime OccurredAt);

public sealed record FundTransactionsPagedResult(
    IReadOnlyList<FundTransactionPagedDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record GetFundTransactionsQuery(Guid FundId, int Page = 1, int PageSize = 20)
    : IRequest<OperationResult<FundTransactionsPagedResult, DomainError>>;
