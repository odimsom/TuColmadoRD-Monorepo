using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory;

/// <summary>
/// Inventory category entity.
/// </summary>
public class Category : ITenantEntity
{
    private Category()
    {
        Name = string.Empty;
        TenantId = TenantIdentifier.Empty;
    }

    private Category(Guid tenantId, string name)
    {
        Id = Guid.NewGuid();
        TenantId = TenantIdentifier.Validate(tenantId).Result;
        Name = name;
        IsActive = true;
    }

    /// <summary>
    /// Category identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Tenant identifier.
    /// </summary>
    public TenantIdentifier TenantId { get; private set; }

    /// <summary>
    /// Category display name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Indicates if category is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Deactivates this category.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Creates a category.
    /// </summary>
    public static OperationResult<Category, DomainError> Create(Guid tenantId, string name)
    {
        if (tenantId == Guid.Empty)
        {
            return OperationResult<Category, DomainError>.Bad(DomainError.Validation("category.tenant_required"));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<Category, DomainError>.Bad(DomainError.Validation("category.name_required"));
        }

        if (name.Length > 80)
        {
            return OperationResult<Category, DomainError>.Bad(DomainError.Validation("category.name_too_long"));
        }

        return OperationResult<Category, DomainError>.Good(new Category(tenantId, name.Trim()));
    }
}
