using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Sales;

/// <summary>
/// Represents a line item in a sale. Child entity of Sale aggregate.
/// All financial computations are immutable after creation.
/// </summary>
public sealed class SaleItem
{
    /// <summary>
    /// Unique item identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to parent Sale.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Product being sold.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Snapshot of product name at sale time for historical accuracy.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of product sold (supports fractional for weight/volume).
    /// </summary>
    public decimal QuantityValue { get; set; }

    /// <summary>
    /// Unit price at sale time (snapshot of SalePrice).
    /// </summary>
    public decimal UnitPriceAmount { get; set; }

    /// <summary>
    /// Unit cost at sale time (for margin reporting).
    /// </summary>
    public decimal CostPriceAmount { get; set; }

    /// <summary>
    /// Tax rate applied to this item.
    /// </summary>
    public decimal ItbisRateValue { get; set; }

    /// <summary>
    /// Line subtotal before tax: UnitPrice × Quantity.
    /// </summary>
    public decimal LineSubtotalAmount { get; set; }

    /// <summary>
    /// Tax amount on this line: LineSubtotal × ItbisRate.
    /// </summary>
    public decimal LineItbisAmount { get; set; }

    /// <summary>
    /// Total for line including tax: LineSubtotal + LineItbis.
    /// </summary>
    public decimal LineTotalAmount { get; set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private SaleItem() { }

    /// <summary>
    /// Creates a new SaleItem with computed line totals.
    /// Called only by Sale.AddItem().
    /// </summary>
    public SaleItem(
        Guid productId,
        string productName,
        Money unitPrice,
        Money costPrice,
        Quantity quantity,
        TaxRate itbisRate)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty.", nameof(productName));

        ProductId = productId;
        ProductName = productName;
        QuantityValue = quantity.Value;
        UnitPriceAmount = unitPrice.Amount;
        CostPriceAmount = costPrice.Amount;
        ItbisRateValue = itbisRate.Rate;

        // Compute line totals
        LineSubtotalAmount = unitPrice.Amount * quantity.Value;
        LineItbisAmount = LineSubtotalAmount * itbisRate.Rate;
        LineTotalAmount = LineSubtotalAmount + LineItbisAmount;
    }

    /// <summary>
    /// Gets quantity as Quantity value object.
    /// </summary>
    public Quantity GetQuantity() => Quantity.Of(QuantityValue).Result;

    /// <summary>
    /// Gets unit price as Money value object.
    /// </summary>
    public Money GetUnitPrice() => Money.FromDecimal(UnitPriceAmount).Result;

    /// <summary>
    /// Gets cost price as Money value object.
    /// </summary>
    public Money GetCostPrice() => Money.FromDecimal(CostPriceAmount).Result;

    /// <summary>
    /// Gets line total as Money value object.
    /// </summary>
    public Money GetLineTotal() => Money.FromDecimal(LineTotalAmount).Result;

    /// <summary>
    /// Gets line subtotal as Money value object.
    /// </summary>
    public Money GetLineSubtotal() => Money.FromDecimal(LineSubtotalAmount).Result;

    /// <summary>
    /// Gets line ITBIS as Money value object.
    /// </summary>
    public Money GetLineItbis() => Money.FromDecimal(LineItbisAmount).Result;
}
