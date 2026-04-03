using TuColmadoRD.Core.Domain.Entities.Audit;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Audit;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Audit;

public class AuditTrailRepository(TuColmadoDbContext dbContext) 
    : GenericRepository<AuditTrail>(dbContext), IAuditTrailRepository
{
}
