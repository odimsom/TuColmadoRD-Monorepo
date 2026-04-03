using System.Threading;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Entities.Purchasing;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;

namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.Purchasing;

public interface IPurchaseOrderRepository : IGenericRepository<PurchaseOrder>
{
}
