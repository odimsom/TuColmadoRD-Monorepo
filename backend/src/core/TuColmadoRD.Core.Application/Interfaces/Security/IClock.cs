namespace TuColmadoRD.Core.Application.Interfaces.Security;

public interface IClock
{
    DateTime UtcNow { get; }
}
