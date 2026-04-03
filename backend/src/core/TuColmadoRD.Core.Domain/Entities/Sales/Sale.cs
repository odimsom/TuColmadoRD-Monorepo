using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales.Events;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Sales;

/// <summary>
/// Sale aggregate root orchestrating sale items, payments, and business invariants.
/// </summary>
public class Sale : ITenantEntity
{
    private readonly List<SaleItem> _items = [];
    private readonly List<SalePayment> _payments = [];
    private readonly List<object> _domainEvents = [];

    private Sale()
    {
        TenantId = TenantIdentifier.Empty;
        ReceiptNumber = string.Empty;
        CashierName = string.Empty;
        StatusId = SaleStatus.Completed.Id;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public Guid TerminalId { get; private set; }
    public Guid ShiftId { get; private set; }
    public string ReceiptNumber { get; private set; }
    public int StatusId { get; private set; }
    public string CashierName { get; private set; }

    public IReadOnlyList<SaleItem> Items => _items.AsReadOnly();
    public IReadOnlyList<SalePayment> Payments => _payments.AsReadOnly();

    public decimal SubtotalAmount { get; private set; }
    public decimal TotalItbisAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TotalPaidAmount { get; private set; }
    public decimal ChangeDueAmount { get; private set; }

    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? VoidedAt { get; private set; }
    public string? VoidReason { get; private set; }

    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public static OperationResult<Sale, DomainError> Create(
        Guid tenantId,
        Guid terminalId,
        Guid shiftId,
        string cashierName,
        string receiptNumber,
        string? notes)
    {
        if (tenantId == Guid.Empty)
        {
            return OperationResult<Sale, DomainError>.Bad(DomainError.Validation("sale.tenant_required"));
        }

        if (terminalId == Guid.Empty)
        {
            return OperationResult<Sale, DomainError>.Bad(DomainError.Validation("sale.terminal_required"));
        }

        if (shiftId == Guid.Empty)
        {
            return OperationResult<Sale, DomainError>.Bad(DomainError.Validation("sale.shift_required"));
        }

        if (string.IsNullOrWhiteSpace(cashierName))
        {
            return OperationResult<Sale, DomainError>.Bad(DomainError.Validation("sale.cashier_name_required"));
        }

        if (string.IsNullOrWhiteSpace(receiptNumber))
        {
            return OperationResult<Sale, DomainError>.Bad(DomainError.Validation("sale.receipt_number_required"));
        }

        if (!string.IsNullOrEmpty(notes) && notes.Length > 300)
        {
            return OperationResult<Sale, DomainError>.Bad(DomainError.Validation("sale.notes_too_long"));
        }

        return OperationResult<Sale, DomainError>.Good(new Sale
        {
            Id = Guid.NewGuid(),
            TenantId = TenantIdentifier.Validate(tenantId).Result,
            TerminalId = terminalId,
            ShiftId = shiftId,
            CashierName = cashierName.Trim(),
            ReceiptNumber = receiptNumber.Trim(),
            Notes = notes?.Trim(),
            StatusId = SaleStatus.Completed.Id,
            CreatedAt = DateTime.UtcNow
        });
    }

    public OperationResult<Unit, DomainError> AddItem(
        Guid productId,
        string productName,
        Money unitPrice,
        Money costPrice,
        Quantity quantity,
        TaxRate itbisRate)
    {
        if (StatusId != SaleStatus.Completed.Id)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("sale.cannot_modify_finalized"));
        }

        if (productId == Guid.Empty)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("sale.product_required"));
        }

        if (_items.Any(i => i.ProductId == productId))
        {
            return OperationResult<Unit, DomainError>.Bad(
                DomainError.Business("sale.duplicate_product", "Use AdjustItemQuantity para modificar una linea existente."));
        }

        var item = new SaleItem(productId, productName, unitPrice, costPrice, quantity, itbisRate)
        {
            SaleId = Id
        };

        _items.Add(item);
        RecalculateTotals();

        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public OperationResult<Unit, DomainError> AddPayment(
        PaymentMethod method,
        Money amount,
        string? reference,
        Guid? customerId = null)
    {
        if (StatusId != SaleStatus.Completed.Id)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("sale.cannot_modify_finalized"));
        }

        if (amount.Amount <= 0)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("sale.payment_amount_must_be_positive"));
        }

        if (method.Id == PaymentMethod.Credit.Id && customerId is null)
        {
            return OperationResult<Unit, DomainError>.Bad(
                DomainError.Validation("sale.credit_payment_customer_required", "El pago a credito requiere un cliente."));
        }

        if (customerId.HasValue && customerId.Value == Guid.Empty)
        {
            return OperationResult<Unit, DomainError>.Bad(
                DomainError.Validation("sale.credit_payment_customer_invalid", "CustomerId no es valido."));
        }

        var payment = new SalePayment(method, amount.Amount, reference, customerId)
        {
            SaleId = Id
        };

        _payments.Add(payment);

        TotalPaidAmount = _payments.Sum(p => p.AmountValue);
        ChangeDueAmount = Math.Max(0m, TotalPaidAmount - TotalAmount);

        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public OperationResult<Unit, DomainError> Finalize()
    {
        if (_items.Count == 0)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("sale.no_items"));
        }

        if (_payments.Count == 0)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("sale.no_payments"));
        }

        if (TotalPaidAmount < TotalAmount)
        {
            return OperationResult<Unit, DomainError>.Bad(
                DomainError.Business("sale.insufficient_payment", $"Faltan RD$ {(TotalAmount - TotalPaidAmount):N2} por pagar."));
        }

        var itemEventLines = _items.Select(i => new SaleItemEventLine(
            i.ProductId,
            i.ProductName,
            i.QuantityValue,
            i.UnitPriceAmount,
            i.LineTotalAmount,
            i.LineItbisAmount)).ToList();

        var paymentEventLines = _payments.Select(p => new SalePaymentEventLine(
            p.PaymentMethodId,
            p.AmountValue,
            p.Reference,
            p.CustomerId)).ToList();

        _domainEvents.Add(new SaleCompletedDomainEvent(
            Id,
            ShiftId,
            TenantId,
            TerminalId,
            ReceiptNumber,
            CashierName,
            SubtotalAmount,
            TotalItbisAmount,
            TotalAmount,
            TotalPaidAmount,
            ChangeDueAmount,
            itemEventLines.AsReadOnly(),
            paymentEventLines.AsReadOnly(),
            DateTime.UtcNow));

        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public OperationResult<Unit, DomainError> Void(string reason)
    {
        if (StatusId == SaleStatus.Voided.Id)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("sale.already_voided"));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("sale.void_reason_required"));
        }

        if (reason.Length > 200)
        {
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("sale.void_reason_too_long"));
        }

        StatusId = SaleStatus.Voided.Id;
        VoidedAt = DateTime.UtcNow;
        VoidReason = reason.Trim();

        _domainEvents.Add(new SaleVoidedDomainEvent(
            Id,
            ShiftId,
            TenantId,
            TerminalId,
            ReceiptNumber,
            VoidReason,
            DateTime.UtcNow));

        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void RecalculateTotals()
    {
        SubtotalAmount = _items.Sum(i => i.LineSubtotalAmount);
        TotalItbisAmount = _items.Sum(i => i.LineItbisAmount);
        TotalAmount = _items.Sum(i => i.LineTotalAmount);

        if (_payments.Count > 0)
        {
            TotalPaidAmount = _payments.Sum(p => p.AmountValue);
            ChangeDueAmount = Math.Max(0m, TotalPaidAmount - TotalAmount);
        }
    }
}
