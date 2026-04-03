using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

public sealed record CatalogItemDto(Guid ProductId, string Name, decimal Price, decimal Stock, int UnitTypeId);

public sealed record GetCatalogQuery() : IRequest<OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>>;
