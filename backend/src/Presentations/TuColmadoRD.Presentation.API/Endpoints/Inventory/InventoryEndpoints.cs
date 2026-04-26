using MediatR;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Inventory;

/// <summary>
/// Minimal API endpoints for inventory module with API versioning support.
/// All endpoints use /api/v1/ prefix for versioning.
/// </summary>
public static class InventoryEndpoints
{
    /// <summary>
    /// Maps inventory endpoint group with versioning.
    /// </summary>
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        group.MapPost("/products", CreateProduct)
            .WithName("CreateProduct")
            .WithOpenApi();

        group.MapPut("/products/{id:guid}/price", UpdatePrice)
            .WithName("UpdateProductPrice")
            .WithOpenApi();

        group.MapPost("/products/{id:guid}/stock/adjust", AdjustStock)
            .WithName("AdjustStock")
            .WithOpenApi();

        group.MapDelete("/products/{id:guid}", DeactivateProduct)
            .WithName("DeactivateProduct")
            .WithOpenApi();

        group.MapGet("/products/{id:guid}", GetProductById)
            .WithName("GetProductById")
            .WithOpenApi();

        group.MapGet("/products", GetProductsPaged)
            .WithName("GetProductsPaged")
            .WithOpenApi();

        group.MapGet("/catalog", GetCatalog)
            .WithName("GetCatalog")
            .WithOpenApi();

        group.MapGet("/products/low-stock", GetLowStock)
            .WithName("GetLowStockProducts")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.CategoryId,
            request.CostPrice,
            request.SalePrice,
            request.ItbisRate,
            request.UnitType);

        var result = await mediator.Send(command, ct);
        if (!result.TryGetResult(out var productId))
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Created($"/api/v1/inventory/products/{productId}", new CreatedProductResponse(productId));
    }

    private static async Task<IResult> UpdatePrice(
        Guid id,
        UpdatePriceRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new UpdateProductPriceCommand(id, request.NewCostPrice, request.NewSalePrice);
        var result = await mediator.Send(command, ct);
        if (!result.IsGood)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(new { });
    }

    private static async Task<IResult> AdjustStock(
        Guid id,
        AdjustStockRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new AdjustStockCommand(id, request.Delta, request.Reason);
        var result = await mediator.Send(command, ct);
        if (!result.IsGood)
        {
            return result.Error.MapDomainError();
        }

        var productResult = await mediator.Send(new GetProductByIdQuery(id), ct);
        if (!productResult.TryGetResult(out var product))
        {
            return productResult.Error.MapDomainError();
        }

        return TypedResults.Ok(new { newStockQuantity = product!.StockQuantity });
    }

    private static async Task<IResult> DeactivateProduct(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateProductCommand(id), ct);
        if (!result.IsGood)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetProductById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        if (!result.TryGetResult(out var dto))
        {
            return result.Error.MapDomainError();
        }

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
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(dto);
    }

    private static async Task<IResult> GetCatalog(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetCatalogQuery(), ct);
        if (!result.TryGetResult(out var dtos))
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(dtos);
    }

    private static async Task<IResult> GetLowStock(
        IMediator mediator,
        int threshold = 5,
        CancellationToken ct = default)
    {
        threshold = Math.Clamp(threshold, 0, 100);
        var result = await mediator.Send(new GetLowStockQuery(threshold), ct);
        if (!result.TryGetResult(out var response))
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(response);
    }
}
