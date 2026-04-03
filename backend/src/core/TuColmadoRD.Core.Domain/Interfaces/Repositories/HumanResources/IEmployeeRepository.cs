using System.Threading;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Entities.HumanResources;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;

namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.HumanResources;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
}
