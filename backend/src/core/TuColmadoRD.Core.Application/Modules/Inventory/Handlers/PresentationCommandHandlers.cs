using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class DeactivatePresentationCommandHandler
    : IRequestHandler<DeactivatePresentationCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IPresentationRepository _presentationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePresentationCommandHandler(
        ITenantProvider tenantProvider,
        IPresentationRepository presentationRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider          = tenantProvider;
        _presentationRepository  = presentationRepository;
        _unitOfWork              = unitOfWork;
    }

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(
        DeactivatePresentationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var result   = await _presentationRepository.GetByIdAsync(request.PresentationId, tenantId, cancellationToken);
        if (!result.TryGetResult(out var presentation))
            return OperationResult<ResultUnit, DomainError>.Bad(result.Error);

        presentation!.Deactivate();
        await _unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}

public sealed class UpdatePresentationPriceCommandHandler
    : IRequestHandler<UpdatePresentationPriceCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IPresentationRepository _presentationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePresentationPriceCommandHandler(
        ITenantProvider tenantProvider,
        IPresentationRepository presentationRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider          = tenantProvider;
        _presentationRepository  = presentationRepository;
        _unitOfWork              = unitOfWork;
    }

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(
        UpdatePresentationPriceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var presResult = await _presentationRepository.GetByIdAsync(
            request.PresentationId, tenantId, cancellationToken);
        if (!presResult.TryGetResult(out var presentation))
            return OperationResult<ResultUnit, DomainError>.Bad(presResult.Error);

        var costResult = Money.FromDecimal(request.NewCostPrice);
        if (!costResult.TryGetResult(out var cost))
            return OperationResult<ResultUnit, DomainError>.Bad(costResult.Error);

        var saleResult = Money.FromDecimal(request.NewSalePrice);
        if (!saleResult.TryGetResult(out var sale))
            return OperationResult<ResultUnit, DomainError>.Bad(saleResult.Error);

        var updateResult = presentation!.UpdatePrice(cost!, sale!);
        if (!updateResult.IsGood)
            return OperationResult<ResultUnit, DomainError>.Bad(updateResult.Error);

        await _unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}
