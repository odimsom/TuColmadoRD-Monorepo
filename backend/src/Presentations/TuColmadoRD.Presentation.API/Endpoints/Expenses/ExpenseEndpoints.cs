using MediatR;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Expenses;

public static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/expenses")
            .WithTags("Expenses")
            .RequireAuthorization();

        group.MapPost(string.Empty, RegisterExpense)
            .WithName("RegisterExpense");

        return app;
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
