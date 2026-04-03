using TuColmadoRD.Core.Domain.Entities.Purchasing;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Purchasing;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Purchasing;

public class PurchaseDetailRepository(TuColmadoDbContext dbContext) : GenericRepository<PurchaseDetail>(dbContext), IPurchaseDetailRepository
{
}
