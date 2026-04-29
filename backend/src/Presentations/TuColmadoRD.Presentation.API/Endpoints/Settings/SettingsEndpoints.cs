using MediatR;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Settings;

/// <summary>
/// Minimal API endpoints for tenant settings (business profile, fiscal data).
/// </summary>
public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/settings")
            .WithTags("Settings")
            .RequireAuthorization();

        group.MapGet("/profile", GetProfile)
            .WithName("GetTenantProfile")
            .WithOpenApi();

        group.MapPut("/profile", UpsertProfile)
            .WithName("UpsertTenantProfile")
            .WithOpenApi();

        return app;
    }

    /// <summary>
    /// Returns the tenant's business profile (for DGI-compliant receipts).
    /// Returns 200 with null body if the profile has never been configured.
    /// </summary>
    private static async Task<IResult> GetProfile(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetTenantProfileQuery(), ct);
        if (!result.TryGetResult(out var dto))
            return result.Error.MapDomainError();

        // Return 200 OK whether dto is null or populated — let the UI decide what to show
        return TypedResults.Ok(dto);
    }

    /// <summary>
    /// Creates or updates the tenant's business profile.
    /// </summary>
    private static async Task<IResult> UpsertProfile(
        UpsertTenantProfileRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new UpsertTenantProfileCommand(
            request.BusinessName,
            request.Rnc,
            request.BusinessAddress,
            request.Phone,
            request.Email);

        var result = await mediator.Send(command, ct);
        if (!result.IsGood)
            return result.Error.MapDomainError();

        return TypedResults.NoContent();
    }
}
