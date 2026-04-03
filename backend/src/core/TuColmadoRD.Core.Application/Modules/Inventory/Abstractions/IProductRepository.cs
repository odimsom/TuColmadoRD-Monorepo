using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Application.Inventory.Abstractions;

/// <summary>
/// Inventory product write repository abstraction.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves a product by id and tenant.
    /// </summary>
    Task<OperationResult<Product, DomainError>> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct);

    /// <summary>
    /// Adds a new product.
    /// </summary>
    Task AddAsync(Product product, CancellationToken ct);

    /// <summary>
    /// Retrieves products by ids filtered by tenant.
    /// </summary>
    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyList<Guid> ids, Guid tenantId, CancellationToken ct);

    /// <summary>
    /// Updates a batch of products.
    /// </summary>
    Task UpdateRangeAsync(IReadOnlyList<Product> products, CancellationToken ct);

    /// <summary>
    /// Checks whether the category exists for a tenant.
    /// </summary>
    Task<bool> CategoryExistsAsync(Guid categoryId, Guid tenantId, CancellationToken ct);
}
