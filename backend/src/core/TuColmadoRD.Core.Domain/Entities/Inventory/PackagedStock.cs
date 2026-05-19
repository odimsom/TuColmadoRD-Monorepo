using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory;

/// <summary>
/// Simple unit counter for packaged presentations (bags, bottles, packets).
/// One record per presentation — quantity incremented on stock entry, decremented on sale.
/// </summary>
public class PackagedStock : ITenantEntity
{
    private PackagedStock()
    {
        TenantId = TenantIdentifier.Empty;
    }

    private PackagedStock(Guid tenantId, Guid presentationId)
    {
        Id             = Guid.NewGuid();
        TenantId       = TenantIdentifier.Validate(tenantId).Result;
        PresentationId = presentationId;
        Quantity       = 0;
        LastUpdatedAt  = DateTime.UtcNow;
    }

    public Guid Id              { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public Guid PresentationId  { get; private set; }
    public int Quantity         { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    public static OperationResult<PackagedStock, DomainError> Create(Guid tenantId, Guid presentationId)
    {
        if (tenantId == Guid.Empty)
            return OperationResult<PackagedStock, DomainError>.Bad(DomainError.Validation("packaged_stock.tenant_required"));

        if (presentationId == Guid.Empty)
            return OperationResult<PackagedStock, DomainError>.Bad(DomainError.Validation("packaged_stock.presentation_required"));

        return OperationResult<PackagedStock, DomainError>.Good(new PackagedStock(tenantId, presentationId));
    }

    public OperationResult<Unit, DomainError> Add(int units)
    {
        if (units <= 0)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("packaged_stock.units_must_be_positive"));

        Quantity     += units;
        LastUpdatedAt = DateTime.UtcNow;
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public OperationResult<Unit, DomainError> Subtract(int units)
    {
        if (units <= 0)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("packaged_stock.units_must_be_positive"));

        if (units > Quantity)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("packaged_stock.insufficient_quantity"));

        Quantity     -= units;
        LastUpdatedAt = DateTime.UtcNow;
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }
}
