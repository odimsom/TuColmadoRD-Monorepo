using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Customers.Events;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Customers
{
    public class Customer : ITenantEntity
    {
        private readonly List<object> _domainEvents = [];
        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public string FullName { get; private set; }
        public Cedula? DocumentId { get; private set; }
        public Phone? ContactPhone { get; private set; }
        public Address? HomeAddress { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Customer() { }

        private Customer(TenantIdentifier tenantId, string fullName, Cedula? documentId, Phone? phone, Address? address)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            FullName = fullName;
            DocumentId = documentId;
            ContactPhone = phone;
            HomeAddress = address;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;

            AddDomainEvent(new CustomerCreatedDomainEvent(Id, TenantId, FullName, CreatedAt));
        }

        public static OperationResult<Customer, string> Create(
            TenantIdentifier tenantId,
            string fullName,
            Cedula? documentId = null,
            Phone? phone = null,
            Address? address = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return OperationResult<Customer, string>.Bad("El nombre completo es obligatorio.");
            if (fullName.Length > 100)
                return OperationResult<Customer, string>.Bad("El nombre completo no puede exceder los 100 caracteres.");

            return OperationResult<Customer, string>.Good(new Customer(tenantId, fullName, documentId, phone, address));
        }

        public void Deactivate() => IsActive = false;

        public void ClearDomainEvents() => _domainEvents.Clear();
        private void AddDomainEvent(object @event) => _domainEvents.Add(@event);
    }
}
