using System.Threading;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Entities.Audit;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;

namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.Audit;

public interface IAuditTrailRepository : IGenericRepository<AuditTrail>
{
}
