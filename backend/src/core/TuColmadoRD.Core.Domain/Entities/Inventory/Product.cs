using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory.Events;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory;

/// <summary>
/// Product aggregate root for inventory operations.
/// </summary>
public class Product : ITenantEntity
{
    private readonly List<object> _domainEvents = [];

    private Product()
    {
        Name = string.Empty;
        TenantId = TenantIdentifier.Empty;
        CostPrice = Money.Zero;
        SalePrice = Money.Zero;
        ItbisRate = TaxRate.Zero;
        UnitType = UnitType.Unit;
    }

    private Product(
        Guid tenantId,
        string name,
        Guid categoryId,
        Money costPrice,
        Money salePrice,
        TaxRate itbisRate,
        UnitType unitType)
    {
        Id = Guid.NewGuid();
        TenantId = TenantIdentifier.Validate(tenantId).Result;
        Name = name;
        CategoryId = categoryId;
        CostPrice = costPrice;
        SalePrice = salePrice;
        ItbisRate = itbisRate;
        UnitType = unitType;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        StockQuantity = 0m;

        AddDomainEvent(new ProductCreatedDomainEvent(Id, tenantId, Name, DateTime.UtcNow));
    }

    /// <summary>
    /// Product identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Tenant identifier.
    /// </summary>
    public TenantIdentifier TenantId { get; private set; }

    /// <summary>
    /// Product name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Category identifier.
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// Cost price.
    /// </summary>
    public Money CostPrice { get; private set; }

    /// <summary>
    /// Sale price.
    /// </summary>
    public Money SalePrice { get; private set; }

    /// <summary>
    /// ITBIS tax rate.
    /// </summary>
    public TaxRate ItbisRate { get; private set; }

    /// <summary>
    /// Unit type.
    /// </summary>
    public UnitType UnitType { get; private set; }

    /// <summary>
    /// Indicates if product is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Last update timestamp (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Current stock quantity.
    /// </summary>
    public decimal StockQuantity { get; private set; }

    /// <summary>
    /// Buffered domain events emitted by this aggregate.
    /// </summary>
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Creates a new product aggregate.
    /// </summary>
    public static OperationResult<Product, DomainError> Create(
        Guid tenantId,
        string name,
        Guid categoryId,
        Money costPrice,
        Money salePrice,
        TaxRate itbisRate,
        UnitType unitType)
    {
        if (tenantId == Guid.Empty)
        {
            return OperationResult<Product, DomainError>.Bad(DomainError.Validation("product.tenant_required"));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<Product, DomainError>.Bad(DomainError.Validation("product.name_required"));
        }

        if (name.Length > 120)
        {
            return OperationResult<Product, DomainError>.Bad(DomainError.Validation("product.name_too_long"));
        }

        if (categoryId == Guid.Empty)
        {
            return OperationResult<Product, DomainError>.Bad(DomainError.Validation("product.category_required"));
        }

        if (salePrice.Amount < costPrice.Amount)
        {
            return OperationResult<Product, DomainError>.Bad(DomainError.Validation("product.sale_price_below_cost"));
        }

        return OperationResult<Product, DomainError>.Good(
            new Product(tenantId, name.Trim(), categoryId, costPrice, salePrice, itbisRate, unitType));
    }

    /// <summary>
    /// Rehydrates product for catalog sync insert flow.
    /// </summary>
    public static OperationResult<Product, DomainError> RehydrateForCatalogSync(
        Guid productId,
        Guid tenantId,
        Guid categoryId,
        string name,
        Money costPrice,
        Money salePrice,
        TaxRate itbisRate)
    {
        var createResult = Create(tenantId, name, categoryId, costPrice, salePrice, itbisRate, UnitType.Unit);
        if (!createResult.TryGetResult(out var product) || product is null)
        {
            return OperationResult<Product, DomainError>.Bad(createResult.Error);
        }

        product.Id = productId;
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.ClearDomainEvents();

        return OperationResult<Product, DomainError>.Good(product);
    }

    /// <summary>
    /// Updates product prices.
    /// </summary>
    public OperationResult<Unit, DomainError> UpdatePrice(Money newCostPrice, Money newSalePrice)
    {
        if (newSalePrice.Amount < newCostPrice.Amount)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("product.sale_price_below_cost"));
        }

        CostPrice = newCostPrice;
        SalePrice = newSalePrice;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductPriceUpdatedDomainEvent(Id, TenantId, SalePrice, DateTime.UtcNow));

        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    /// <summary>
    /// Updates basic fields from catalog synchronization payload.
    /// </summary>
    public OperationResult<Unit, DomainError> UpdateFromCatalogSync(
        Guid categoryId,
        string name,
        Money costPrice,
        Money salePrice)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("product.name_required"));
        }

        if (name.Length > 120)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("product.name_too_long"));
        }

        if (salePrice.Amount < costPrice.Amount)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("product.sale_price_below_cost"));
        }

        CategoryId = categoryId;
        Name = name.Trim();
        CostPrice = costPrice;
        SalePrice = salePrice;
        UpdatedAt = DateTime.UtcNow;

        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    /// <summary>
    /// Adjusts stock by delta quantity.
    /// </summary>
    public OperationResult<Unit, DomainError> AdjustStock(decimal delta)
    {
        var newStock = StockQuantity + delta;
        if (newStock < 0)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("product.insufficient_stock"));
        }

        StockQuantity = newStock;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new StockAdjustedDomainEvent(Id, TenantId, delta, StockQuantity, DateTime.UtcNow));

        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    /// <summary>
    /// Deactivates product.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductDeactivatedDomainEvent(Id, TenantId, DateTime.UtcNow));
    }

    /// <summary>
    /// Clears pending aggregate domain events.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(object @event) => _domainEvents.Add(@event);
}
