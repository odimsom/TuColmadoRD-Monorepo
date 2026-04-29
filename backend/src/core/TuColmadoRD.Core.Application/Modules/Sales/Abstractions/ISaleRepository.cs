using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Core.Application.Sales.Abstractions;

/// <summary>
/// Write repository for Sale aggregate root.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Adds a new sale (and its items/payments via EF owned navigation).
    /// </summary>
    Task AddAsync(Sale sale, CancellationToken ct);

    /// <summary>
    /// Updates an existing sale.
    /// </summary>
    Task UpdateAsync(Sale sale, CancellationToken ct);

    /// <summary>
    /// Retrieves a sale by ID with all items and payments.
    /// </summary>
    Task<Sale?> GetByIdAsync(Guid saleId, Guid tenantId, CancellationToken ct);

    /// <summary>
    /// Retrieves all sales for a terminal.
    /// </summary>
    Task<IReadOnlyList<Sale>> GetByTerminalIdAsync(Guid terminalId, Guid tenantId, CancellationToken ct);

    /// <summary>
    /// Retrieves all sales for a shift.
    /// </summary>
    Task<IReadOnlyList<Sale>> GetByShiftIdAsync(Guid shiftId, Guid tenantId, CancellationToken ct);

    /// <summary>
    /// Retrieves paged sales, total count, and total revenue across all pages.
    /// </summary>
    Task<(IReadOnlyList<Sale> Items, int TotalCount, decimal TotalRevenue)> GetPagedAsync(
        Guid tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken ct);

    /// <summary>
    /// Gets queryable for custom joins and filters.
    /// </summary>
    IQueryable<Sale> GetQueryable();
}
