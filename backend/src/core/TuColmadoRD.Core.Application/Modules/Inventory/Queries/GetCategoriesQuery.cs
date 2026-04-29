using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

public sealed record CategoryDto(Guid Id, string Name);

public sealed record GetCategoriesQuery
    : IRequest<OperationResult<IReadOnlyList<CategoryDto>, DomainError>>;
