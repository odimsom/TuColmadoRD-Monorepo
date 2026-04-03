namespace TuColmadoRD.Core.Application.DTOs.Tenancy;

public sealed record DeviceIdentity(
    Guid TenantId,
    Guid TerminalId,
    string PublicLicenseKey,
    DateTimeOffset PairedAt,
    string? LicenseToken = null
);
