using System.Threading;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;

namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;

public interface IDebtTransactionRepository : IGenericRepository<DebtTransaction>
{
}
