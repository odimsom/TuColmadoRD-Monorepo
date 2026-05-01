using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Presentation.API.Extensions;
using ApplicationSaleItemRequest = TuColmadoRD.Core.Application.Sales.Commands.SaleItemRequest;
using ApplicationSalePaymentRequest = TuColmadoRD.Core.Application.Sales.Commands.SalePaymentRequest;
using ApplicationDeliveryAddressRequest = TuColmadoRD.Core.Application.Sales.Commands.DeliveryAddressRequest;

namespace TuColmadoRD.Presentation.API.Endpoints.Sales;

public static class SalesEndpoints
{
    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/sales")
            .WithTags("Sales")
            .RequireAuthorization();

        group.MapPost(string.Empty, CreateSale)
            .WithName("CreateSale")
            .WithOpenApi();

        group.MapPost("/{saleId:guid}/void", VoidSale)
            .WithName("VoidSale")
            .WithOpenApi();

        group.MapGet("/{saleId:guid}", GetSaleById)
            .WithName("GetSaleById")
            .WithOpenApi();

        group.MapGet(string.Empty, GetSalesPaged)
            .WithName("GetSalesPaged")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> CreateSale(
        CreateSaleRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var commandItems = request.Items.Select(i => new ApplicationSaleItemRequest(i.ProductId, i.Quantity)).ToList();
        var commandPayments = request.Payments.Select(p => new ApplicationSalePaymentRequest(p.PaymentMethodId, p.Amount, p.Reference, p.CustomerId)).ToList();

        ApplicationDeliveryAddressRequest? deliveryAddress = null;
        if (request.DeliveryAddress is { } da)
            deliveryAddress = new ApplicationDeliveryAddressRequest(da.Province, da.Sector, da.Street, da.Reference, da.HouseNumber, da.Latitude, da.Longitude);

        var command = new CreateSaleCommand(commandItems, commandPayments, request.Notes, request.BuyerRnc, deliveryAddress);

        var result = await mediator.Send(command, ct);
        if (!result.TryGetResult(out var created) || created is null)
        {
            return result.Error.MapDomainError();
        }

        var response = new CreateSaleResponse(
            created.SaleId,
            created.ReceiptNumber,
            created.NcfNumber,
            created.Subtotal,
            created.TotalItbis,
            created.Total,
            created.TotalPaid,
            created.ChangeDue,
            created.Items.Select(i => new CreateSaleLineResponse(
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.LineItbis,
                i.LineTotal)).ToList());

        return TypedResults.Created($"/api/v1/sales/{created.SaleId}", response);
    }

    private static async Task<IResult> VoidSale(
        Guid saleId,
        VoidSaleRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new VoidSaleCommand(saleId, request.VoidReason);
        var result = await mediator.Send(command, ct);

        if (!result.IsGood)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(new { saleId, status = "voided" });
    }

    private static async Task<IResult> GetSaleById(
        Guid saleId,
        ISaleService saleService,
        ITenantProvider tenantProvider,
        CancellationToken ct)
    {
        var result = await saleService.GetSaleDetailAsync(saleId, tenantProvider.TenantId, ct);
        if (!result.TryGetResult(out var sale) || sale is null)
        {
            return result.Error.MapDomainError();
        }

        var response = new SaleDetailResponse(
            sale.Id,
            sale.ShiftId,
            sale.TerminalId,
            sale.ReceiptNumber,
            sale.CashierName,
            sale.StatusId,
            sale.SubtotalAmount,
            sale.TotalItbisAmount,
            sale.TotalAmount,
            sale.TotalPaidAmount,
            sale.ChangeDueAmount,
            sale.Notes,
            sale.CreatedAt,
            sale.VoidedAt,
            sale.VoidReason,
            sale.Items.Select(i => new SaleItemResponse(
                i.ProductId,
                i.ProductName,
                i.QuantityValue,
                i.UnitPriceAmount,
                i.CostPriceAmount,
                i.ItbisRateValue,
                i.LineSubtotalAmount,
                i.LineItbisAmount,
                i.LineTotalAmount)).ToList(),
            sale.Payments.Select(p => new SalePaymentResponse(
                p.PaymentMethodId,
                p.AmountValue,
                p.Reference,
                p.CustomerId,
                p.ReceivedAt)).ToList());

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> GetSalesPaged(
        ISaleService saleService,
        ITenantProvider tenantProvider,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = await saleService.GetPagedSalesAsync(tenantProvider.TenantId, page, pageSize, ct);
        if (!result.TryGetResult(out var paged) || paged is null)
        {
            return result.Error.MapDomainError();
        }

        var items = paged.Items
            .Select(s => new SaleSummaryResponse(
                s.Id,
                s.ReceiptNumber,
                s.StatusId,
                s.TotalAmount,
                s.TotalPaidAmount,
                s.CreatedAt,
                s.Items.Count))
            .ToList();

        var totalPages = (int)Math.Ceiling((double)paged.TotalCount / paged.PageSize);
        var response = new PagedSalesResponse(items, paged.PageNumber, paged.PageSize, paged.TotalCount, totalPages, paged.TotalRevenue);

        return TypedResults.Ok(response);
    }
}
