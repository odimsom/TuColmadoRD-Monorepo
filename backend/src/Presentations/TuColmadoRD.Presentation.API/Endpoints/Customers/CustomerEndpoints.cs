using MediatR;
using TuColmadoRD.Core.Application.Customers.Commands;
using TuColmadoRD.Core.Application.Customers.Queries;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Customers;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/customers")
            .WithTags("Customers")
            .RequireAuthorization();

        group.MapPost(string.Empty, CreateCustomer)
            .WithName("CreateCustomer");

        group.MapGet("/{id:guid}", GetCustomerById)
            .WithName("GetCustomerById");

        group.MapPost("/{id:guid}/payments", RegisterPayment)
            .WithName("RegisterPayment");

        group.MapGet(string.Empty, GetCustomers)
            .WithName("GetCustomers");

        group.MapGet("/{id:guid}/statement", GetCustomerStatement)
            .WithName("GetCustomerStatement");

        return app;
    }

    private static async Task<IResult> CreateCustomer(
        CreateCustomerRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new CreateCustomerCommand(
            request.FullName,
            request.DocumentId,
            request.Phone,
            request.Address is null
                ? null
                : new CreateCustomerAddressRequest(
                    request.Address.Province,
                    request.Address.Sector,
                    request.Address.Street,
                    request.Address.Reference,
                    request.Address.HouseNumber,
                    request.Address.Latitude,
                    request.Address.Longitude),
            request.CreditLimit);

        var result = await mediator.Send(command, ct);
        if (!result.TryGetResult(out var created) || created is null)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Created(
            $"/api/v1/customers/{created.CustomerId}",
            new CreateCustomerResponse(
                created.CustomerId,
                created.AccountId,
                created.Balance,
                created.CreditLimit,
                created.IsActive,
                created.CreatedAt));
    }

    private static async Task<IResult> GetCustomerById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(id), ct);
        if (!result.TryGetResult(out var customer) || customer is null)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.Ok(customer);
    }

    private static async Task<IResult> RegisterPayment(
        Guid id,
        RegisterPaymentRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new RegisterAccountPaymentCommand(
            id,
            request.Amount,
            request.PaymentMethodId,
            request.Concept);

        var result = await mediator.Send(command, ct);
        if (!result.IsGood)
        {
            return result.Error.MapDomainError();
        }

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetCustomers(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new TuColmadoRD.Core.Application.Customers.Queries.GetCustomersWithBalanceQuery(), ct);
        if (!result.IsGood)
        {
            return result.Error.MapDomainError();
        }

        if (!result.TryGetResult(out var dtos)) return TypedResults.Ok();
        return TypedResults.Ok(dtos);
    }

    private static async Task<IResult> GetCustomerStatement(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new TuColmadoRD.Core.Application.Customers.Queries.GetCustomerStatementQuery(id), ct);
        if (!result.IsGood)
        {
            return result.Error.MapDomainError();
        }

        if (!result.TryGetResult(out var dtos)) return TypedResults.Ok();
        return TypedResults.Ok(dtos);
    }
}
