using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Fiscal;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Fiscal
{
    public class Tax : ITenantEntity
    {
    private Tax() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }

        public string Name { get; private set; }
        public TaxType Type { get; private set; }
        public TaxRate Rate { get; private set; }
        public bool IsActive { get; private set; }

        private Tax(TenantIdentifier tenantId, string name, TaxType type, TaxRate rate)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            Name = name;
            Type = type;
            Rate = rate;
            IsActive = true;
        }

        public static OperationResult<Tax, string> Create(
            TenantIdentifier tenantId,
            string name,
            TaxType type,
            TaxRate rate)
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult<Tax, string>.Bad("El nombre del impuesto es obligatorio.");

            return OperationResult<Tax, string>.Good(new Tax(tenantId, name.Trim(), type, rate));
        }

        public void Deactivate() => IsActive = false;

        public OperationResult<Money, string> Calculate(Money baseAmount)
        {
            var taxResult = Rate.CalculateTax(baseAmount);
            return taxResult.IsGood
                ? OperationResult<Money, string>.Good(taxResult.Result)
                : OperationResult<Money, string>.Bad(taxResult.Error.ToString());
        }
    }
}
