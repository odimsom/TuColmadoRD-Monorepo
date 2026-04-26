namespace TuColmadoRD.Presentation.API.Endpoints.Settings;

/// <summary>
/// Response DTO for GET /api/v1/settings/profile.
/// Null means the tenant hasn't configured their profile yet.
/// </summary>
public sealed record TenantProfileResponse(
    string BusinessName,
    string? Rnc,
    string BusinessAddress,
    string? Phone,
    string? Email);

/// <summary>
/// Request body for PUT /api/v1/settings/profile (upsert).
/// </summary>
public sealed record UpsertTenantProfileRequest(
    string BusinessName,
    string? Rnc,
    string BusinessAddress,
    string? Phone,
    string? Email);
