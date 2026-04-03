using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.HumanResources;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.HumanResources
{
    public class Employee : ITenantEntity
    {
    private Employee() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }

        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Cedula? IdCard { get; private set; }
        public Phone? Phone { get; private set; }

        public EmployeeRole Role { get; private set; }
        public DateTime HireDate { get; private set; }
        public bool IsActive { get; private set; }

        private Employee(TenantIdentifier tenantId, string firstName, string lastName, EmployeeRole role)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            HireDate = DateTime.UtcNow;
            IsActive = true;
        }

        public static OperationResult<Employee, string> Create(
            TenantIdentifier tenantId,
            string firstName,
            string lastName,
            EmployeeRole role,
            Cedula? idCard = null,
            Phone? phone = null)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return OperationResult<Employee, string>.Bad("El nombre completo es obligatorio.");

            return OperationResult<Employee, string>.Good(new Employee(tenantId, firstName.Trim(), lastName.Trim(), role)
            {
                IdCard = idCard,
                Phone = phone
            });
        }

        public void UpdateRole(EmployeeRole newRole) => Role = newRole;
        public void Deactivate() => IsActive = false;
        public string FullName => $"{FirstName} {LastName}";
    }
}
