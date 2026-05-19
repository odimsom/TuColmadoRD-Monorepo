namespace TuColmadoRD.Core.Application.Inventory.DTOs;

public sealed record MonetaryFundDto(
    Guid Id,
    Guid TenantId,
    string Name,
    decimal CurrentBalance,
    DateTime CreatedAt);

public sealed record FundTransactionDto(
    Guid Id,
    Guid FundId,
    int Type,
    string TypeName,
    decimal Amount,
    int? Category,
    string? CategoryName,
    string Description,
    string? JustificationNote,
    Guid? ReferenceId,
    decimal BalanceAfter,
    DateTime OccurredAt);

public sealed record FundBalanceResponse(
    MonetaryFundDto Fund,
    IReadOnlyList<FundTransactionDto> RecentTransactions);
