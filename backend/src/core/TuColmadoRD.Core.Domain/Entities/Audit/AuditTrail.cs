using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Audit
{
    public class AuditTrail
    {
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public string TableName { get; private set; }
        public string Action { get; private set; }
        public string OldValues { get; private set; }
        public string NewValues { get; private set; }
        public DateTime Timestamp { get; private set; }

        private AuditTrail() 
        { 
            TableName = string.Empty;
            Action = string.Empty;
            OldValues = string.Empty;
            NewValues = string.Empty;
        }

        private AuditTrail(TenantIdentifier tenantId, string tableName, string action, string oldValues, string newValues)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            TableName = tableName;
            Action = action;
            OldValues = oldValues;
            NewValues = newValues;
            Timestamp = DateTime.UtcNow;
        }

        public static OperationResult<AuditTrail, string> Create(
            TenantIdentifier tenantId,
            string tableName,
            string action,
            string oldValues,
            string newValues)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return OperationResult<AuditTrail, string>.Bad("TableName is required.");

            if (string.IsNullOrWhiteSpace(action))
                return OperationResult<AuditTrail, string>.Bad("Action is required.");

            return OperationResult<AuditTrail, string>.Good(new AuditTrail(tenantId, tableName, action, oldValues, newValues));
        }
    }
}
