using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Core.Application.Interfaces.Security;

public interface ITimeGuard
{
    Task<OperationResult<DateTime, SubscriptionError>> GetLastKnownTimeAsync();
    Task<OperationResult<Unit, SubscriptionError>> AdvanceTimeAsync(DateTime newTime);
}
