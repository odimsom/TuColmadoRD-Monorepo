using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Purchasing;

public class Supplier : ITenantEntity
{
    public Guid Id { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public string Name { get; private set; }
    public Rnc Rnc { get; private set; }
    public SupplierType Type { get; private set; }
    public bool IsActive { get; private set; }

    private Supplier()
    {
        Name = string.Empty;
        TenantId = null!;
        Rnc = null!;
    }

    private Supplier(TenantIdentifier tenantId, string name, Rnc rnc, SupplierType type)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name;
        Rnc = rnc;
        Type = type;
        IsActive = true;
    }

    public static OperationResult<Supplier, DomainError> Create(TenantIdentifier tenantId, string name, Rnc rnc, SupplierType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<Supplier, DomainError>.Bad(DomainError.Validation("Supplier.InvalidName", "Nombre del proveedor requerido."));

        return OperationResult<Supplier, DomainError>.Good(new Supplier(tenantId, name.Trim(), rnc, type));
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
