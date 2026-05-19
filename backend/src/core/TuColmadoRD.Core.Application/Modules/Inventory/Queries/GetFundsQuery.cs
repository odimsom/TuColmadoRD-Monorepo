using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

public sealed record FundSummaryDto(
    Guid Id,
    Guid TenantId,
    string Name,
    decimal CurrentBalance,
    DateTime CreatedAt);

public sealed record GetFundsQuery
    : IRequest<OperationResult<IReadOnlyList<FundSummaryDto>, DomainError>>;
