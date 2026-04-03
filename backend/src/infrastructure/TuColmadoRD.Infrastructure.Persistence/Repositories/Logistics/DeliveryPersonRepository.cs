using TuColmadoRD.Core.Domain.Entities.Logistics;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Logistics;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Logistics;

public class DeliveryPersonRepository(TuColmadoDbContext dbContext) : GenericRepository<DeliveryPerson>(dbContext), IDeliveryPersonRepository
{
}
