using System.Threading;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;

namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;

public interface ISaleRepository : IGenericRepository<Sale>
{
}
