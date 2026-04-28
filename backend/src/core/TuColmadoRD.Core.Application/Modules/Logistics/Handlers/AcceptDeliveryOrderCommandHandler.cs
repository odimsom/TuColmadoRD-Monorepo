using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Modules.Logistics.Commands;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Logistics;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Modules.Logistics.Handlers;

public sealed class AcceptDeliveryOrderCommandHandler : IRequestHandler<AcceptDeliveryOrderCommand, OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>
{
    private readonly IDeliveryOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptDeliveryOrderCommandHandler(
        IDeliveryOrderRepository repository,
        ITenantProvider tenantProvider,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>> Handle(AcceptDeliveryOrderCommand request, CancellationToken ct)
    {
        var tenantId = _tenantProvider.TenantId;
        var order = await _repository.GetByIdAsync(request.DeliveryOrderId, cancellationToken: ct);

        if (order is null || order.TenantId.Value != tenantId.Value)
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.NotFound("delivery.order_not_found"));

        // In a real system, we'd update DeliveryPersonId too.
        order.Dispatch();
        
        await _repository.UpdateAsync(order, ct);
        await _unitOfWork.CommitAsync(ct);

        return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Good(TuColmadoRD.Core.Domain.Base.Result.Unit.Value);
    }
}
