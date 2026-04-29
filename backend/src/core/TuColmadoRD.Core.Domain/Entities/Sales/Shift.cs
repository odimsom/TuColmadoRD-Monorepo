using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales.Events;
using TuColmadoRD.Core.Domain.Enums.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Sales
{
    public class Shift : ITenantEntity
    {
        private readonly List<object> _domainEvents = [];

        private Shift()
        {
            TenantId = TenantIdentifier.Empty;
            CashierName = string.Empty;
            OpeningCashAmount = Money.Zero;
            Status = ShiftStatus.Open;
            TotalSalesAmount = Money.Zero;
        }

        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid TerminalId { get; private set; }
        public ShiftStatus Status { get; private set; }
        public string CashierName { get; private set; }
        public Money OpeningCashAmount { get; private set; }
        public Money? ClosingCashAmount { get; private set; }
        public DateTime OpenedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }
        public Money? ExpectedCashAmount { get; private set; }
        public Money? ActualCashAmount { get; private set; }
        public decimal? CashDifferenceAmount { get; private set; }
        public string? Notes { get; private set; }
        public int TotalSalesCount { get; private set; }
        public Money TotalSalesAmount { get; private set; }

        public Money TotalExpenses { get; private set; } = Money.Zero;
        public Money TotalCashSales { get; private set; } = Money.Zero;

        public Money TotalAccountPayments { get; private set; } = Money.Zero;   
        public Money TotalCashIn { get; private set; } = Money.Zero;
        public Money TotalCardIn { get; private set; } = Money.Zero;
        public Money TotalTransferIn { get; private set; } = Money.Zero;

        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

        private Shift(Guid tenantId, Guid terminalId, Money openingCash, string cashierName)
        {
            Id = Guid.NewGuid();
            TenantId = TenantIdentifier.Validate(tenantId).Result;
            TerminalId = terminalId;
            Status = ShiftStatus.Open;
            CashierName = cashierName;
            OpeningCashAmount = openingCash;
            OpenedAt = DateTime.UtcNow;
            TotalSalesCount = 0;
            TotalSalesAmount = Money.Zero;

            AddDomainEvent(new ShiftOpenedDomainEvent(
                Id,
                tenantId,
                terminalId,
                openingCash.Amount,
                cashierName,
                OpenedAt));
        }

        public static OperationResult<Shift, DomainError> Open(
            Guid tenantId,
            Guid terminalId,
            Money openingCash,
            string cashierName)
        {
            if (tenantId == Guid.Empty)
            {
                return OperationResult<Shift, DomainError>.Bad(DomainError.Validation("shift.tenant_required"));
            }

            if (string.IsNullOrWhiteSpace(cashierName))
            {
                return OperationResult<Shift, DomainError>.Bad(DomainError.Validation("shift.cashier_name_required"));
            }

            if (cashierName.Length > 100)
            {
                return OperationResult<Shift, DomainError>.Bad(DomainError.Validation("shift.cashier_name_too_long"));
            }

            if (openingCash.Amount < 0)
            {
                return OperationResult<Shift, DomainError>.Bad(DomainError.Validation("shift.negative_cash"));
            }

            return OperationResult<Shift, DomainError>.Good(
                new Shift(tenantId, terminalId, openingCash, cashierName.Trim()));
        }

        public OperationResult<Unit, DomainError> Close(
            Money actualCashAmount,
            Money expectedCashAmount,
            string? notes)
        {
            if (Status != ShiftStatus.Open)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Business("shift.already_closed"));
            }

            if (!string.IsNullOrEmpty(notes) && notes.Length > 500)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("shift.notes_too_long"));
            }

            if (actualCashAmount.Amount < 0 || expectedCashAmount.Amount < 0)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("shift.negative_cash"));
            }

            Status = ShiftStatus.Closed;
            ClosedAt = DateTime.UtcNow;
            ClosingCashAmount = actualCashAmount;
            ActualCashAmount = actualCashAmount;
            ExpectedCashAmount = expectedCashAmount;
            CashDifferenceAmount = actualCashAmount.Amount - expectedCashAmount.Amount;
            Notes = notes;

            AddDomainEvent(new ShiftClosedDomainEvent(
                Id,
                TenantId,
                TerminalId,
                actualCashAmount.Amount,
                expectedCashAmount.Amount,
                CashDifferenceAmount.Value,
                TotalSalesCount,
                TotalSalesAmount.Amount,
                ClosedAt.Value));

            return OperationResult<Unit, DomainError>.Good(Unit.Value);
        }

        public OperationResult<Unit, DomainError> RegisterSale(Money saleTotal)
        {
            if (Status != ShiftStatus.Open)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Business("shift.closed_cannot_register_sale"));
            }

            if (saleTotal.Amount < 0)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("shift.negative_cash"));
            }

            TotalSalesCount += 1;
            TotalSalesAmount += saleTotal;

            return OperationResult<Unit, DomainError>.Good(Unit.Value);
        }
        public OperationResult<Unit, DomainError> RegisterExpense(Money amount)
        {
            if (Status != ShiftStatus.Open)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Business("shift.closed_cannot_register_expense"));
            }

            if (amount.Amount <= 0)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("shift.expense_amount_invalid"));
            }

            var availableCash = OpeningCashAmount.Amount + TotalCashIn.Amount + TotalCashSales.Amount - TotalExpenses.Amount;
            if (amount.Amount > availableCash)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Business("shift.insufficient_cash", "No hay suficiente efectivo en caja para registrar este gasto."));
            }

            TotalExpenses += amount;

            return OperationResult<Unit, DomainError>.Good(Unit.Value);
        }

        public OperationResult<Unit, DomainError> RegisterAccountPayment(Money amount, PaymentMethod method)
        {
            if (Status != ShiftStatus.Open)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Business("shift.closed_cannot_register_payment"));
            }

            if (amount.Amount <= 0)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("shift.payment_amount_invalid"));
            }

            if (method == PaymentMethod.Credit)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("shift.payment_cannot_be_credit"));
            }

            TotalAccountPayments += amount;

            if (method == PaymentMethod.Cash)
                TotalCashIn += amount;
            else if (method == PaymentMethod.Card)
                TotalCardIn += amount;
            else if (method == PaymentMethod.Transfer)
                TotalTransferIn += amount;

            return OperationResult<Unit, DomainError>.Good(Unit.Value);
        }
        public OperationResult<Unit, DomainError> ReverseSale(Money saleTotal)
        {
            if (Status != ShiftStatus.Open)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Business("shift.closed_cannot_reverse_sale"));
            }

            if (saleTotal.Amount < 0)
            {
                return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("shift.negative_cash"));
            }

            if (TotalSalesCount > 0)
            {
                TotalSalesCount -= 1;
            }

            var newTotal = Math.Max(0m, TotalSalesAmount.Amount - saleTotal.Amount);
            TotalSalesAmount = Money.FromDecimal(newTotal).Result;

            return OperationResult<Unit, DomainError>.Good(Unit.Value);
        }

        public void ClearDomainEvents() => _domainEvents.Clear();

        private void AddDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);
    }
}
