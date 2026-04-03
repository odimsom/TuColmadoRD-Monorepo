using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Customers;        

public class CustomerAccountRepository(TuColmadoDbContext dbContext) : GenericRepository<CustomerAccount>(dbContext), ICustomerAccountRepository
{
    public async Task<CustomerAccount?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<CustomerAccount>()
            .FirstOrDefaultAsync(ca => ca.CustomerId == customerId, cancellationToken);
    }
}
