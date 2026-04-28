using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Modules.Logistics.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Logistics;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Presentation.API.Endpoints.Logistics.Handlers;

internal sealed class GetPendingDeliveryOrdersQueryHandler : IRequestHandler<GetPendingDeliveryOrdersQuery, OperationResult<IReadOnlyList<DeliveryOrderDto>, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetPendingDeliveryOrdersQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<IReadOnlyList<DeliveryOrderDto>, DomainError>> Handle(GetPendingDeliveryOrdersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        // Joining with Sales and Customers to get the full order info
        var orders = await (from d in _dbContext.DeliveryOrders
                            join s in _dbContext.Sales on d.SaleId equals s.Id
                            // Try to get the first payment to find the customer (for credit sales)
                            let payment = s.Payments.FirstOrDefault()
                            join c in _dbContext.Customers on payment.CustomerId equals c.Id into custGroup
                            from c in custGroup.DefaultIfEmpty()
                            where d.TenantId.Value == tenantId && d.Status == DeliveryStatus.Pending
                            select new DeliveryOrderDto(
                                d.Id,
                                d.SaleId,
                                s.ReceiptNumber,
                                s.TotalAmount,
                                c != null ? c.FullName : "Cliente " + s.ReceiptNumber,
                                c != null && c.ContactPhone != null ? c.ContactPhone.Value : "",
                                d.Destination.Province,
                                d.Destination.Sector,
                                d.Destination.Street,
                                d.Destination.HouseNumber,
                                d.Destination.Reference,
                                d.Destination.Latitude,
                                d.Destination.Longitude,
                                d.Status.ToString()
                            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyList<DeliveryOrderDto>, DomainError>.Good(orders);
    }
}
