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
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class CreateMonetaryFundCommandHandler
    : IRequestHandler<CreateMonetaryFundCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IMonetaryFundRepository _fundRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMonetaryFundCommandHandler(ITenantProvider tenantProvider,
        IMonetaryFundRepository fundRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider  = tenantProvider;
        _fundRepository  = fundRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork      = unitOfWork;
    }

    public async Task<OperationResult<Guid, DomainError>> Handle(
        CreateMonetaryFundCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var depositResult = Money.FromDecimal(request.InitialDeposit);
        if (!depositResult.TryGetResult(out var deposit))
            return OperationResult<Guid, DomainError>.Bad(depositResult.Error);

        var fundResult = MonetaryFund.Create(tenantId, request.Name, deposit!);
        if (!fundResult.TryGetResult(out var fund))
            return OperationResult<Guid, DomainError>.Bad(fundResult.Error);

        var outboxMessage = new OutboxMessage("MonetaryFundCreated",
            JsonSerializer.Serialize(new { fund!.Id, TenantId = tenantId, OccurredAt = DateTime.UtcNow }));

        await _fundRepository.AddAsync(fund, cancellationToken);
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<Guid, DomainError>.Good(fund.Id);
    }
}

public sealed class RecordFundDepositCommandHandler
    : IRequestHandler<RecordFundDepositCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IMonetaryFundRepository _fundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordFundDepositCommandHandler(ITenantProvider tenantProvider,
        IMonetaryFundRepository fundRepository, IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _fundRepository = fundRepository;
        _unitOfWork     = unitOfWork;
    }

    public async Task<OperationResult<Guid, DomainError>> Handle(
        RecordFundDepositCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var fundResult = await _fundRepository.GetByIdAsync(request.FundId, tenantId, cancellationToken);
        if (!fundResult.TryGetResult(out var fund))
            return OperationResult<Guid, DomainError>.Bad(fundResult.Error);

        var amountResult = Money.FromDecimal(request.Amount);
        if (!amountResult.TryGetResult(out var amount))
            return OperationResult<Guid, DomainError>.Bad(amountResult.Error);

        var depositResult = fund!.Deposit(amount!, request.Description);
        if (!depositResult.TryGetResult(out var tx))
            return OperationResult<Guid, DomainError>.Bad(depositResult.Error);

        await _fundRepository.TrackNewTransactionAsync(tx!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<Guid, DomainError>.Good(tx!.Id);
    }
}

public sealed class RecordFundExpenseCommandHandler
    : IRequestHandler<RecordFundExpenseCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IMonetaryFundRepository _fundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordFundExpenseCommandHandler(ITenantProvider tenantProvider,
        IMonetaryFundRepository fundRepository, IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _fundRepository = fundRepository;
        _unitOfWork     = unitOfWork;
    }

    public async Task<OperationResult<Guid, DomainError>> Handle(
        RecordFundExpenseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var fundResult = await _fundRepository.GetByIdAsync(request.FundId, tenantId, cancellationToken);
        if (!fundResult.TryGetResult(out var fund))
            return OperationResult<Guid, DomainError>.Bad(fundResult.Error);

        var amountResult = Money.FromDecimal(request.Amount);
        if (!amountResult.TryGetResult(out var amount))
            return OperationResult<Guid, DomainError>.Bad(amountResult.Error);

        var categoryResult = FundExpenseCategory.FromId(request.Category);
        if (!categoryResult.TryGetResult(out var category))
            return OperationResult<Guid, DomainError>.Bad(categoryResult.Error);

        var expenseResult = fund!.RecordExpense(
            amount!, category!, request.Description,
            request.JustificationNote, request.ReferenceId);

        if (!expenseResult.TryGetResult(out var tx))
            return OperationResult<Guid, DomainError>.Bad(expenseResult.Error);

        await _fundRepository.TrackNewTransactionAsync(tx!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<Guid, DomainError>.Good(tx!.Id);
    }
}
