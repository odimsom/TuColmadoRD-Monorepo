using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class GetMonetaryFundQueryHandler
    : IRequestHandler<GetMonetaryFundQuery, OperationResult<FundBalanceResponse, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IMonetaryFundRepository _fundRepository;

    public GetMonetaryFundQueryHandler(
        ITenantProvider tenantProvider,
        IMonetaryFundRepository fundRepository)
    {
        _tenantProvider  = tenantProvider;
        _fundRepository  = fundRepository;
    }

    public async Task<OperationResult<FundBalanceResponse, DomainError>> Handle(
        GetMonetaryFundQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var fundResult = await _fundRepository.GetByIdAsync(request.FundId, tenantId, cancellationToken);
        if (!fundResult.TryGetResult(out var fund))
            return OperationResult<FundBalanceResponse, DomainError>.Bad(fundResult.Error);

        var txDtos = fund!.Transactions
            .OrderByDescending(t => t.OccurredAt)
            .Take(50)
            .Select(t => new FundTransactionDto(
                t.Id,
                t.FundId,
                t.Type.Id,
                t.Type.Name,
                t.Amount.Amount,
                t.Category?.Id,
                t.Category?.Name,
                t.Description,
                t.JustificationNote,
                t.ReferenceId,
                t.BalanceAfter.Amount,
                t.OccurredAt))
            .ToList();

        var fundDto = new MonetaryFundDto(
            fund.Id,
            fund.TenantId.Value,
            fund.Name,
            fund.CurrentBalance.Amount,
            fund.CreatedAt);

        return OperationResult<FundBalanceResponse, DomainError>.Good(
            new FundBalanceResponse(fundDto, txDtos));
    }
}
