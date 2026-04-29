namespace TuColmadoRD.Core.Domain.Entities.Sales;

/// <summary>
/// Represents a payment method and amount applied to a sale.
/// Child entity of Sale aggregate.
/// </summary>
public sealed class SalePayment
{
    /// <summary>
    /// Unique payment identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to parent Sale.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Payment method used (Cash, Card, Transfer, Credit).
    /// </summary>
    public int PaymentMethodId { get; set; }

    /// <summary>
    /// Amount paid using this method.
    /// </summary>
    public decimal AmountValue { get; set; }

    /// <summary>
    /// Reference information (last 4 of card, transfer reference, etc.).
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Customer linked to a credit payment.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// When payment was received (UTC).
    /// </summary>
    public DateTime ReceivedAt { get; set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private SalePayment() { }

    /// <summary>
    /// Creates a new SalePayment.
    /// Called only by Sale.AddPayment().
    /// </summary>
    public SalePayment(
        PaymentMethod method,
        decimal amount,
        string? reference,
        Guid? customerId)
    {
        PaymentMethodId = method.Id;
        AmountValue = amount;
        Reference = reference;
        CustomerId = customerId;
        ReceivedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Replaces the placeholder payment data with real settlement values.
    /// Called only by Sale.SettleDeliveryPayment().
    /// </summary>
    internal void Settle(PaymentMethod method, decimal amount, string? reference, Guid? customerId)
    {
        PaymentMethodId = method.Id;
        AmountValue = amount;
        Reference = reference;
        CustomerId = customerId;
        ReceivedAt = DateTime.UtcNow;
    }
}
