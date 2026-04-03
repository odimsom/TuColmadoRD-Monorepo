using TuColmadoRD.Core.Domain.Entities.Treasury;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Treasury;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Treasury;

public class CashDrawerRepository(TuColmadoDbContext dbContext) : GenericRepository<CashDrawer>(dbContext), ICashDrawerRepository
{
}
