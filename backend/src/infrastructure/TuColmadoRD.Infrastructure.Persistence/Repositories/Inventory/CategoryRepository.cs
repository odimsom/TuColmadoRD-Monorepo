using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public class CategoryRepository(TuColmadoDbContext dbContext) : GenericRepository<Category>(dbContext), ICategoryRepository
{
}
