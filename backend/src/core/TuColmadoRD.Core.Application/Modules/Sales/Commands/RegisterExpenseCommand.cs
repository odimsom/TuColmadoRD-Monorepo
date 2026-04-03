using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Commands;

public sealed record RegisterExpenseCommand(
    decimal Amount,
    string Category,
    string Description
) : IRequest<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>;
