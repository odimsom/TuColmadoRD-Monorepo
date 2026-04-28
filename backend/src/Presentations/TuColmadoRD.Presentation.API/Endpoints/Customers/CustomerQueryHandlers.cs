using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Customers.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Core.Domain.Entities.Customers;

namespace TuColmadoRD.Presentation.API.Endpoints.Customers.Handlers;

internal sealed class GetCustomersWithBalanceQueryHandler : IRequestHandler<GetCustomersWithBalanceQuery, OperationResult<IReadOnlyList<CustomerSummaryDto>, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetCustomersWithBalanceQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<IReadOnlyList<CustomerSummaryDto>, DomainError>> Handle(GetCustomersWithBalanceQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        // Join customer and customerAccount to get balance and credit limit
        var customers = await (
            from c in _dbContext.Set<Customer>().AsNoTracking()
            join ca in _dbContext.Set<CustomerAccount>().AsNoTracking()
                on c.Id equals ca.CustomerId into leftJ
            from account in leftJ.DefaultIfEmpty()
            where c.TenantId.Value == tenantId && c.IsActive
            select new CustomerSummaryDto(
                c.Id,
                c.FullName,
                c.ContactPhone != null ? c.ContactPhone.Value : string.Empty,
                account != null ? account.Balance.Amount : 0m,
                account != null ? account.CreditLimit.Amount : 0m,
                c.IsActive,
                c.HomeAddress != null ? c.HomeAddress.Province : null,
                c.HomeAddress != null ? c.HomeAddress.Sector : null,
                c.HomeAddress != null ? c.HomeAddress.Street : null,
                c.HomeAddress != null ? c.HomeAddress.HouseNumber : null,
                c.HomeAddress != null ? c.HomeAddress.Reference : null,
                c.HomeAddress != null ? c.HomeAddress.Latitude : null,
                c.HomeAddress != null ? c.HomeAddress.Longitude : null
            )).ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyList<CustomerSummaryDto>, DomainError>.Good(customers);
    }
}

internal sealed class GetCustomerStatementQueryHandler : IRequestHandler<GetCustomerStatementQuery, OperationResult<IReadOnlyList<CustomerStatementDto>, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetCustomerStatementQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<IReadOnlyList<CustomerStatementDto>, DomainError>> Handle(GetCustomerStatementQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var account = await _dbContext.Set<CustomerAccount>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TenantId.Value == tenantId && a.CustomerId == request.CustomerId, cancellationToken);

        if (account == null)
            return OperationResult<IReadOnlyList<CustomerStatementDto>, DomainError>.Good(new List<CustomerStatementDto>());

        var statements = await _dbContext.Set<DebtTransaction>()
            .AsNoTracking()
            .Where(t => t.TenantId.Value == tenantId && t.CustomerAccountId == account.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new CustomerStatementDto(
                t.Id,
                t.CreatedAt,
                t.Type.ToString(),
                t.Amount.Amount,
                t.Concept))
            .ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyList<CustomerStatementDto>, DomainError>.Good(statements);
    }
}
