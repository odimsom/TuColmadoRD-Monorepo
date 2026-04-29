using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Queries;

public sealed record ExpenseSummaryDto(
    Guid Id,
    decimal Amount,
    string Category,
    string Description,
    DateTime Date);

public sealed record GetExpensesQuery(int Page = 1, int PageSize = 50)
    : IRequest<OperationResult<IReadOnlyList<ExpenseSummaryDto>, DomainError>>;
