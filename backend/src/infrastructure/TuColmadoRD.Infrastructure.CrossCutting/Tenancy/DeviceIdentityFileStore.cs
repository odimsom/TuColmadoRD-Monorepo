using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Infrastructure.CrossCutting.Tenancy;

public sealed class DeviceIdentityFileStore : IDeviceIdentityFileStore
{
    public bool Exists(string path) => File.Exists(path);

    public OperationResult<byte[], DevicePairingError> ReadAllBytes(string path)
    {
        try
        {
            return OperationResult<byte[], DevicePairingError>.Good(File.ReadAllBytes(path));
        }
        catch
        {
            return OperationResult<byte[], DevicePairingError>.Bad(DevicePairingError.IoError);
        }
    }

    public OperationResult<Unit, DevicePairingError> WriteAllBytes(string path, byte[] content)
    {
        try
        {
            File.WriteAllBytes(path, content);
            return OperationResult<Unit, DevicePairingError>.Good(Unit.Value);
        }
        catch
        {
            return OperationResult<Unit, DevicePairingError>.Bad(DevicePairingError.IoError);
        }
    }
}
