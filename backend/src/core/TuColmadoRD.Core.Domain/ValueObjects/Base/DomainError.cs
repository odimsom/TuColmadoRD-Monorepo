namespace TuColmadoRD.Core.Domain.ValueObjects.Base
{
    public abstract record DomainError(string Code, string Message)
    {
        public override string ToString() => $"[{Code}] {Message}";

        public static DomainError Validation(string code, string? message = null)
            => new GenericDomainError(code, message ?? code);

        public static DomainError Business(string code, string? message = null)
            => new GenericDomainError(code, message ?? code);

        public static DomainError NotFound(string code, string? message = null)
            => new GenericDomainError(code, message ?? code);

        private sealed record GenericDomainError(string InnerCode, string InnerMessage)
            : DomainError(InnerCode, InnerMessage);
    }

    public sealed record ValidationError(string Field, string Message)
        : DomainError("VAL_001", Message);

    public sealed record NotFoundError(string Entity, Guid Id)
        : DomainError("NF_001", $"{Entity} con id '{Id}' no fue encontrado.");
}
