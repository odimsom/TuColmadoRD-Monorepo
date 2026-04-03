using TuColmadoRD.Core.Application.DTOs.Security;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Core.Application.Interfaces.Security;

public interface ILicenseVerifier
{
    Task<OperationResult<LicenseStatus, SubscriptionError>> VerifyAsync();
}
