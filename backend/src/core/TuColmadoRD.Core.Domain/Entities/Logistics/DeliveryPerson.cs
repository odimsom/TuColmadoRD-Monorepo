using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Logistics
{
    public class DeliveryPerson : ITenantEntity
    {
    private DeliveryPerson() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }

        public string Name { get; private set; }
        public Phone? ContactPhone { get; private set; }
        public string? VehiclePlate { get; private set; }

        public bool IsActive { get; private set; }
        public bool IsAvailable { get; private set; }

        private DeliveryPerson(TenantIdentifier tenantId, string name, Phone? phone, string? plate)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            Name = name;
            ContactPhone = phone;
            VehiclePlate = plate;
            IsActive = true;
            IsAvailable = true;
        }

        public static OperationResult<DeliveryPerson, string> Create(
            TenantIdentifier tenantId,
            string name,
            Phone? phone = null,
            string? plate = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return OperationResult<DeliveryPerson, string>.Bad("El nombre del delivery es obligatorio.");

            return OperationResult<DeliveryPerson, string>.Good(new DeliveryPerson(tenantId, name.Trim(), phone, plate));
        }

        public void SetAvailability(bool available) => IsAvailable = available;
        public void Deactivate() => IsActive = false;
    }
}
