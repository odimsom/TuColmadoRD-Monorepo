using MediatR;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Inventory.Commands;

public record CreateMonetaryFundCommand(
    string Name,
    decimal InitialDeposit
) : IRequest<OperationResult<Guid, DomainError>>, ICommandMarker;

public record RecordFundDepositCommand(
    Guid FundId,
    decimal Amount,
    string Description
) : IRequest<OperationResult<Guid, DomainError>>, ICommandMarker;

public record RecordFundExpenseCommand(
    Guid FundId,
    decimal Amount,
    int Category,
    string Description,
    string? JustificationNote,
    Guid? ReferenceId
) : IRequest<OperationResult<Guid, DomainError>>, ICommandMarker;
