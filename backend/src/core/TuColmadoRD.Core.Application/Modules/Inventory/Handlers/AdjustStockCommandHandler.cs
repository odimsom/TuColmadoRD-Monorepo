using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

/// <summary>
/// Handles stock adjustment command.
/// </summary>
public sealed class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IProductRepository _productRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdjustStockCommandHandler(
        ITenantProvider tenantProvider,
        IProductRepository productRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _productRepository = productRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<OperationResult<ResultUnit, DomainError>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(OperationResult<ResultUnit, DomainError>.Bad(
            DomainError.Business("stock.operation_deprecated",
                "Stock is now tracked per presentation. Use DrawFromContainerCommand or ConfirmStockEntryCommand.")));
    }
}
