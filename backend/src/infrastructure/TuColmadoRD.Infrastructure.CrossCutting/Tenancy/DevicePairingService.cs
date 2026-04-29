using System.Net;
using System.Net.Http.Json;
using TuColmadoRD.Core.Application.DTOs.Tenancy;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Infrastructure.CrossCutting.Tenancy;

public sealed class DevicePairingService : IDevicePairingService
{
    private readonly HttpClient _http;
    private readonly LocalDeviceTenantProvider _localProvider;

    public DevicePairingService(HttpClient http, LocalDeviceTenantProvider localProvider)
    {
        _http = http;
        _localProvider = localProvider;
    }

    public async Task<OperationResult<DeviceIdentity, DevicePairingError>> PairAsync(
        string email,
        string password,
        string deviceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "/api/auth/pair-device",
                new { email, password, deviceName },
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.AuthFailed);

            if (response.StatusCode == HttpStatusCode.Conflict)
                return OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.TerminalConflict);

            if (!response.IsSuccessStatusCode)
                return OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.AuthFailed);

            var payload = await response.Content.ReadFromJsonAsync<PairDeviceResponse>(
                cancellationToken: cancellationToken);

            if (payload is null)
                return OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.AuthFailed);

            var identity = new DeviceIdentity(
                payload.TenantId,
                payload.TerminalId,
                payload.PublicLicenseKey,
                DateTimeOffset.UtcNow);

            return _localProvider.Persist(identity).Match(
                onGood: _ => OperationResult<DeviceIdentity, DevicePairingError>.Good(identity),
                onBad: err => OperationResult<DeviceIdentity, DevicePairingError>.Bad(err!));
        }
        catch (HttpRequestException)
        {
            return OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.NoInternet);
        }
    }

    private sealed record PairDeviceResponse(
        Guid TenantId,
        Guid TerminalId,
        string PublicLicenseKey);
}
