using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;

public class SaleDetailRepository(TuColmadoDbContext dbContext) : GenericRepository<SaleDetail>(dbContext), ISaleDetailRepository
{
}
