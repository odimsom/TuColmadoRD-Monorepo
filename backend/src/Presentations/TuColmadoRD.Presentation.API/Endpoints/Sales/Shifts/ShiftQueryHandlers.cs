using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Sales.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Presentation.API.Endpoints.Sales.Shifts.Handlers;

internal sealed class GetCurrentShiftSummaryQueryHandler : IRequestHandler<GetCurrentShiftSummaryQuery, OperationResult<ShiftSummaryReportDto, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetCurrentShiftSummaryQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<ShiftSummaryReportDto, DomainError>> Handle(GetCurrentShiftSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var shift = await _dbContext.Set<Shift>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId.Value == tenantId && s.TerminalId == request.TerminalId && s.Status == TuColmadoRD.Core.Domain.Enums.Sales.ShiftStatus.Open, cancellationToken);

        if (shift == null)
            return OperationResult<ShiftSummaryReportDto, DomainError>.Bad(DomainError.NotFound("Shift.NotFound", "No hay turno activo para este terminal."));

        var summary = new ShiftSummaryReportDto(
            shift.Id,
            shift.OpenedAt,
            shift.OpeningCashAmount.Amount,
            shift.TotalCashSales.Amount,
            shift.TotalAccountPayments.Amount,
            shift.TotalExpenses.Amount,
            shift.ExpectedCashAmount != null ? shift.ExpectedCashAmount.Amount : 0m);

        return OperationResult<ShiftSummaryReportDto, DomainError>.Good(summary);
    }
}
