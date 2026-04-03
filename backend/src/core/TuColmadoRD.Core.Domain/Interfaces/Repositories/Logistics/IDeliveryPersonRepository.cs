using System.Threading;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Entities.Logistics;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;

namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.Logistics;

public interface IDeliveryPersonRepository : IGenericRepository<DeliveryPerson>
{
}
