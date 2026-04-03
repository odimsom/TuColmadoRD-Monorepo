using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Errors;

public sealed record SyncError(string Code, string Message) : DomainError(Code, Message);
