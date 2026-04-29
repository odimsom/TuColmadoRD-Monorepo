using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Queries;

/// <summary>
/// Service for querying sales data with pagination and filtering.
/// Decouples query logic from command handlers.
/// </summary>
public sealed class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;

    public SaleService(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<OperationResult<IEnumerable<Sale>, DomainError>> GetSalesByTerminalAsync(
        Guid terminalId, Guid tenantId, CancellationToken ct)
    {
        var sales = await _saleRepository.GetByTerminalIdAsync(terminalId, tenantId, ct);
        return OperationResult<IEnumerable<Sale>, DomainError>.Good(sales);
    }

    public async Task<OperationResult<IEnumerable<Sale>, DomainError>> GetSalesByShiftAsync(
        Guid shiftId, Guid tenantId, CancellationToken ct)
    {
        var sales = await _saleRepository.GetByShiftIdAsync(shiftId, tenantId, ct);
        return OperationResult<IEnumerable<Sale>, DomainError>.Good(sales);
    }

    public async Task<OperationResult<Sale, DomainError>> GetSaleDetailAsync(
        Guid saleId, Guid tenantId, CancellationToken ct)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId, tenantId, ct);
        if (sale is null)
            return OperationResult<Sale, DomainError>.Bad(
                DomainError.NotFound("sale.not_found"));

        return OperationResult<Sale, DomainError>.Good(sale);
    }

    public async Task<OperationResult<SalePaginationResult, DomainError>> GetPagedSalesAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken ct)
    {
        if (pageNumber < 1 || pageSize < 1)
            return OperationResult<SalePaginationResult, DomainError>.Bad(
                DomainError.Business("pagination.invalid_params",
                    "Número o tamaño de página inválido."));

        var (sales, totalCount, totalRevenue) = await _saleRepository
            .GetPagedAsync(tenantId, pageNumber, pageSize, ct);

        var result = new SalePaginationResult(
            sales,
            pageNumber,
            pageSize,
            totalCount,
            totalRevenue);

        return OperationResult<SalePaginationResult, DomainError>.Good(result);
    }
}

/// <summary>
/// Pagination result for sales.
/// </summary>
public sealed record SalePaginationResult(
    IEnumerable<Sale> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    decimal TotalRevenue);
