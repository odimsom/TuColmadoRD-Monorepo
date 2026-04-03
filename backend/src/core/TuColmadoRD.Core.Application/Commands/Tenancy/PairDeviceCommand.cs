using MediatR;
using System.Net;
using System.Text.Json;
using TuColmadoRD.Core.Application.DTOs.Tenancy;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Core.Application.Commands.Tenancy;

public sealed record PairDeviceCommand(
    string Email,
    string Password,
    string DeviceName
) : IRequest<OperationResult<DeviceIdentity, DevicePairingError>>;

public sealed class PairDeviceCommandHandler
    : IRequestHandler<PairDeviceCommand, OperationResult<DeviceIdentity, DevicePairingError>>
{
    private readonly IDevicePairingService _pairingService;
    private readonly ITenantProvider _tenantProvider;

    public PairDeviceCommandHandler(
        IDevicePairingService pairingService,
        ITenantProvider tenantProvider)
    {
        _pairingService = pairingService;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<DeviceIdentity, DevicePairingError>> Handle(
        PairDeviceCommand request,
        CancellationToken cancellationToken)
    {
        if (_tenantProvider.IsPaired)
            return OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.AlreadyPaired);

        return await _pairingService.PairAsync(request.Email, request.Password, request.DeviceName, cancellationToken);
    }
}
