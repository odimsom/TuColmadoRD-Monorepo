using MediatR;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Core.Application.Sales.Queries;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Expenses;

public static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/expenses")
            .WithTags("Expenses")
            .RequireAuthorization();

        group.MapGet(string.Empty, GetExpenses)
            .WithName("GetExpenses");

        group.MapPost(string.Empty, RegisterExpense)
            .WithName("RegisterExpense");

        return app;
    }

    private static async Task<IResult> GetExpenses(
        IMediator mediator,
        CancellationToken ct,
        int page = 1,
        int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await mediator.Send(new GetExpensesQuery(page, pageSize), ct);
        if (!result.TryGetResult(out var items))
        {
            return result.Error.MapDomainError();
        }
        return TypedResults.Ok(items);
    }

    private static async Task<IResult> RegisterExpense(
        RegisterExpenseRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new RegisterExpenseCommand(
            request.Amount,
            request.Category,
            request.Description);

        var result = await mediator.Send(command, ct);
        if (!result.IsGood)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.NoContent();
    }
}
