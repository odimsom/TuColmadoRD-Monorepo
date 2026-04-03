using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Treasury
{
    public class CashBox : ITenantEntity
    {
    private CashBox() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public string Name { get; private set; }
        public Money Balance { get; private set; }
        public bool IsActive { get; private set; }

        private CashBox(TenantIdentifier tenantId, string name, Money initialBalance)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            Name = name;
            Balance = initialBalance;
            IsActive = true;
        }

        public static OperationResult<CashBox, string> Create(TenantIdentifier tenantId, string name, Money initialBalance)
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult<CashBox, string>.Bad("El nombre de la caja es obligatorio.");

            return OperationResult<CashBox, string>.Good(new CashBox(tenantId, name, initialBalance));
        }

        public OperationResult<bool, string> Withdraw(Money amount)
        {
            var result = Balance - amount;
            if (!result.IsGood)
                return OperationResult<bool, string>.Bad("Fondos insuficientes en caja chica.");

            Balance = result.Result!;
            return OperationResult<bool, string>.Good(true);
        }

        public void Deposit(Money amount)
        {
            Balance = Balance + amount;
        }
    }
}
