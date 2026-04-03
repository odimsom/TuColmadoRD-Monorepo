namespace TuColmadoRD.Core.Application.DTOs.Security;

public record LicenseStatus(
    bool IsValid, 
    DateTime ValidUntil, 
    string? FailureReason
);
