namespace TuColmadoRD.Core.Application.DTOs.Sync;

public record SaleCreatedPayload
{
    public Guid SaleId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime Date { get; init; }
    public List<SaleLineItemPayload> Items { get; init; } = new();
}

public record SaleLineItemPayload(Guid ProductId, decimal Quantity, decimal UnitPrice);
