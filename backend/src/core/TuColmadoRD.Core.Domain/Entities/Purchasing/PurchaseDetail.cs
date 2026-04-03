using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Purchasing;

public class PurchaseDetail
{
    public Guid Id { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal SubTotal { get; private set; }

    private PurchaseDetail() { }

    internal PurchaseDetail(Guid purchaseOrderId, Guid productId, decimal quantity, decimal unitCost)
    {
        Id = Guid.NewGuid();
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
        SubTotal = quantity * unitCost;
    }
}
