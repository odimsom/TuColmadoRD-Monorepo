using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Logistics;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Logistics
{
    public class DeliveryOrder : ITenantEntity
    {
    private DeliveryOrder() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid SaleId { get; private set; }
        public Guid DeliveryPersonId { get; private set; }

        public Address Destination { get; private set; }
        public DeliveryStatus Status { get; private set; }
        public DateTime? DispatchedAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }

        private DeliveryOrder(TenantIdentifier tenantId, Guid saleId, Guid deliveryId, Address destination)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            SaleId = saleId;
            DeliveryPersonId = deliveryId;
            Destination = destination;
            Status = DeliveryStatus.Pending;
        }

        public static OperationResult<DeliveryOrder, string> Create(
            TenantIdentifier tenantId, Guid saleId, Guid deliveryId, Address destination)
        {
            return OperationResult<DeliveryOrder, string>.Good(
                new DeliveryOrder(tenantId, saleId, deliveryId, destination));
        }

        public void Dispatch()
        {
            Status = DeliveryStatus.InTransit;
            DispatchedAt = DateTime.UtcNow;
        }

        public void MarkAsDelivered()
        {
            Status = DeliveryStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
        }
    }
}
