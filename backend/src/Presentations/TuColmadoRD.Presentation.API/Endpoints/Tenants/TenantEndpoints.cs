using Microsoft.EntityFrameworkCore;
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
        TuColmadoRD.Infrastructure.Persistence.Contexts.TuColmadoDbContext dbContext,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation(
            "📢 Received new tenant notification - TenantId: {TenantId}, Name: {Name}, Owner: {OwnerEmail}",
            request.TenantId,
            request.Name,
            request.OwnerEmail);

        if (Guid.TryParse(request.TenantId, out var tenantIdGuid))
        {
            var tenantId = TuColmadoRD.Core.Domain.ValueObjects.TenantIdentifier.Validate(tenantIdGuid).Result;
            
            // 1. Create Default Tenant Profile
            var existingProfile = await dbContext.Set<TuColmadoRD.Core.Domain.Entities.System.TenantProfile>()
                .AnyAsync(p => p.TenantId.Value == tenantIdGuid, ct);

            if (!existingProfile)
            {
                var profileResult = TuColmadoRD.Core.Domain.Entities.System.TenantProfile.Create(
                    tenantId,
                    request.Name,
                    "Dirección pendiente de configurar",
                    null,
                    request.OwnerEmail);

                if (profileResult.IsGood && profileResult.TryGetResult(out var profile))
                {
                    dbContext.Set<TuColmadoRD.Core.Domain.Entities.System.TenantProfile>().Add(profile!);
                    await dbContext.SaveChangesAsync(ct);
                    logger.LogInformation("✅ Default TenantProfile created for {TenantId}", request.TenantId);
                }
            }
        }

        var response = new TenantNotificationResponse(
            request.TenantId,
            "Acknowledged",
            DateTime.UtcNow);

        return TypedResults.Created($"/api/tenants/{request.TenantId}", response);
    }
}
