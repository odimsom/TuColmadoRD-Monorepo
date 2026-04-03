using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Treasury
{
    public class CashDrawer : ITenantEntity
    {
    private CashDrawer() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid ShiftId { get; private set; }

        public Money OpeningBalance { get; private set; }
        public Money CurrentBalance { get; private set; }
        public bool IsOpen { get; private set; }

        private CashDrawer(TenantIdentifier tenantId, Guid shiftId, Money openingBalance)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            ShiftId = shiftId;
            OpeningBalance = openingBalance;
            CurrentBalance = openingBalance;
            IsOpen = true;
        }

        public static OperationResult<CashDrawer, string> Open(TenantIdentifier tenantId, Guid shiftId, Money openingBalance)
        {
            return OperationResult<CashDrawer, string>.Good(new CashDrawer(tenantId, shiftId, openingBalance));
        }

        public void RecordSale(Money amount)
        {
            CurrentBalance = CurrentBalance + amount;
        }

        public void Close() => IsOpen = false;
    }
}
