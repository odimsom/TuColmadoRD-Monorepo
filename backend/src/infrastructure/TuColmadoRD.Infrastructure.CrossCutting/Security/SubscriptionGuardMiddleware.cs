using Microsoft.AspNetCore.Http;
using System.Text.Json;
using TuColmadoRD.Core.Application.Interfaces.Security;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Infrastructure.CrossCutting.Security;

public class SubscriptionGuardMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] WhitelistedPaths = new[]
    {
        "/api/device/pair",
        "/api/device/renew-license",
        "/api/device/status",
        "/health",
        "/swagger"
    };

    public SubscriptionGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILicenseVerifier licenseVerifier)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        if (WhitelistedPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

        var verifyResult = await licenseVerifier.VerifyAsync();
        
        if (!verifyResult.IsGood)
        {
            verifyResult.TryGetError(out var error);
            await WriteErrorResponse(context, error!);
            return;
        }

        verifyResult.TryGetResult(out var status);
        if (status != null && !status.IsValid)
        {
            var fallbackError = new SubscriptionError("verfication_failed", status.FailureReason ?? "Unknown validation failure");
            await WriteErrorResponse(context, fallbackError);
            return;
        }

        await _next(context);
    }

    private static async Task WriteErrorResponse(HttpContext context, SubscriptionError error)
    {
        context.Response.StatusCode = 402;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = error.Code,
            message = error.Message,
            validUntil = (DateTime?)null
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
