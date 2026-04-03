using TuColmadoRD.Core.Domain.Entities.Purchasing;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Purchasing;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Purchasing;

public class PurchaseOrderRepository(TuColmadoDbContext dbContext) : GenericRepository<PurchaseOrder>(dbContext), IPurchaseOrderRepository
{
}
