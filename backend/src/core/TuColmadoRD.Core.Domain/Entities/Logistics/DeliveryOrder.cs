using System.Security.Cryptography;
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
        public Guid? DeliveryPersonId { get; private set; }

        public Address Destination { get; private set; }
        public DeliveryStatus Status { get; private set; }
        public string ConfirmationCode { get; private set; }
        public DateTime? DispatchedAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }

        // Unambiguous charset — excludes 0/O, 1/I/L to avoid visual confusion
        private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        private DeliveryOrder(TenantIdentifier tenantId, Guid saleId, Guid? deliveryId, Address destination)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            SaleId = saleId;
            DeliveryPersonId = deliveryId;
            Destination = destination;
            Status = DeliveryStatus.Pending;
            ConfirmationCode = GenerateCode();
        }

        private static string GenerateCode()
        {
            var chars = new char[6];
            for (int i = 0; i < 6; i++)
                chars[i] = CodeChars[RandomNumberGenerator.GetInt32(CodeChars.Length)];
            return new string(chars);
        }

        public static OperationResult<DeliveryOrder, string> Create(
            TenantIdentifier tenantId, Guid saleId, Guid? deliveryId, Address destination)
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
