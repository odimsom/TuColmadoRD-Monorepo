using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.System;

public interface ITenantProfileRepository : IGenericRepository<TenantProfile>
{
    /// <summary>
    /// Returns the profile for a tenant, or null if not yet configured.
    /// </summary>
    Task<TenantProfile?> GetByTenantAsync(
        TenantIdentifier tenantId,
        CancellationToken cancellationToken = default);
}
