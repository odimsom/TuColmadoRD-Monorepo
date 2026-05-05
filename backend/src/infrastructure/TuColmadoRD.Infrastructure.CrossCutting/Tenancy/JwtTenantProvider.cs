using Microsoft.AspNetCore.Http;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Infrastructure.CrossCutting.Tenancy;

public sealed class JwtTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TenantIdentifier TenantId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?
                .User.FindFirst("tenant_id")?.Value;

            if (Guid.TryParse(raw, out var id) && id != Guid.Empty)
            {
                var result = TenantIdentifier.Validate(id);
                if (result.IsGood && result.TryGetResult(out var tid))
                    return tid!;
            }

            return TenantIdentifier.Empty;
        }
    }

    public Guid TerminalId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?
                .User.FindFirst("terminal_id")?.Value;

            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsPaired =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
