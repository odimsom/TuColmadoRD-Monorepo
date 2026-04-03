using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Queries;

public sealed record ShiftSummaryReportDto(
    Guid ShiftId,
    DateTime OpenedAt,
    decimal InitialCash,
    decimal TotalCashSales,
    decimal TotalAccountPayments,
    decimal TotalExpenses,
    decimal ExpectedCash);

public sealed record GetCurrentShiftSummaryQuery(Guid TerminalId) : IRequest<OperationResult<ShiftSummaryReportDto, DomainError>>;
