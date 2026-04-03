namespace TuColmadoRD.Presentation.API.Endpoints.Purchasing;

public sealed record PurchaseItemApiRequest(Guid ProductId, decimal Quantity, decimal UnitCost);

public sealed record CreatePurchaseApiRequest(Guid SupplierId, string SupplierNcf, IReadOnlyList<PurchaseItemApiRequest> Items);
