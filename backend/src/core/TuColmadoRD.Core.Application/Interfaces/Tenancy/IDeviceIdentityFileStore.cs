using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Core.Application.Interfaces.Tenancy;

public interface IDeviceIdentityFileStore
{
    bool Exists(string path);
    OperationResult<byte[], DevicePairingError> ReadAllBytes(string path);
    OperationResult<Unit, DevicePairingError> WriteAllBytes(string path, byte[] content);
}
