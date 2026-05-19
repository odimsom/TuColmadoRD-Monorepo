using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class ConfirmStockEntryCommandHandler
    : IRequestHandler<ConfirmStockEntryCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IPresentationRepository _presentationRepository;
    private readonly IStockContainerRepository _containerRepository;
    private readonly IPackagedStockRepository _packagedStockRepository;
    private readonly IStockEntryRepository _stockEntryRepository;
    private readonly IMonetaryFundRepository _fundRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmStockEntryCommandHandler(
        ITenantProvider tenantProvider,
        IPresentationRepository presentationRepository,
        IStockContainerRepository containerRepository,
        IPackagedStockRepository packagedStockRepository,
        IStockEntryRepository stockEntryRepository,
        IMonetaryFundRepository fundRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider        = tenantProvider;
        _presentationRepository = presentationRepository;
        _containerRepository   = containerRepository;
        _packagedStockRepository = packagedStockRepository;
        _stockEntryRepository  = stockEntryRepository;
        _fundRepository        = fundRepository;
        _outboxRepository      = outboxRepository;
        _unitOfWork            = unitOfWork;
    }

    public async Task<OperationResult<Guid, DomainError>> Handle(
        ConfirmStockEntryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        if (request.Lines is null || request.Lines.Count == 0)
            return OperationResult<Guid, DomainError>.Bad(DomainError.Validation("stock_entry.must_have_at_least_one_line"));

        var entryResult = StockEntry.Create(tenantId, request.PurchasedAt, request.SupplierName, request.Notes);
        if (!entryResult.TryGetResult(out var entry))
            return OperationResult<Guid, DomainError>.Bad(entryResult.Error);

        var newContainers  = new List<StockContainer>();
        var stockToUpdate  = new List<(PackagedStock stock, int units)>();

        foreach (var lineDto in request.Lines)
        {
            var presentationResult = await _presentationRepository.GetByIdAsync(
                lineDto.PresentationId, tenantId, cancellationToken);
            if (!presentationResult.TryGetResult(out var presentation))
                return OperationResult<Guid, DomainError>.Bad(presentationResult.Error);

            var costPerUnitResult = Money.FromDecimal(lineDto.CostPerUnit);
            if (!costPerUnitResult.TryGetResult(out var costPerUnit))
                return OperationResult<Guid, DomainError>.Bad(costPerUnitResult.Error);

            var addLineResult = entry!.AddLine(
                lineDto.PresentationId, lineDto.ContainerCount,
                lineDto.UnitsPerContainer, lineDto.NominalSizePerUnit, costPerUnit!);
            if (!addLineResult.IsGood)
                return OperationResult<Guid, DomainError>.Bad(addLineResult.Error);

            if (presentation!.PresentationType == PresentationType.BulkContainer)
            {
                for (int i = 0; i < lineDto.ContainerCount; i++)
                {
                    var code = await _containerRepository.NextContainerCodeAsync(tenantId, cancellationToken);
                    var containerResult = StockContainer.Create(
                        tenantId, lineDto.PresentationId, code,
                        lineDto.NominalSizePerUnit, null, request.PurchasedAt, null);
                    if (!containerResult.TryGetResult(out var container))
                        return OperationResult<Guid, DomainError>.Bad(containerResult.Error);

                    newContainers.Add(container!);
                }
            }
            else
            {
                var existing = await _packagedStockRepository.GetByPresentationIdAsync(
                    lineDto.PresentationId, tenantId, cancellationToken);

                if (existing is null)
                {
                    var newStockResult = PackagedStock.Create(tenantId, lineDto.PresentationId);
                    if (!newStockResult.TryGetResult(out var newStock))
                        return OperationResult<Guid, DomainError>.Bad(newStockResult.Error);

                    newStock!.Add(lineDto.ContainerCount * lineDto.UnitsPerContainer);
                    await _packagedStockRepository.AddAsync(newStock, cancellationToken);
                }
                else
                {
                    stockToUpdate.Add((existing, lineDto.ContainerCount * lineDto.UnitsPerContainer));
                }
            }
        }

        var confirmResult = entry!.Confirm();
        if (!confirmResult.IsGood)
            return OperationResult<Guid, DomainError>.Bad(confirmResult.Error);

        // Optionally charge monetary fund
        if (request.FundId.HasValue)
        {
            var fundResult = await _fundRepository.GetByIdAsync(request.FundId.Value, tenantId, cancellationToken);
            if (fundResult.TryGetResult(out var fund) && fund is not null)
            {
                var expenseResult = fund.RecordExpense(
                    entry.TotalCost,
                    FundExpenseCategory.StockPurchase,
                    $"Entrada de stock #{entry.Id}",
                    request.FundExpenseJustification);

                if (expenseResult.TryGetResult(out var tx) && tx is not null)
                    entry.LinkFundTransaction(tx.Id);
            }
        }

        foreach (var (stock, units) in stockToUpdate)
            stock.Add(units);

        if (newContainers.Count > 0)
            await _containerRepository.AddRangeAsync(newContainers, cancellationToken);

        await _stockEntryRepository.AddAsync(entry, cancellationToken);

        var outboxMessage = new OutboxMessage("StockEntryConfirmed",
            JsonSerializer.Serialize(new
            {
                EntryId = entry.Id,
                TenantId = tenantId,
                entry.TotalCost.Amount,
                LineCount = request.Lines.Count,
                OccurredAt = DateTime.UtcNow
            }));
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<Guid, DomainError>.Good(entry.Id);
    }
}
