using TuColmadoRD.Core.Domain.Entities.Fiscal;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Fiscal;

public class TaxRepository(TuColmadoDbContext dbContext) : GenericRepository<Tax>(dbContext), ITaxRepository
{
}
