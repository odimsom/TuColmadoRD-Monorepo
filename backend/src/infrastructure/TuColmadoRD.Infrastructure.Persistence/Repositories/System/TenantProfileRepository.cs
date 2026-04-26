using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.System;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.System;

public class TenantProfileRepository(TuColmadoDbContext dbContext)
    : GenericRepository<TenantProfile>(dbContext), ITenantProfileRepository
{
    public async Task<TenantProfile?> GetByTenantAsync(
        TenantIdentifier tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(tp => tp.TenantId.Value == tenantId.Value, cancellationToken);
    }
}
