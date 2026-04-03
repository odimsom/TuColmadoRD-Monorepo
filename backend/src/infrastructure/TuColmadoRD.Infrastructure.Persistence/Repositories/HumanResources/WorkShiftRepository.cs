using TuColmadoRD.Core.Domain.Entities.HumanResources;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.HumanResources;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.HumanResources;

public class WorkShiftRepository(TuColmadoDbContext dbContext) : GenericRepository<WorkShift>(dbContext), IWorkShiftRepository
{
}
