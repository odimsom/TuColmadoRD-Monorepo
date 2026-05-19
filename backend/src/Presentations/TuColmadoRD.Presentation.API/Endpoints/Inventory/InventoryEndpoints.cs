using MediatR;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Inventory;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        // Products
        group.MapPost("/products", CreateProduct).WithName("CreateProduct").WithOpenApi();
        group.MapPost("/products/seed-defaults", SeedDefaultProducts).WithName("SeedDefaultProducts").WithOpenApi();
        group.MapDelete("/products/{id:guid}", DeactivateProduct).WithName("DeactivateProduct").WithOpenApi();
        group.MapGet("/products/{id:guid}", GetProductById).WithName("GetProductById").WithOpenApi();
        group.MapGet("/products", GetProductsPaged).WithName("GetProductsPaged").WithOpenApi();
        group.MapGet("/catalog", GetCatalog).WithName("GetCatalog").WithOpenApi();

        // Legacy stubbed endpoints (return deprecation error via handler)
        group.MapPut("/products/{id:guid}/price", UpdatePrice).WithName("UpdateProductPrice").WithOpenApi();
        group.MapPost("/products/{id:guid}/stock/adjust", AdjustStock).WithName("AdjustStock").WithOpenApi();
        group.MapGet("/products/low-stock", GetLowStock).WithName("GetLowStockProducts").WithOpenApi();

        // Presentations
        group.MapPost("/products/{id:guid}/presentations", AddPresentation).WithName("AddPresentation").WithOpenApi();
        group.MapGet("/products/{id:guid}/presentations", GetPresentationsByProduct).WithName("GetPresentationsByProduct").WithOpenApi();
        group.MapDelete("/presentations/{id:guid}", DeactivatePresentation).WithName("DeactivatePresentation").WithOpenApi();
        group.MapPut("/presentations/{id:guid}/price", UpdatePresentationPrice).WithName("UpdatePresentationPrice").WithOpenApi();

        // Stock entries
        group.MapPost("/stock-entries", ConfirmStockEntry).WithName("ConfirmStockEntry").WithOpenApi();
        group.MapGet("/stock-entries", GetStockEntries).WithName("GetStockEntries").WithOpenApi();

        // Container operations
        group.MapGet("/presentations/{id:guid}/containers", GetContainersByPresentation).WithName("GetContainersByPresentation").WithOpenApi();
        group.MapPost("/containers/{id:guid}/open", OpenContainer).WithName("OpenContainer").WithOpenApi();
        group.MapPost("/containers/{id:guid}/draw", DrawFromContainer).WithName("DrawFromContainer").WithOpenApi();
        group.MapPost("/containers/{id:guid}/empty", MarkContainerEmpty).WithName("MarkContainerEmpty").WithOpenApi();
        group.MapPut("/presentations/{id:guid}/active-container", SetActiveContainer).WithName("SetActiveContainer").WithOpenApi();

        // Monetary fund
        group.MapGet("/funds", GetFunds).WithName("GetFunds").WithOpenApi();
        group.MapPost("/funds", CreateFund).WithName("CreateFund").WithOpenApi();
        group.MapGet("/funds/{id:guid}", GetFund).WithName("GetFund").WithOpenApi();
        group.MapPost("/funds/{id:guid}/deposit", FundDeposit).WithName("FundDeposit").WithOpenApi();
        group.MapPost("/funds/{id:guid}/expense", FundExpense).WithName("FundExpense").WithOpenApi();
        group.MapGet("/funds/{id:guid}/transactions", GetFundTransactions).WithName("GetFundTransactions").WithOpenApi();

        // Categories
        group.MapGet("/categories", GetCategories).WithName("GetCategories").WithOpenApi();
        group.MapPost("/categories", CreateCategory).WithName("CreateCategory").WithOpenApi();
        group.MapPost("/categories/seed-defaults", SeedDefaultCategories).WithName("SeedDefaultCategories").WithOpenApi();
        group.MapDelete("/categories/{id:guid}", DeactivateCategory).WithName("DeactivateCategory").WithOpenApi();

        return app;
    }

    // ── Products ─────────────────────────────────────────────────────────────

    private static async Task<IResult> SeedDefaultProducts(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SeedDefaultProductsCommand(), ct);
        if (!result.TryGetResult(out var count))
            return result.Error.MapDomainError();

        return TypedResults.Ok(new { message = "Productos sembrados correctamente", count });
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request, IMediator mediator, CancellationToken ct)
    {
        var command = new CreateProductCommand(request.Name, request.CategoryId, request.ItbisRate);
        var result = await mediator.Send(command, ct);
        if (!result.TryGetResult(out var productId))
            return result.Error.MapDomainError();

        return TypedResults.Created($"/api/v1/inventory/products/{productId}", new CreatedProductResponse(productId));
    }

    private static async Task<IResult> UpdatePrice(
        Guid id, UpdatePriceRequest request, IMediator mediator, CancellationToken ct)
    {
        var command = new UpdateProductPriceCommand(id, request.NewCostPrice, request.NewSalePrice);
        var result = await mediator.Send(command, ct);
        return result.IsGood ? TypedResults.Ok(new { }) : result.Error.MapDomainError();
    }

    private static async Task<IResult> AdjustStock(
        Guid id, AdjustStockRequest request, IMediator mediator, CancellationToken ct)
    {
        var command = new AdjustStockCommand(id, request.Delta, request.Reason);
        var result = await mediator.Send(command, ct);
        return result.IsGood ? TypedResults.Ok(new { }) : result.Error.MapDomainError();
    }

    private static async Task<IResult> DeactivateProduct(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateProductCommand(id), ct);
        return result.IsGood ? TypedResults.NoContent() : result.Error.MapDomainError();
    }

    private static async Task<IResult> GetProductById(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        if (!result.TryGetResult(out var dto))
            return result.Error.MapDomainError();

        return TypedResults.Ok(dto);
    }

    private static async Task<IResult> GetProductsPaged(
        IMediator mediator,
        int page = 1,
        int pageSize = 20,
        string? nameFilter = null,
        Guid? categoryId = null,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = new GetProductsPagedQuery(page, pageSize, nameFilter, categoryId, includeInactive);
        var result = await mediator.Send(query, ct);
        if (!result.TryGetResult(out var dto))
            return result.Error.MapDomainError();

        return TypedResults.Ok(dto);
    }

    private static async Task<IResult> GetCatalog(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCatalogQuery(), ct);
        if (!result.TryGetResult(out var dtos))
            return result.Error.MapDomainError();

        return TypedResults.Ok(dtos);
    }

    private static async Task<IResult> GetLowStock(
        IMediator mediator, int threshold = 5, CancellationToken ct = default)
    {
        threshold = Math.Clamp(threshold, 0, 100);
        var result = await mediator.Send(new GetLowStockQuery(threshold), ct);
        if (!result.TryGetResult(out var response))
            return result.Error.MapDomainError();

        return TypedResults.Ok(response);
    }

    // ── Presentations ─────────────────────────────────────────────────────────

    private static async Task<IResult> AddPresentation(
        Guid id, AddPresentationRequest request, IMediator mediator, CancellationToken ct)
    {
        var command = new AddProductPresentationCommand(
            id,
            request.DisplayName,
            request.PresentationType,
            request.SellMode,
            request.MeasureUnit,
            request.SalePrice,
            request.CostPrice,
            request.Brand,
            request.NominalCapacity);

        var result = await mediator.Send(command, ct);
        if (!result.TryGetResult(out var presentationId))
            return result.Error.MapDomainError();

        return TypedResults.Created($"/api/v1/inventory/presentations/{presentationId}", new { presentationId });
    }

    private static async Task<IResult> DeactivatePresentation(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivatePresentationCommand(id), ct);
        return result.IsGood ? TypedResults.NoContent() : result.Error.MapDomainError();
    }

    private static async Task<IResult> UpdatePresentationPrice(
        Guid id, UpdatePresentationPriceRequest request, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdatePresentationPriceCommand(id, request.SalePrice, request.CostPrice), ct);
        return result.IsGood ? TypedResults.Ok(new { }) : result.Error.MapDomainError();
    }

    private static async Task<IResult> GetPresentationsByProduct(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPresentationsByProductQuery(id), ct);
        if (!result.TryGetResult(out var dtos))
            return result.Error.MapDomainError();

        return TypedResults.Ok(dtos);
    }

    // ── Stock Entries ─────────────────────────────────────────────────────────

    private static async Task<IResult> ConfirmStockEntry(
        ConfirmStockEntryRequest request, IMediator mediator, CancellationToken ct)
    {
        var lines = request.Lines
            .Select(l => new StockEntryLineDto(
                l.PresentationId, l.ContainerCount, l.UnitsPerContainer,
                l.NominalSizePerUnit, l.CostPerUnit))
            .ToList();

        var command = new ConfirmStockEntryCommand(
            request.PurchasedAt,
            request.SupplierName,
            request.Notes,
            request.FundId,
            request.FundExpenseJustification,
            lines);

        var result = await mediator.Send(command, ct);
        if (!result.TryGetResult(out var entryId))
            return result.Error.MapDomainError();

        return TypedResults.Created($"/api/v1/inventory/stock-entries/{entryId}", new { entryId });
    }

    private static async Task<IResult> GetStockEntries(
        IMediator mediator, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetStockEntriesQuery(page, pageSize), ct);
        if (!result.TryGetResult(out var paged))
            return result.Error.MapDomainError();

        return TypedResults.Ok(paged);
    }

    // ── Containers ────────────────────────────────────────────────────────────

    private static async Task<IResult> GetContainersByPresentation(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetContainersByPresentationQuery(id), ct);
        if (!result.TryGetResult(out var dtos))
            return result.Error.MapDomainError();

        return TypedResults.Ok(dtos);
    }

    private static async Task<IResult> OpenContainer(
        Guid id, OpenContainerRequest request, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new OpenContainerCommand(id, request.ActualCapacity), ct);
        return result.IsGood ? TypedResults.Ok(new { }) : result.Error.MapDomainError();
    }

    private static async Task<IResult> DrawFromContainer(
        Guid id, DrawFromContainerRequest request, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DrawFromContainerCommand(id, request.Amount, request.AllowOverDraw), ct);
        if (!result.TryGetResult(out var remaining))
            return result.Error.MapDomainError();

        return TypedResults.Ok(new { remaining });
    }

    private static async Task<IResult> MarkContainerEmpty(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new MarkContainerEmptyCommand(id), ct);
        return result.IsGood ? TypedResults.NoContent() : result.Error.MapDomainError();
    }

    private static async Task<IResult> SetActiveContainer(
        Guid id, SetActiveContainerRequest request, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetActiveContainerCommand(id, request.ContainerId), ct);
        return result.IsGood ? TypedResults.Ok(new { }) : result.Error.MapDomainError();
    }

    private static async Task<IResult> GetFunds(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetFundsQuery(), ct);
        if (!result.TryGetResult(out var funds))
            return result.Error.MapDomainError();

        return TypedResults.Ok(funds);
    }

    private static async Task<IResult> GetFundTransactions(
        Guid id, IMediator mediator, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetFundTransactionsQuery(id, page, pageSize), ct);
        if (!result.TryGetResult(out var paged))
            return result.Error.MapDomainError();

        return TypedResults.Ok(paged);
    }

    // ── Monetary Fund ─────────────────────────────────────────────────────────

    private static async Task<IResult> CreateFund(
        CreateFundRequest request, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateMonetaryFundCommand(request.Name, request.InitialDeposit), ct);
        if (!result.TryGetResult(out var fundId))
            return result.Error.MapDomainError();

        return TypedResults.Created($"/api/v1/inventory/funds/{fundId}", new { fundId });
    }

    private static async Task<IResult> GetFund(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMonetaryFundQuery(id), ct);
        if (!result.TryGetResult(out var response))
            return result.Error.MapDomainError();

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> FundDeposit(
        Guid id, FundDepositRequest request, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new RecordFundDepositCommand(id, request.Amount, request.Description), ct);
        if (!result.TryGetResult(out var transactionId))
            return result.Error.MapDomainError();

        return TypedResults.Ok(new { transactionId });
    }

    private static async Task<IResult> FundExpense(
        Guid id, FundExpenseRequest request, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new RecordFundExpenseCommand(
            id, request.Amount, request.Category,
            request.Description, request.JustificationNote, request.ReferenceId), ct);

        if (!result.TryGetResult(out var transactionId))
            return result.Error.MapDomainError();

        return TypedResults.Ok(new { transactionId });
    }

    // ── Categories ────────────────────────────────────────────────────────────

    private static async Task<IResult> GetCategories(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoriesQuery(), ct);
        if (!result.TryGetResult(out var categories))
            return result.Error.MapDomainError();

        return TypedResults.Ok(categories);
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryRequest request, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCategoryCommand(request.Name), ct);
        if (!result.TryGetResult(out var id))
            return result.Error.MapDomainError();

        return TypedResults.Created($"/api/v1/inventory/categories/{id}", new { id });
    }

    private static async Task<IResult> SeedDefaultCategories(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SeedDefaultCategoriesCommand(), ct);
        if (!result.TryGetResult(out var count))
            return result.Error.MapDomainError();

        return TypedResults.Ok(new { created = count });
    }

    private static async Task<IResult> DeactivateCategory(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateCategoryCommand(id), ct);
        return result.IsGood ? TypedResults.NoContent() : result.Error.MapDomainError();
    }
}
