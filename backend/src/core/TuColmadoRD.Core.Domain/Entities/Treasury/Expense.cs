using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Treasury;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Treasury
{
    public class Expense : ITenantEntity
    {
    private Expense() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid CashBoxId { get; private set; }

        public string Description { get; private set; }
        public Money Amount { get; private set; }
        public ExpenseCategory Category { get; private set; }
        public DateTime Date { get; private set; }
        public string? ReferenceNumber { get; private set; }

        private Expense(TenantIdentifier tenantId, Guid cashBoxId, string description, Money amount, ExpenseCategory category)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CashBoxId = cashBoxId;
            Description = description;
            Amount = amount;
            Category = category;
            Date = DateTime.UtcNow;
        }

        public static OperationResult<Expense, string> Record(
            TenantIdentifier tenantId,
            Guid cashBoxId,
            string description,
            Money amount,
            ExpenseCategory category,
            string? reference = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                return OperationResult<Expense, string>.Bad("La descripción del gasto es requerida.");

            if (amount.Amount <= 0)
                return OperationResult<Expense, string>.Bad("El monto del gasto debe ser mayor a cero.");

            return OperationResult<Expense, string>.Good(new Expense(tenantId, cashBoxId, description, amount, category)
            {
                ReferenceNumber = reference
            });
        }
    }
}
