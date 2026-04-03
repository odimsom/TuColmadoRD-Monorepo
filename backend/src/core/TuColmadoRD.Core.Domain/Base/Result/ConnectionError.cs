using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Base.Result
{
    public sealed record ConnectionError(string Message)
        : DomainError("CONN_001", Message)
    {
        public static readonly ConnectionError Offline =
            new("No hay conexión a internet disponible.");

        public static readonly ConnectionError Timeout =
            new("La verificación de conexión superó el tiempo límite.");
    }
}
