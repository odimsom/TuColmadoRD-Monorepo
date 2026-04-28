using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Customers.Events;
using TuColmadoRD.Core.Domain.Enums.Customers;
using TuColmadoRD.Core.Domain.ValueObjects;
using System.Collections.Generic;

namespace TuColmadoRD.Core.Domain.Entities.Customers
{
    public class CustomerAccount : ITenantEntity
    {
        private readonly List<object> _domainEvents = [];
        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

        private readonly List<DebtTransaction> _transactions = [];
        public IReadOnlyCollection<DebtTransaction> Transactions => _transactions.AsReadOnly();

        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid CustomerId { get; private set; }
        public Money Balance { get; private set; }
        public Money CreditLimit { get; private set; }
        public DateTime LastActivity { get; private set; }

        public CreditLimitStatus Status
        {
            get
            {
                if (Balance.Amount > CreditLimit.Amount)
                    return CreditLimitStatus.Exceeded;

                if (Balance.Amount >= (CreditLimit.Amount * 0.8m))
                    return CreditLimitStatus.NearLimit;

                return CreditLimitStatus.Healthy;
            }
        }

        private CustomerAccount() { }

        private CustomerAccount(TenantIdentifier tenantId, Guid customerId, Money creditLimit)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CustomerId = customerId;
            Balance = Money.Zero;
            CreditLimit = creditLimit;
            LastActivity = DateTime.UtcNow;
        }

        public static OperationResult<CustomerAccount, string> Create(TenantIdentifier tenantId, Guid customerId, Money creditLimit)
        {
            if (creditLimit.Amount < 0)
                return OperationResult<CustomerAccount, string>.Bad("El l�mite de cr�dito no puede ser negativo.");

            return OperationResult<CustomerAccount, string>.Good(new CustomerAccount(tenantId, customerId, creditLimit));
        }

        /// <summary>
        /// Incrementa la deuda (Fiao) creando una transacci�n.
        /// </summary>
        public OperationResult<bool, string> RecordCharge(Money amount, Guid terminalId, string concept, string? receiptReference = null)
        {
            var projectedBalance = Balance + amount;

            if (projectedBalance.Amount > CreditLimit.Amount)
            {
                return OperationResult<bool, string>.Bad(
                    $"Operacin rechazada: El lmite de crdito es {CreditLimit} y el nuevo balance de {projectedBalance} lo excedera.");
            }

            var transactionResult = DebtTransaction.Create(
                TenantId, Id, terminalId, amount, TransactionType.Charge, concept, receiptReference);

            if (!transactionResult.IsGood)
                return OperationResult<bool, string>.Bad(transactionResult.Error!);

            var transaction = transactionResult.Result!;
            _transactions.Add(transaction);

            ApplyBalanceChange(amount, TransactionType.Charge, transaction.Id, transaction.CreatedAt);

            return OperationResult<bool, string>.Good(true);
        }

        public void ApplyBalanceChange(Money amount, TransactionType type, Guid transactionId, DateTime transactionCreatedAt)
        {
            if (type == TransactionType.Charge)
            {
                Balance += amount;
                AddDomainEvent(new ChargeRegisteredDomainEvent(
                    Id, TenantId, CustomerId, amount, Balance, transactionId, transactionCreatedAt));
            }
            else if (type == TransactionType.Credit)
            {
                var subtractResult = Balance - amount;
                Balance = subtractResult.IsGood ? subtractResult.Result! : Money.Zero;
                AddDomainEvent(new PaymentRegisteredDomainEvent(
                    Id, TenantId, CustomerId, amount, Balance, transactionId, transactionCreatedAt));
            }

            LastActivity = DateTime.UtcNow;
        }

        /// <summary>
        /// Registra un abono (Pagar la libreta) creando una transacci�n.
        /// </summary>
        public OperationResult<bool, string> RecordPayment(Money amount, Guid terminalId, string concept, string? receiptReference = null)
        {
            var newBalanceResult = Balance - amount;

            if (!newBalanceResult.IsGood)
            {
                return OperationResult<bool, string>.Bad($"Error calculando nuevo balance: {newBalanceResult.Error}");
            }

            var transactionResult = DebtTransaction.Create(
                TenantId, Id, terminalId, amount, TransactionType.Credit, concept, receiptReference);

            if (!transactionResult.IsGood)
                return OperationResult<bool, string>.Bad(transactionResult.Error!);

            var transaction = transactionResult.Result!;
            _transactions.Add(transaction);

            Balance = newBalanceResult.Result!;
            LastActivity = DateTime.UtcNow;

            AddDomainEvent(new PaymentRegisteredDomainEvent(
                Id, TenantId, CustomerId, amount, Balance, transaction.Id, transaction.CreatedAt));

            return OperationResult<bool, string>.Good(true);
        }

        public void UpdateCreditLimit(Money newLimit)
        {
            CreditLimit = newLimit;
            LastActivity = DateTime.UtcNow;
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
        private void AddDomainEvent(object @event) => _domainEvents.Add(@event);
    }
}
