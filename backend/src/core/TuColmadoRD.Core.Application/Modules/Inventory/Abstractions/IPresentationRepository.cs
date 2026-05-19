using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Abstractions;

public interface IPresentationRepository
{
    Task<OperationResult<ProductPresentation, DomainError>> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductPresentation>> GetByProductIdAsync(Guid productId, Guid tenantId, CancellationToken ct = default);
    Task AddAsync(ProductPresentation presentation, CancellationToken ct = default);
    Task<bool> ProductExistsAsync(Guid productId, Guid tenantId, CancellationToken ct = default);
}
