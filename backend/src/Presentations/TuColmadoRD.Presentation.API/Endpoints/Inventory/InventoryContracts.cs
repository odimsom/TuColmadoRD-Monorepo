namespace TuColmadoRD.Presentation.API.Endpoints.Inventory;

public sealed record CreateProductRequest(
    string Name,
    Guid CategoryId,
    decimal ItbisRate);

public sealed record UpdatePriceRequest(decimal NewCostPrice, decimal NewSalePrice);

public sealed record AdjustStockRequest(decimal Delta, string Reason);

public sealed record CreatedProductResponse(Guid ProductId);

public sealed record ApiErrorResponse(string Error, string Message, int StatusCode);

public sealed record CreateCategoryRequest(string Name);

// Presentations
public sealed record UpdatePresentationPriceRequest(decimal SalePrice, decimal CostPrice);

public sealed record AddPresentationRequest(
    string DisplayName,
    int PresentationType,
    int SellMode,
    int MeasureUnit,
    decimal SalePrice,
    decimal CostPrice,
    string? Brand,
    decimal? NominalCapacity);

// Stock entries
public sealed record StockEntryLineRequest(
    Guid PresentationId,
    int ContainerCount,
    int UnitsPerContainer,
    decimal NominalSizePerUnit,
    decimal CostPerUnit);

public sealed record ConfirmStockEntryRequest(
    DateTime PurchasedAt,
    string? SupplierName,
    string? Notes,
    Guid? FundId,
    string? FundExpenseJustification,
    List<StockEntryLineRequest> Lines);

// Container operations
public sealed record OpenContainerRequest(decimal? ActualCapacity);

public sealed record DrawFromContainerRequest(decimal Amount, bool AllowOverDraw = false);

public sealed record SetActiveContainerRequest(Guid ContainerId);

// Monetary fund
public sealed record CreateFundRequest(string Name, decimal InitialDeposit);

public sealed record FundDepositRequest(decimal Amount, string Description);

public sealed record FundExpenseRequest(
    decimal Amount,
    int Category,
    string Description,
    string? JustificationNote,
    Guid? ReferenceId);
