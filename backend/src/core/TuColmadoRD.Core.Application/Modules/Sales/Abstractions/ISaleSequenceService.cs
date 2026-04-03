using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Abstractions;

/// <summary>
/// Service for generating unique receipt numbers.
/// </summary>
public interface ISaleSequenceService
{
    /// <summary>
    /// Generates a unique receipt number for a sale.
    /// Format: "REC-{terminalId[..4].ToUpper()}-{yyyyMMdd}-{seq:D5}"
    /// </summary>
    Task<OperationResult<string, DomainError>> GenerateReceiptNumberAsync(
        Guid tenantId,
        Guid terminalId,
        CancellationToken ct);
}
