using MediatR;
using TuColmadoRD.Core.Application.Sales.Shifts.Commands;
using TuColmadoRD.Core.Application.Sales.Shifts.Queries;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Sales.Shifts;

public static class ShiftEndpoints
{
    public static IEndpointRouteBuilder MapShiftEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/sales/shifts")
            .WithTags("Sales-Shifts")
            .RequireAuthorization();

        group.MapPost("/open", OpenShift)
            .WithName("OpenShift")
            .WithOpenApi();

        group.MapPost("/{id:guid}/close", CloseShift)
            .WithName("CloseShift")
            .WithOpenApi();

        group.MapGet("/current", GetCurrentShift)
            .WithName("GetCurrentShift")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetShiftById)
            .WithName("GetShiftById")
            .WithOpenApi();

        group.MapGet("", GetShiftsPaged)
            .WithName("GetShiftsPaged")
            .WithOpenApi();
        group.MapGet("/current/summary", GetCurrentShiftSummary)
            .WithName("GetCurrentShiftSummary")
            .WithOpenApi();
        return app;
    }

    private static async Task<IResult> OpenShift(
        OpenShiftRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new OpenShiftCommand(request.OpeningCashAmount, request.CashierName);
        var result = await mediator.Send(command, ct);
        
        if (!result.TryGetResult(out var shiftId))
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Created($"/api/v1/sales/shifts/{shiftId}", new OpenShiftResponse(shiftId));
    }

    private static async Task<IResult> CloseShift(
        Guid id,
        CloseShiftRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new CloseShiftCommand(id, request.ActualCashAmount, request.Notes);
        var result = await mediator.Send(command, ct);
        
        if (!result.TryGetResult(out var closeResult) || closeResult is null)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(new CloseShiftResponse(
            closeResult.ShiftId,
            closeResult.TotalSalesCount,
            closeResult.TotalSalesAmount,
            closeResult.ExpectedCashAmount,
            closeResult.ActualCashAmount,
            closeResult.CashDifference,
            closeResult.ClosedAt));
    }

    private static async Task<IResult> GetCurrentShift(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetCurrentShiftQuery(), ct);
        
        if (!result.TryGetResult(out var shiftDto) || shiftDto is null)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(shiftDto);
    }

    private static async Task<IResult> GetShiftById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetShiftByIdQuery(id), ct);
        
        if (!result.TryGetResult(out var shiftDto) || shiftDto is null)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(shiftDto);
    }

    private static async Task<IResult> GetShiftsPaged(
        IMediator mediator,
        int page = 1,
        int pageSize = 20,
        DateTime? from = null,
        DateTime? to = null,
        string status = "all",
        CancellationToken ct = default)
    {
        pageSize = Math.Min(Math.Max(pageSize, 1), 100);
        page = Math.Max(page, 1);

        var statusFilter = status.ToLowerInvariant() switch
        {
            "open" => ShiftStatusFilter.Open,
            "closed" => ShiftStatusFilter.Closed,
            _ => ShiftStatusFilter.All
        };

        var query = new GetShiftsPagedQuery(page, pageSize, from, to, statusFilter);
        var result = await mediator.Send(query, ct);
        
        if (!result.TryGetResult(out var paged) || paged is null)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(new PagedShiftResponse(
            paged.Items,
            paged.Page,
            paged.PageSize,
            paged.TotalCount,
            paged.TotalPages));
    }

    private static async Task<IResult> GetCurrentShiftSummary(
        IMediator mediator,
        TuColmadoRD.Core.Application.Interfaces.Tenancy.ITenantProvider tenantProvider,
        CancellationToken ct)
    {
        // Obtain actual terminal Id from the context or provider if available
        var terminalId = Guid.Empty; // using an empty or default depending on system implementation
        var query = new TuColmadoRD.Core.Application.Sales.Queries.GetCurrentShiftSummaryQuery(terminalId);
        
        var result = await mediator.Send(query, ct);
        if (!result.IsGood || !result.TryGetResult(out var summary))
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(summary);
    }
}
