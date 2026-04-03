using System.Text.Json;
using Microsoft.AspNetCore.Http;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;

namespace TuColmadoRD.Infrastructure.CrossCutting.Tenancy;

public sealed class PairingGuardMiddleware
{
    private readonly RequestDelegate _next;

    public PairingGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        if (!tenantProvider.IsPaired && !IsPairingEndpoint(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new
            {
                error = "device_not_paired",
                message = "Este dispositivo no está vinculado. Realice el emparejamiento en POST /api/device/pair."
            });

            await context.Response.WriteAsync(body);
            return;
        }

        await _next(context);
    }

    private static bool IsPairingEndpoint(HttpRequest request) =>
        request.Method == HttpMethods.Post &&
        request.Path.StartsWithSegments("/api/device/pair", StringComparison.OrdinalIgnoreCase);
}
