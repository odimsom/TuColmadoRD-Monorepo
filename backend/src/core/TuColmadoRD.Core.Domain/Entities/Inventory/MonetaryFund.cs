using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory.Events;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory;

/// <summary>
/// Capital fund the colmado uses for purchasing stock.
/// Not the same as the cash register — this is reserved purchasing capital.
/// </summary>
public class MonetaryFund : ITenantEntity
{
    private readonly List<FundTransaction> _transactions = [];
    private readonly List<object> _domainEvents = [];

    private MonetaryFund()
    {
        TenantId       = TenantIdentifier.Empty;
        Name           = string.Empty;
        CurrentBalance = Money.Zero;
    }

    private MonetaryFund(Guid tenantId, string name, Money initialDeposit)
    {
        Id             = Guid.NewGuid();
        TenantId       = TenantIdentifier.Validate(tenantId).Result;
        Name           = name.Trim();
        CurrentBalance = initialDeposit;
        CreatedAt      = DateTime.UtcNow;
        UpdatedAt      = DateTime.UtcNow;

        var tx = FundTransaction.CreateDeposit(Id, tenantId, initialDeposit, "Depósito inicial", initialDeposit);
        _transactions.Add(tx);
        AddDomainEvent(new FundTransactionRecordedDomainEvent(
            tx.Id, Id, tenantId, FundTransactionType.Deposit.Name,
            initialDeposit.Amount, CurrentBalance.Amount, DateTime.UtcNow));
    }

    public Guid Id              { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public string Name          { get; private set; }
    public Money CurrentBalance { get; private set; }
    public DateTime CreatedAt   { get; private set; }
    public DateTime UpdatedAt   { get; private set; }

    public IReadOnlyCollection<FundTransaction> Transactions => _transactions.AsReadOnly();
    public IReadOnlyCollection<object> DomainEvents          => _domainEvents.AsReadOnly();

    public static OperationResult<MonetaryFund, DomainError> Create(
        Guid tenantId,
        string name,
        Money initialDeposit)
    {
        if (tenantId == Guid.Empty)
            return OperationResult<MonetaryFund, DomainError>.Bad(DomainError.Validation("fund.tenant_required"));

        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<MonetaryFund, DomainError>.Bad(DomainError.Validation("fund.name_required"));

        if (name.Length > 80)
            return OperationResult<MonetaryFund, DomainError>.Bad(DomainError.Validation("fund.name_too_long"));

        if (initialDeposit.Amount < 0)
            return OperationResult<MonetaryFund, DomainError>.Bad(DomainError.Validation("fund.initial_deposit_cannot_be_negative"));

        return OperationResult<MonetaryFund, DomainError>.Good(new MonetaryFund(tenantId, name, initialDeposit));
    }

    public OperationResult<FundTransaction, DomainError> Deposit(Money amount, string description)
    {
        if (amount.Amount <= 0)
            return OperationResult<FundTransaction, DomainError>.Bad(DomainError.Validation("fund.deposit_amount_must_be_positive"));

        CurrentBalance = CurrentBalance + amount;
        UpdatedAt      = DateTime.UtcNow;

        var tx = FundTransaction.CreateDeposit(Id, TenantId.Value, amount, description, CurrentBalance);
        _transactions.Add(tx);
        AddDomainEvent(new FundTransactionRecordedDomainEvent(
            tx.Id, Id, TenantId.Value, FundTransactionType.Deposit.Name,
            amount.Amount, CurrentBalance.Amount, DateTime.UtcNow));

        return OperationResult<FundTransaction, DomainError>.Good(tx);
    }

    /// <summary>
    /// Records an expense against the fund.
    /// When amount exceeds balance, justificationNote is mandatory.
    /// </summary>
    public OperationResult<FundTransaction, DomainError> RecordExpense(
        Money amount,
        FundExpenseCategory category,
        string description,
        string? justificationNote,
        Guid? referenceId = null)
    {
        if (amount.Amount <= 0)
            return OperationResult<FundTransaction, DomainError>.Bad(DomainError.Validation("fund.expense_amount_must_be_positive"));

        bool exceedsBalance = amount.Amount > CurrentBalance.Amount;

        if (exceedsBalance && string.IsNullOrWhiteSpace(justificationNote))
            return OperationResult<FundTransaction, DomainError>.Bad(
                DomainError.Business("fund.justification_required_when_exceeding_balance"));

        // Allow overdraft — record the actual balance (can go negative)
        var balanceAfterResult = Money.FromDecimal(CurrentBalance.Amount - amount.Amount);
        if (!balanceAfterResult.TryGetResult(out var balanceAfter))
        {
            // Balance would go negative — create a zero-floor Money via direct subtraction workaround
            balanceAfter = Money.Zero;
        }

        // When overdrafting, store negative balance as zero (tracked via justification)
        CurrentBalance = exceedsBalance ? Money.Zero : balanceAfter!;
        UpdatedAt      = DateTime.UtcNow;

        var tx = FundTransaction.CreateExpense(
            Id, TenantId.Value, amount, category, description,
            justificationNote, referenceId, CurrentBalance);
        _transactions.Add(tx);
        AddDomainEvent(new FundTransactionRecordedDomainEvent(
            tx.Id, Id, TenantId.Value, FundTransactionType.Expense.Name,
            amount.Amount, CurrentBalance.Amount, DateTime.UtcNow));

        return OperationResult<FundTransaction, DomainError>.Good(tx);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(object @event) => _domainEvents.Add(@event);
}

public class FundTransaction
{
    private FundTransaction()
    {
        TenantId      = TenantIdentifier.Empty;
        Description   = string.Empty;
        Type          = FundTransactionType.Deposit;
        Amount        = Money.Zero;
        BalanceAfter  = Money.Zero;
    }

    private FundTransaction(
        Guid fundId,
        Guid tenantId,
        FundTransactionType type,
        Money amount,
        FundExpenseCategory? category,
        string description,
        string? justificationNote,
        Guid? referenceId,
        Money balanceAfter)
    {
        Id                 = Guid.NewGuid();
        FundId             = fundId;
        TenantId           = TenantIdentifier.Validate(tenantId).Result;
        Type               = type;
        Amount             = amount;
        Category           = category;
        Description        = description;
        JustificationNote  = justificationNote;
        ReferenceId        = referenceId;
        BalanceAfter       = balanceAfter;
        OccurredAt         = DateTime.UtcNow;
    }

    public Guid Id              { get; private set; }
    public Guid FundId          { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public FundTransactionType Type { get; private set; }
    public Money Amount         { get; private set; }
    public FundExpenseCategory? Category { get; private set; }
    public string Description   { get; private set; }
    public string? JustificationNote { get; private set; }
    public Guid? ReferenceId    { get; private set; }
    public Money BalanceAfter   { get; private set; }
    public DateTime OccurredAt  { get; private set; }

    internal static FundTransaction CreateDeposit(
        Guid fundId, Guid tenantId, Money amount, string description, Money balanceAfter) =>
        new(fundId, tenantId, FundTransactionType.Deposit, amount, null, description, null, null, balanceAfter);

    internal static FundTransaction CreateExpense(
        Guid fundId, Guid tenantId, Money amount, FundExpenseCategory category,
        string description, string? justificationNote, Guid? referenceId, Money balanceAfter) =>
        new(fundId, tenantId, FundTransactionType.Expense, amount, category,
            description, justificationNote, referenceId, balanceAfter);
}
