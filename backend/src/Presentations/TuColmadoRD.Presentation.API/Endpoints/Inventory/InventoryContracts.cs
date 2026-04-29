namespace TuColmadoRD.Presentation.API.Endpoints.Inventory;

/// <summary>
/// Request body for product creation.
/// </summary>
public sealed record CreateProductRequest(
    string Name,
    Guid CategoryId,
    decimal CostPrice,
    decimal SalePrice,
    decimal ItbisRate,
    int UnitType);

/// <summary>
/// Request body for price updates.
/// </summary>
public sealed record UpdatePriceRequest(decimal NewCostPrice, decimal NewSalePrice);

/// <summary>
/// Request body for stock adjustments.
/// </summary>
public sealed record AdjustStockRequest(decimal Delta, string Reason);

/// <summary>
/// Created product API response.
/// </summary>
public sealed record CreatedProductResponse(Guid ProductId);

/// <summary>
/// Standard API error payload.
/// </summary>
public sealed record ApiErrorResponse(string Error, string Message, int StatusCode);

/// <summary>
/// Request body for category creation.
/// </summary>
public sealed record CreateCategoryRequest(string Name);
