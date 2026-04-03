using MediatR;
using TuColmadoRD.Core.Application.Purchasing.Commands;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Purchasing;

public static class PurchasingEndpoints
{
    public static IEndpointRouteBuilder MapPurchasingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/purchases")
            .WithTags("Purchasing")
            .RequireAuthorization();

        group.MapPost(string.Empty, CreateAndCompletePurchase)
            .WithName("CreatePurchase")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> CreateAndCompletePurchase(
        CreatePurchaseApiRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var items = request.Items.Select(i => new PurchaseItemRequest(i.ProductId, i.Quantity, i.UnitCost)).ToList();
        
        var command = new CreateAndCompletePurchaseCommand(request.SupplierId, request.SupplierNcf, items);

        var result = await mediator.Send(command, ct);
        if (!result.TryGetResult(out var orderId))
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Created($"/api/v1/purchases/{orderId}", new { OrderId = orderId });
    }
}
