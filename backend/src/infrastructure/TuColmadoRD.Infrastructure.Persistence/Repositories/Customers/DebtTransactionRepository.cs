using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Customers;

public class DebtTransactionRepository(TuColmadoDbContext dbContext) : GenericRepository<DebtTransaction>(dbContext), IDebtTransactionRepository
{
}
