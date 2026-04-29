using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Fiscal;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Fiscal;

public class FiscalSequenceRepository(TuColmadoDbContext dbContext)
    : GenericRepository<FiscalSequence>(dbContext), IFiscalSequenceRepository
{
    public async Task<FiscalSequence?> GetActiveByPrefixAsync(
        TenantIdentifier tenantId,
        string prefix,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.FiscalSequences
            .Where(fs =>
                fs.TenantId.Value == tenantId.Value &&
                fs.Prefix == prefix &&
                fs.IsActive &&
                fs.ValidUntil > DateTime.UtcNow &&
                fs.CurrentSequence <= fs.EndSequence)
            .OrderBy(fs => fs.ValidUntil)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
