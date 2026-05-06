using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Customers;

public class CustomerRepository(TuColmadoDbContext dbContext) : GenericRepository<Customer>(dbContext), ICustomerRepository
{
    public async Task<Customer?> GetByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Customer>().FirstOrDefaultAsync(c => c.DocumentId != null && c.DocumentId.Value == documentId, cancellationToken);
    }
}
