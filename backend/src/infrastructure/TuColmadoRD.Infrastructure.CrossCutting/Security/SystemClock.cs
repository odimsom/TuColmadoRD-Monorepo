using TuColmadoRD.Core.Application.Interfaces.Security;

namespace TuColmadoRD.Infrastructure.CrossCutting.Security;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
