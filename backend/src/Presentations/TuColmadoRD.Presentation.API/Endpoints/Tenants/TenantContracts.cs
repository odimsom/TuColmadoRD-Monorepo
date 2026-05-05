namespace TuColmadoRD.Presentation.API.Endpoints.Tenants;

/// <summary>
/// Request body for POST /api/tenants (tenant notification from Auth service).
/// This endpoint receives notifications when new tenants are created in the Auth service.
/// </summary>
public sealed record CreateTenantNotificationRequest(
    string TenantId,
    string Name,
    string OwnerEmail);

/// <summary>
/// Response DTO for tenant notification acknowledgment.
/// </summary>
public sealed record TenantNotificationResponse(
    string TenantId,
    string Status,
    DateTime ReceivedAt);
