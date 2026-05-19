using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory.Events;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory;

public class ProductPresentation : ITenantEntity
{
    private readonly List<object> _domainEvents = [];

    private ProductPresentation()
    {
        TenantId        = TenantIdentifier.Empty;
        DisplayName     = string.Empty;
        Brand           = string.Empty;
        PresentationType = PresentationType.PackagedUnit;
        SellMode        = SellMode.ByUnit;
        MeasureUnit     = UnitOfMeasure.Unit;
        SalePrice       = Money.Zero;
        CostPrice       = Money.Zero;
    }

    private ProductPresentation(
        Guid tenantId,
        Guid productId,
        string displayName,
        PresentationType presentationType,
        SellMode sellMode,
        UnitOfMeasure measureUnit,
        Money salePrice,
        Money costPrice,
        string? brand,
        decimal? nominalCapacity)
    {
        Id               = Guid.NewGuid();
        TenantId         = TenantIdentifier.Validate(tenantId).Result;
        ProductId        = productId;
        DisplayName      = displayName.Trim();
        PresentationType = presentationType;
        SellMode         = sellMode;
        MeasureUnit      = measureUnit;
        SalePrice        = salePrice;
        CostPrice        = costPrice;
        Brand            = brand?.Trim() ?? string.Empty;
        NominalCapacity  = nominalCapacity;
        IsActive         = true;
        CreatedAt        = DateTime.UtcNow;
        UpdatedAt        = DateTime.UtcNow;

        AddDomainEvent(new PresentationCreatedDomainEvent(Id, productId, tenantId, DisplayName, DateTime.UtcNow));
    }

    public Guid Id              { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public Guid ProductId       { get; private set; }
    public string DisplayName   { get; private set; }
    public PresentationType PresentationType { get; private set; }
    public SellMode SellMode    { get; private set; }
    public UnitOfMeasure MeasureUnit { get; private set; }
    public Money SalePrice      { get; private set; }
    public Money CostPrice      { get; private set; }
    public string Brand         { get; private set; }

    /// <summary>Declared capacity per container (e.g. 50 for a 50-lb sack). Null for packaged units.</summary>
    public decimal? NominalCapacity { get; private set; }

    public bool IsActive        { get; private set; }
    public DateTime CreatedAt   { get; private set; }
    public DateTime UpdatedAt   { get; private set; }

    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public static OperationResult<ProductPresentation, DomainError> Create(
        Guid tenantId,
        Guid productId,
        string displayName,
        PresentationType presentationType,
        SellMode sellMode,
        UnitOfMeasure measureUnit,
        Money salePrice,
        Money costPrice,
        string? brand,
        decimal? nominalCapacity)
    {
        if (tenantId == Guid.Empty)
            return OperationResult<ProductPresentation, DomainError>.Bad(DomainError.Validation("presentation.tenant_required"));

        if (productId == Guid.Empty)
            return OperationResult<ProductPresentation, DomainError>.Bad(DomainError.Validation("presentation.product_required"));

        if (string.IsNullOrWhiteSpace(displayName))
            return OperationResult<ProductPresentation, DomainError>.Bad(DomainError.Validation("presentation.display_name_required"));

        if (displayName.Length > 100)
            return OperationResult<ProductPresentation, DomainError>.Bad(DomainError.Validation("presentation.display_name_too_long"));

        if (salePrice.Amount < costPrice.Amount)
            return OperationResult<ProductPresentation, DomainError>.Bad(DomainError.Validation("presentation.sale_price_below_cost"));

        if (presentationType == PresentationType.BulkContainer && (nominalCapacity is null || nominalCapacity <= 0))
            return OperationResult<ProductPresentation, DomainError>.Bad(DomainError.Validation("presentation.bulk_requires_nominal_capacity"));

        return OperationResult<ProductPresentation, DomainError>.Good(new ProductPresentation(
            tenantId, productId, displayName, presentationType, sellMode,
            measureUnit, salePrice, costPrice, brand, nominalCapacity));
    }

    public OperationResult<Unit, DomainError> UpdatePrice(Money newCostPrice, Money newSalePrice)
    {
        if (newSalePrice.Amount < newCostPrice.Amount)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("presentation.sale_price_below_cost"));

        CostPrice = newCostPrice;
        SalePrice = newSalePrice;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public void Deactivate()
    {
        IsActive  = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(object @event) => _domainEvents.Add(@event);
}
