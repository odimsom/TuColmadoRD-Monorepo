using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Customers;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Customers
{
    public class DebtTransaction : ITenantEntity
    {
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid CustomerAccountId { get; private set; }
        public Guid TerminalId { get; private set; }
        public Money Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public string Concept { get; private set; }
        public string? ReceiptReference { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private DebtTransaction() { }

        private DebtTransaction(
            TenantIdentifier tenantId,
            Guid accountId,
            Guid terminalId,
            Money amount,
            TransactionType type,
            string concept,
            string? receiptReference)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CustomerAccountId = accountId;
            TerminalId = terminalId;
            Amount = amount;
            Type = type;
            Concept = concept;
            ReceiptReference = receiptReference;
            CreatedAt = DateTime.UtcNow;
        }

        public static OperationResult<DebtTransaction, string> Create(
            TenantIdentifier tenantId,
            Guid accountId,
            Guid terminalId,
            Money amount,
            TransactionType type,
            string concept,
            string? receiptReference = null)
        {
            if (string.IsNullOrWhiteSpace(concept))
                return OperationResult<DebtTransaction, string>.Bad("El concepto es obligatorio.");
            if (amount.Amount <= 0)
                return OperationResult<DebtTransaction, string>.Bad("El monto de la transacción debe ser mayor a cero.");

            return OperationResult<DebtTransaction, string>.Good(
                new DebtTransaction(tenantId, accountId, terminalId, amount, type, concept.Trim(), receiptReference)
            );
        }
    }
}
