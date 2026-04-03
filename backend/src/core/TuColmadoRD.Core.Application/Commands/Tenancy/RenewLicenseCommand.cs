using MediatR;
using TuColmadoRD.Core.Application.DTOs.Security;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Core.Application.Commands.Tenancy;

public record RenewLicenseCommand(string AdminToken) : IRequest<OperationResult<LicenseStatus, SubscriptionError>>;
