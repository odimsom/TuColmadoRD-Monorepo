using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Inventory.Commands;

public sealed record CreateCategoryCommand(string Name)
    : IRequest<OperationResult<Guid, DomainError>>;

public sealed record SeedDefaultCategoriesCommand
    : IRequest<OperationResult<int, DomainError>>;

public sealed record DeactivateCategoryCommand(Guid CategoryId)
    : IRequest<OperationResult<ResultUnit, DomainError>>;
