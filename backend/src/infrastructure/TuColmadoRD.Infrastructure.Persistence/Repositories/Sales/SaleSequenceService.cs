using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;

public sealed class SaleSequenceService(TuColmadoDbContext dbContext) : ISaleSequenceService
{
    public async Task<OperationResult<string, DomainError>> GenerateReceiptNumberAsync(
        Guid tenantId,
        Guid terminalId,
        CancellationToken ct)
    {
        if (tenantId == Guid.Empty)
        {
            return OperationResult<string, DomainError>.Bad(DomainError.Validation("sale.tenant_required"));
        }

        var now = DateTime.UtcNow;
        var startOfDay = now.Date;
        var endOfDay = startOfDay.AddDays(1);

        var dailyCount = await dbContext.Sales
            .Where(s => s.TenantId.Value == tenantId
                && s.TerminalId == terminalId
                && s.CreatedAt >= startOfDay
                && s.CreatedAt < endOfDay)
            .CountAsync(ct);

        var sequence = dailyCount + 1;
        var terminalPart = terminalId == Guid.Empty
            ? "WEB0"
            : terminalId.ToString("N")[..4].ToUpperInvariant();
        var receipt = $"REC-{terminalPart}-{now:yyyyMMdd}-{sequence:D5}";

        return OperationResult<string, DomainError>.Good(receipt);
    }
}
