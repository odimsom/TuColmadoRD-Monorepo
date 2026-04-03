using TuColmadoRD.Core.Application.DTOs.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Core.Application.Interfaces.Tenancy;

public interface IDevicePairingService
{
    Task<OperationResult<DeviceIdentity, DevicePairingError>> PairAsync(
        string email,
        string password,
        string deviceName,
        CancellationToken cancellationToken = default);
}
