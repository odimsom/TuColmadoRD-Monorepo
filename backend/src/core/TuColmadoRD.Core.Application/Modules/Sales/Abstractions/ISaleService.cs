using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Core.Application.Sales.Queries;

namespace TuColmadoRD.Core.Application.Sales.Abstractions;

public interface ISaleService
{
    Task<OperationResult<IEnumerable<Sale>, DomainError>> GetSalesByTerminalAsync(
        Guid terminalId,
        Guid tenantId,
        CancellationToken ct);

    Task<OperationResult<IEnumerable<Sale>, DomainError>> GetSalesByShiftAsync(
        Guid shiftId,
        Guid tenantId,
        CancellationToken ct);

    Task<OperationResult<Sale, DomainError>> GetSaleDetailAsync(
        Guid saleId,
        Guid tenantId,
        CancellationToken ct);

    Task<OperationResult<SalePaginationResult, DomainError>> GetPagedSalesAsync(
        Guid tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken ct);
}
