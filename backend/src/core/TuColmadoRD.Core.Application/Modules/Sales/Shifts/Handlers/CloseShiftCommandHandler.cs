using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Shifts.Commands;
using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Handlers;

public sealed class CloseShiftCommandHandler : IRequestHandler<CloseShiftCommand, OperationResult<CloseShiftResult, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IShiftRepository _shiftRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseShiftCommandHandler(
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

    public async Task<OperationResult<CloseShiftResult, DomainError>> Handle(CloseShiftCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var terminalId = _tenantProvider.TerminalId;

        var shift = await _shiftRepository.GetOpenShiftAsync(request.ShiftId, tenantId, cancellationToken);
        if (shift is null)
        {
            return OperationResult<CloseShiftResult, DomainError>.Bad(DomainError.NotFound("shift.not_found"));
        }

        if (shift.TerminalId != terminalId)
        {
            return OperationResult<CloseShiftResult, DomainError>.Bad(
                DomainError.Business("shift.terminal_mismatch", "Este turno no pertenece a esta caja."));
        }

        var expectedCashBasedOnCashOnlySales = shift.OpeningCashAmount.Amount + shift.TotalSalesAmount.Amount;

        var actualCashResult = Money.FromDecimal(request.ActualCashAmount);
        if (!actualCashResult.TryGetResult(out var actualCash) || actualCash is null)
        {
            return OperationResult<CloseShiftResult, DomainError>.Bad(actualCashResult.Error);
        }

        var expectedCashResult = Money.FromDecimal(expectedCashBasedOnCashOnlySales);
        if (!expectedCashResult.TryGetResult(out var expectedCash) || expectedCash is null)
        {
            return OperationResult<CloseShiftResult, DomainError>.Bad(expectedCashResult.Error);
        }

        var closeResult = shift.Close(actualCash, expectedCash, request.Notes);
        if (!closeResult.TryGetResult(out ResultUnit _))
        {
            return OperationResult<CloseShiftResult, DomainError>.Bad(closeResult.Error);
        }

        await _shiftRepository.UpdateAsync(shift, cancellationToken);

        var payload = new ShiftClosedPayload(
            shift.Id,
            tenantId,
            terminalId,
            shift.CashierName,
            shift.ActualCashAmount?.Amount ?? 0m,
            shift.ExpectedCashAmount?.Amount ?? 0m,
            shift.CashDifferenceAmount ?? 0m,
            shift.TotalSalesCount,
            shift.TotalSalesAmount.Amount,
            shift.ClosedAt ?? DateTime.UtcNow);

        var outboxMessage = new OutboxMessage("ShiftClosed", JsonSerializer.Serialize(payload));
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<CloseShiftResult, DomainError>.Good(new CloseShiftResult(
            shift.Id,
            shift.TotalSalesCount,
            shift.TotalSalesAmount.Amount,
            shift.ExpectedCashAmount?.Amount ?? 0m,
            shift.ActualCashAmount?.Amount ?? 0m,
            shift.CashDifferenceAmount ?? 0m,
            shift.ClosedAt ?? DateTime.UtcNow));
    }
}
