using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.HumanResources
{
    public class WorkShift : ITenantEntity
    {
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public string Name { get; private set; }
        
        private WorkShift() 
        { 
            Name = string.Empty;
        }

        public WorkShift(TenantIdentifier tenantId, string name)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            Name = name;
        }
    }
}
