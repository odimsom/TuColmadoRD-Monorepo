using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class OpenContainerCommandHandler
    : IRequestHandler<OpenContainerCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IStockContainerRepository _containerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OpenContainerCommandHandler(ITenantProvider tenantProvider,
        IStockContainerRepository containerRepository, IUnitOfWork unitOfWork)
    {
        _tenantProvider      = tenantProvider;
        _containerRepository = containerRepository;
        _unitOfWork          = unitOfWork;
    }

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(
        OpenContainerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var result   = await _containerRepository.GetByIdAsync(request.ContainerId, tenantId, cancellationToken);
        if (!result.TryGetResult(out var container))
            return OperationResult<ResultUnit, DomainError>.Bad(result.Error);

        if (request.ActualCapacity.HasValue)
        {
            var setResult = container!.SetActualCapacity(request.ActualCapacity.Value);
            if (!setResult.IsGood) return setResult;
        }

        var openResult = container!.Open();
        if (!openResult.IsGood) return openResult;

        await _unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}

public sealed class DrawFromContainerCommandHandler
    : IRequestHandler<DrawFromContainerCommand, OperationResult<decimal, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IStockContainerRepository _containerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DrawFromContainerCommandHandler(ITenantProvider tenantProvider,
        IStockContainerRepository containerRepository, IUnitOfWork unitOfWork)
    {
        _tenantProvider      = tenantProvider;
        _containerRepository = containerRepository;
        _unitOfWork          = unitOfWork;
    }

    public async Task<OperationResult<decimal, DomainError>> Handle(
        DrawFromContainerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var result   = await _containerRepository.GetByIdAsync(request.ContainerId, tenantId, cancellationToken);
        if (!result.TryGetResult(out var container))
            return OperationResult<decimal, DomainError>.Bad(result.Error);

        var drawResult = container!.Draw(request.Amount, request.AllowOverDraw);
        if (!drawResult.IsGood)
            return OperationResult<decimal, DomainError>.Bad(drawResult.Error);

        await _unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<decimal, DomainError>.Good(container.CurrentRemaining);
    }
}

public sealed class MarkContainerEmptyCommandHandler
    : IRequestHandler<MarkContainerEmptyCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IStockContainerRepository _containerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkContainerEmptyCommandHandler(ITenantProvider tenantProvider,
        IStockContainerRepository containerRepository, IUnitOfWork unitOfWork)
    {
        _tenantProvider      = tenantProvider;
        _containerRepository = containerRepository;
        _unitOfWork          = unitOfWork;
    }

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(
        MarkContainerEmptyCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var result   = await _containerRepository.GetByIdAsync(request.ContainerId, tenantId, cancellationToken);
        if (!result.TryGetResult(out var container))
            return OperationResult<ResultUnit, DomainError>.Bad(result.Error);

        var emptyResult = container!.MarkEmpty();
        if (!emptyResult.IsGood) return emptyResult;

        await _unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}

public sealed class SetActiveContainerCommandHandler
    : IRequestHandler<SetActiveContainerCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IStockContainerRepository _containerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetActiveContainerCommandHandler(ITenantProvider tenantProvider,
        IStockContainerRepository containerRepository, IUnitOfWork unitOfWork)
    {
        _tenantProvider      = tenantProvider;
        _containerRepository = containerRepository;
        _unitOfWork          = unitOfWork;
    }

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(
        SetActiveContainerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        // Deactivate current active source (if any)
        var current = await _containerRepository.GetActiveSourceAsync(
            request.PresentationId, tenantId, cancellationToken);
        current?.SetAsActiveSource(false);

        // Activate the requested one
        var targetResult = await _containerRepository.GetByIdAsync(
            request.ContainerId, tenantId, cancellationToken);
        if (!targetResult.TryGetResult(out var target))
            return OperationResult<ResultUnit, DomainError>.Bad(targetResult.Error);

        target!.SetAsActiveSource(true);
        await _unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}
