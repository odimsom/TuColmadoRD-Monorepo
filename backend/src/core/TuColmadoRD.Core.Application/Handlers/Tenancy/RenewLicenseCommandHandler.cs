using MediatR;
using System.Net.Http.Json;
using TuColmadoRD.Core.Application.Commands.Tenancy;
using TuColmadoRD.Core.Application.DTOs.Security;
using TuColmadoRD.Core.Application.DTOs.Tenancy;
using TuColmadoRD.Core.Application.Interfaces.Security;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Core.Application.Handlers.Tenancy;

public class RenewLicenseCommandHandler : IRequestHandler<RenewLicenseCommand, OperationResult<LicenseStatus, SubscriptionError>>
{
    private readonly HttpClient _httpClient;
    private readonly IDeviceIdentityStore _identityStore;
    private readonly ILicenseVerifier _licenseVerifier;

    public RenewLicenseCommandHandler(
        IDevicePairingService pairingService, // To resolve the base URL configured in that client
        IHttpClientFactory httpClientFactory,
        IDeviceIdentityStore identityStore,
        ILicenseVerifier licenseVerifier)
    {
        _httpClient = httpClientFactory.CreateClient("DevicePairingService");
        _identityStore = identityStore;
        _licenseVerifier = licenseVerifier;
    }

    public async Task<OperationResult<LicenseStatus, SubscriptionError>> Handle(RenewLicenseCommand request, CancellationToken cancellationToken)
    {
        var identityResult = _identityStore.Read();
        if (!identityResult.TryGetResult(out var identity))
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.LicenseNotFound);
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/renew-license");
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.AdminToken);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        }
        catch
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(new SubscriptionError("no_internet", "No hay conexión a internet disponible para renovar."));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(new SubscriptionError("auth_failed", "Token de administración inválido o expirado."));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(new SubscriptionError("renewal_rejected", "Tenant suspendido o inactivo. No se pudo renovar."));
        }

        var payload = await response.Content.ReadFromJsonAsync<RenewLicenseResponse>(cancellationToken: cancellationToken);
        if (payload == null || string.IsNullOrWhiteSpace(payload.LicenseToken))
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(new SubscriptionError("verification_failed", "Respuesta inválida del servidor de autenticación."));
        }

        var newIdentity = identity! with { LicenseToken = payload.LicenseToken };
        
        var persistResult = _identityStore.Persist(newIdentity);
        if (!persistResult.IsGood)
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(new SubscriptionError("io_error", "No se pudo escribir el token de licencia en el storage local."));
        }

        return await _licenseVerifier.VerifyAsync();
    }

    private record RenewLicenseResponse(string LicenseToken, string ValidUntil);
}
