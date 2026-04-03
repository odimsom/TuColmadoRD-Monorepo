using MediatR;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

/// <summary>
/// Query to retrieve paged product projections.
/// </summary>
public record GetProductsPagedQuery(
    int Page,
    int PageSize,
    string? NameFilter,
    Guid? CategoryId,
    bool IncludeInactive
) : IRequest<OperationResult<PagedResult<ProductDto>, DomainError>>;
