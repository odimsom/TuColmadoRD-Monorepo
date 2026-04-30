using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Tenants;

/// <summary>
/// Minimal API endpoints for tenant management.
/// These endpoints handle notifications from the Auth service when new tenants are created.
/// </summary>
public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Tenants");

        // Internal endpoint for Auth service to notify of new tenants
        // Does not require authorization since it's inter-service communication
        group.MapPost("/tenants", NotifyNewTenant)
            .WithName("NotifyNewTenant")
            .WithOpenApi()
            .AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Receives notification from Auth service when a new tenant is created.
    /// This is an internal endpoint for service-to-service communication.
    /// </summary>
    private static async Task<IResult> NotifyNewTenant(
        CreateTenantNotificationRequest request,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation(
            "📢 Received new tenant notification - TenantId: {TenantId}, Name: {Name}, Owner: {OwnerEmail}",
            request.TenantId,
            request.Name,
            request.OwnerEmail);

        // TODO: Implement tenant initialization logic here
        // Examples:
        // - Create default settings for the tenant
        // - Initialize default catalog/products
        // - Set up roles and permissions
        // - Send welcome email

        var response = new TenantNotificationResponse(
            request.TenantId,
            "Acknowledged",
            DateTime.UtcNow);

        return TypedResults.Created($"/api/tenants/{request.TenantId}", response);
    }
}
