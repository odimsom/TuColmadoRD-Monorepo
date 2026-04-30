using MediatR;
using TuColmadoRD.Core.Application.Modules.Logistics.Commands;
using TuColmadoRD.Core.Application.Modules.Logistics.Queries;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Logistics;

public static class DeliveryEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/logistics/delivery")
            .WithTags("Logistics-Delivery")
            .RequireAuthorization();

        group.MapGet("/pending", GetPendingOrders)
            .WithName("GetPendingDeliveryOrders")
            .WithOpenApi();

        group.MapPost("/{id:guid}/accept", AcceptOrder)
            .WithName("AcceptDeliveryOrder")
            .WithOpenApi();

        group.MapPost("/{id:guid}/complete", CompleteOrder)
            .WithName("CompleteDeliveryOrder")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetPendingOrders(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPendingDeliveryOrdersQuery(), ct);
        if (!result.IsGood) return result.Error.MapDomainError();
        return TypedResults.Ok(result.Result);
    }

    private static async Task<IResult> AcceptOrder(Guid id, IMediator mediator, CancellationToken ct)
    {
        // For now, deliveryPersonId is empty/system-assigned.
        var result = await mediator.Send(new AcceptDeliveryOrderCommand(id, Guid.Empty), ct);
        if (!result.IsGood) return result.Error.MapDomainError();
        return TypedResults.Ok(new { status = "InTransit" });
    }

    private static async Task<IResult> CompleteOrder(Guid id, CompleteDeliveryOrderRequest body, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CompleteDeliveryOrderCommand(id, body.Payments, body.ConfirmationCode, body.DriverLatitude, body.DriverLongitude), ct);
        if (!result.IsGood) return result.Error.MapDomainError();
        return TypedResults.Ok(new { status = "Delivered" });
    }
}

internal sealed record CompleteDeliveryOrderRequest(
    List<SalePaymentRequest> Payments,
    string ConfirmationCode,
    double? DriverLatitude,
    double? DriverLongitude);
