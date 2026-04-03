using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Application.Inventory.Queries;

namespace TuColmadoRD.Core.Application.Inventory.Abstractions;

/// <summary>
/// Read-only projection repository for inventory products.
/// </summary>
public interface IProductReadRepository
{
    /// <summary>
    /// Retrieves a single product projection by id.
    /// </summary>
    Task<ProductDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct);

    /// <summary>
    /// Retrieves paged product projections.
    /// </summary>
    Task<PagedResult<ProductDto>> GetPagedAsync(GetProductsPagedQuery query, Guid tenantId, CancellationToken ct);
}
