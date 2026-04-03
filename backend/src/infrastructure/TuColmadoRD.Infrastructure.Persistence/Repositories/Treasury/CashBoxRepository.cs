using TuColmadoRD.Core.Domain.Entities.Treasury;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Treasury;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Treasury;

public class CashBoxRepository(TuColmadoDbContext dbContext) : GenericRepository<CashBox>(dbContext), ICashBoxRepository
{
}
