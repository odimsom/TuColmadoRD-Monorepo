using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Errors;

public sealed record SubscriptionError(string Code, string Message) : DomainError(Code, Message)
{
    public static readonly SubscriptionError LicenseNotFound =
        new("license_not_found", "No se encontró un token de licencia en el dispositivo.");

    public static readonly SubscriptionError InvalidSignature =
        new("invalid_signature", "La firma del token de licencia es inválida (posible alteración).");

    public static readonly SubscriptionError ClockTamperDetected =
        new("clock_tamper_detected", "Se ha detectado una alteración en el reloj del sistema.");

    public static readonly SubscriptionError SubscriptionExpired =
        new("subscription_expired", "La suscripción a la plataforma ha expirado.");

    public static readonly SubscriptionError TerminalMismatch =
        new("terminal_mismatch", "El token de licencia pertenece a otra terminal.");
        
    public static readonly SubscriptionError VerificationFailed =
        new("verification_failed", "La validación criptográfica de la licencia falló.");
}
