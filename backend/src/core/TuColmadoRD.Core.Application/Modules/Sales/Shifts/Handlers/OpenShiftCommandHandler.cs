using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Shifts.Commands;
using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Handlers;

public sealed class OpenShiftCommandHandler : IRequestHandler<OpenShiftCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IShiftRepository _shiftRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OpenShiftCommandHandler(
        ITenantProvider tenantProvider,
        IShiftRepository shiftRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _shiftRepository = shiftRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<Guid, DomainError>> Handle(OpenShiftCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var terminalId = _tenantProvider.TerminalId;

        var hasOpenShift = await _shiftRepository.HasOpenShiftAsync(tenantId, terminalId, cancellationToken);
        if (hasOpenShift)
        {
            return OperationResult<Guid, DomainError>.Bad(
                DomainError.Business("shift.already_open", "Ya existe un turno abierto en esta caja."));
        }

        var openingCashResult = Money.FromDecimal(request.OpeningCashAmount);
        if (!openingCashResult.TryGetResult(out var openingCash) || openingCash is null)
        {
            return OperationResult<Guid, DomainError>.Bad(openingCashResult.Error);
        }

        var shiftResult = Shift.Open(tenantId, terminalId, openingCash, request.CashierName);
        if (!shiftResult.TryGetResult(out var shift) || shift is null)
        {
            return OperationResult<Guid, DomainError>.Bad(shiftResult.Error);
        }

        await _shiftRepository.AddAsync(shift, cancellationToken);

        var payload = new ShiftOpenedPayload(
            shift.Id,
            tenantId,
            terminalId,
            shift.CashierName,
            shift.OpeningCashAmount.Amount,
            shift.OpenedAt);

        var outboxMessage = new OutboxMessage("ShiftOpened", JsonSerializer.Serialize(payload));
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<Guid, DomainError>.Good(shift.Id);
    }
}
