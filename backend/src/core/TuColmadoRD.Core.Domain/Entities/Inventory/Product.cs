using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory.Events;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory;

public class Product : ITenantEntity
{
    private readonly List<object> _domainEvents = [];
    private readonly List<ProductPresentation> _presentations = [];

    private Product()
    {
        Name     = string.Empty;
        TenantId = TenantIdentifier.Empty;
    }

    private Product(Guid tenantId, string name, Guid categoryId, TaxRate itbisRate)
    {
        Id        = Guid.NewGuid();
        TenantId  = TenantIdentifier.Validate(tenantId).Result;
        Name      = name;
        CategoryId = categoryId;
        ItbisRate = itbisRate;
        IsActive  = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductCreatedDomainEvent(Id, tenantId, Name, DateTime.UtcNow));
    }

    private Product(Guid productId, Guid tenantId, string name, Guid categoryId, TaxRate itbisRate)
    {
        Id        = productId;
        TenantId  = TenantIdentifier.Validate(tenantId).Result;
        Name      = name;
        CategoryId = categoryId;
        ItbisRate = itbisRate;
        IsActive  = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id              { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public string Name          { get; private set; }
    public Guid CategoryId      { get; private set; }
    public TaxRate ItbisRate    { get; private set; }
    public bool IsActive        { get; private set; }
    public DateTime CreatedAt   { get; private set; }
    public DateTime UpdatedAt   { get; private set; }

    public IReadOnlyCollection<ProductPresentation> Presentations => _presentations.AsReadOnly();
    public IReadOnlyCollection<object> DomainEvents               => _domainEvents.AsReadOnly();

    public static OperationResult<Product, DomainError> Create(
        Guid tenantId,
        string name,
        Guid categoryId,
        TaxRate itbisRate)
    {
        if (tenantId == Guid.Empty)
            return OperationResult<Product, DomainError>.Bad(DomainError.Validation("product.tenant_required"));

        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<Product, DomainError>.Bad(DomainError.Validation("product.name_required"));

        if (name.Length > 120)
            return OperationResult<Product, DomainError>.Bad(DomainError.Validation("product.name_too_long"));

        if (categoryId == Guid.Empty)
            return OperationResult<Product, DomainError>.Bad(DomainError.Validation("product.category_required"));

        return OperationResult<Product, DomainError>.Good(new Product(tenantId, name.Trim(), categoryId, itbisRate));
    }

    /// <summary>Reconstructs a product from an external catalog, preserving the remote ID.</summary>
    public static Product Rehydrate(Guid productId, Guid tenantId, string name, Guid categoryId, TaxRate itbisRate) =>
        new Product(productId, tenantId, name.Trim(), categoryId, itbisRate);

    public OperationResult<Unit, DomainError> UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("product.name_required"));

        if (name.Length > 120)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("product.name_too_long"));

        Name      = name.Trim();
        UpdatedAt = DateTime.UtcNow;
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public OperationResult<Unit, DomainError> UpdateItbisRate(TaxRate itbisRate)
    {
        ItbisRate = itbisRate;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public void Deactivate()
    {
        IsActive  = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductDeactivatedDomainEvent(Id, TenantId, DateTime.UtcNow));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(object @event) => _domainEvents.Add(@event);
}
