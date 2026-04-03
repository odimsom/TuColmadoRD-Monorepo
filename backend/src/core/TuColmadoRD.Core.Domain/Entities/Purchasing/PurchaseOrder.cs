using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Purchasing.Events;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Purchasing;

public class PurchaseOrder : ITenantEntity
{
    private readonly List<object> _domainEvents = [];
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid ShiftId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public PurchaseStatus Status { get; private set; }
    public string SupplierNcf { get; private set; }
    public decimal TotalAmount { get; private set; }

    private readonly List<PurchaseDetail> _details = [];
    public IReadOnlyCollection<PurchaseDetail> Details => _details.AsReadOnly();

    private PurchaseOrder()
    {
        TenantId = null!;
        SupplierNcf = string.Empty;
    }

    private PurchaseOrder(TenantIdentifier tenantId, Guid supplierId, Guid shiftId, string supplierNcf)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        SupplierId = supplierId;
        ShiftId = shiftId;
        SupplierNcf = supplierNcf ?? string.Empty;
        OrderDate = DateTime.UtcNow;
        Status = PurchaseStatus.Draft;
        TotalAmount = 0m;
    }

    public static OperationResult<PurchaseOrder, DomainError> Create(TenantIdentifier tenantId, Guid supplierId, Guid shiftId, string supplierNcf)
    {
        if (supplierId == Guid.Empty)
            return OperationResult<PurchaseOrder, DomainError>.Bad(DomainError.Validation("PurchaseOrder.InvalidSupplier", "El proveedor es requerido."));

        if (shiftId == Guid.Empty)
            return OperationResult<PurchaseOrder, DomainError>.Bad(DomainError.Validation("PurchaseOrder.InvalidShift", "El turno activo es requerido."));

        return OperationResult<PurchaseOrder, DomainError>.Good(new PurchaseOrder(tenantId, supplierId, shiftId, supplierNcf));
    }

    public void AddDetail(Guid productId, decimal quantity, decimal cost)
    {
        var detail = new PurchaseDetail(Id, productId, quantity, cost);
        _details.Add(detail);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        TotalAmount = _details.Sum(x => x.SubTotal);
    }

    public OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError> CompletePurchase()
    {
        if (Status != PurchaseStatus.Draft)
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.Validation("PurchaseOrder.InvalidStatus", "La compra ya fue procesada."));

        if (!_details.Any())
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.Validation("PurchaseOrder.EmptyDetails", "Debe agregar al menos un producto a la compra."));

        Status = PurchaseStatus.Received;

        var detailEvents = _details.Select(d => new PurchaseDetailEventData(
            d.ProductId,
            d.Quantity,
            d.UnitCost)).ToList().AsReadOnly();

        _domainEvents.Add(new PurchaseCompletedDomainEvent(
            Id,
            TenantId,
            ShiftId,
            TotalAmount,
            detailEvents,
            DateTime.UtcNow));

        return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Good(TuColmadoRD.Core.Domain.Base.Result.Unit.Value);
    }
}
