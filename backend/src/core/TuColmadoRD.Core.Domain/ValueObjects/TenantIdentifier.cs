using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    public record TenantIdentifier
    {
        public Guid Value { get; private init; }

        private TenantIdentifier(Guid value) => Value = value;

        public static TenantIdentifier Empty => new(Guid.Empty);

        public static OperationResult<TenantIdentifier, string> Validate(Guid value)
        {
            return value == Guid.Empty
                ? OperationResult<TenantIdentifier, string>.Bad("TenantId inválido.")
                : OperationResult<TenantIdentifier, string>.Good(new TenantIdentifier(value));
        }
        public static implicit operator Guid(TenantIdentifier tenantId) => tenantId.Value;
        public override string ToString() => Value.ToString();
    }
}