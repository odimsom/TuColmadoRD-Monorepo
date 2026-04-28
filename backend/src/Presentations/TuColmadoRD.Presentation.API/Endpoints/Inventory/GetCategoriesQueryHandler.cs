using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Presentation.API.Endpoints.Inventory.Handlers;

internal sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, OperationResult<IReadOnlyList<CategoryDto>, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetCategoriesQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<IReadOnlyList<CategoryDto>, DomainError>> Handle(
        GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var categories = await _dbContext.Set<Category>()
            .AsNoTracking()
            .Where(c => c.TenantId.Value == tenantId && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyList<CategoryDto>, DomainError>.Good(categories);
    }
}
