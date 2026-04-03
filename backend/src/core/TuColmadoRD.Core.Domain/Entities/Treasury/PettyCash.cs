using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Treasury
{
    public class PettyCash : ITenantEntity
    {
    private PettyCash() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public string Name { get; private set; }
        public Money Balance { get; private set; }
        public Money MaxLimit { get; private set; }

        private PettyCash(TenantIdentifier tenantId, string name, Money initialBalance, Money maxLimit)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            Name = name;
            Balance = initialBalance;
            MaxLimit = maxLimit;
        }

        public static OperationResult<PettyCash, string> Create(
            TenantIdentifier tenantId,
            string name,
            Money initialBalance,
            Money maxLimit)
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult<PettyCash, string>.Bad("Nombre de caja chica requerido.");

            return OperationResult<PettyCash, string>.Good(new PettyCash(tenantId, name, initialBalance, maxLimit));
        }

        public OperationResult<bool, string> ProcessExpense(Money amount)
        {
            var result = Balance - amount;
            if (!result.IsGood)
                return OperationResult<bool, string>.Bad("Fondos de caja chica insuficientes.");

            Balance = result.Result!;
            return OperationResult<bool, string>.Good(true);
        }

        public void Replenish(Money amount)
        {
            Balance = Balance + amount;
        }
    }
}
