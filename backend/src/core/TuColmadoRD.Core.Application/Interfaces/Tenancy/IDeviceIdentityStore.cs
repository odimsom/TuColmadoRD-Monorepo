using TuColmadoRD.Core.Application.DTOs.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Core.Application.Interfaces.Tenancy;

public interface IDeviceIdentityStore
{
    OperationResult<DeviceIdentity, DevicePairingError> Read();
    OperationResult<Unit, DevicePairingError> Persist(DeviceIdentity identity);
}
