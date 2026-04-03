using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Errors;

public sealed record DevicePairingError(string Code, string Message) : DomainError(Code, Message)
{
    public static readonly DevicePairingError AlreadyPaired =
        new("device_already_paired", "Este dispositivo ya está vinculado a un tenant.");

    public static readonly DevicePairingError NoInternet =
        new("no_internet", "No hay conexión a internet disponible para completar el emparejamiento.");

    public static readonly DevicePairingError AuthFailed =
        new("auth_failed", "Credenciales inválidas. Verifique el email y la contraseña.");

    public static readonly DevicePairingError TerminalConflict =
        new("terminal_conflict", "Esta terminal ya está registrada bajo otro tenant.");

    public static readonly DevicePairingError IoError =
        new("io_error", "No se pudo escribir el archivo de identidad del dispositivo.");
}
