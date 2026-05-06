using System.Linq;
using TuColmadoRD.Core.Domain.Entities.Logistics;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Logistics;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Logistics;

public class DeliveryOrderRepository(TuColmadoDbContext dbContext) : GenericRepository<DeliveryOrder>(dbContext), IDeliveryOrderRepository
{
    private readonly TuColmadoDbContext _context = dbContext;

    public IQueryable<DeliveryOrder> GetQueryable()
    {
        return _context.Set<DeliveryOrder>().AsQueryable();
    }
}
