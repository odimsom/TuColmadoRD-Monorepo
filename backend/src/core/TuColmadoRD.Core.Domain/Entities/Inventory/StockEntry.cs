using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory.Events;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory;

/// <summary>
/// A purchase event that can contain multiple lines of different presentations.
/// Example: "4 boxes × 50 units of 1-lb bags + 2 sacks × 50 lb bulk rice".
/// </summary>
public class StockEntry : ITenantEntity
{
    private readonly List<StockEntryLine> _lines = [];
    private readonly List<object> _domainEvents = [];

    private StockEntry()
    {
        TenantId      = TenantIdentifier.Empty;
        SupplierName  = string.Empty;
        Notes         = string.Empty;
        TotalCost     = Money.Zero;
    }

    private StockEntry(Guid tenantId, DateTime purchasedAt, string? supplierName, string? notes)
    {
        Id           = Guid.NewGuid();
        TenantId     = TenantIdentifier.Validate(tenantId).Result;
        PurchasedAt  = purchasedAt;
        SupplierName = supplierName?.Trim() ?? string.Empty;
        Notes        = notes?.Trim() ?? string.Empty;
        TotalCost    = Money.Zero;
        CreatedAt    = DateTime.UtcNow;
    }

    public Guid Id              { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public DateTime PurchasedAt { get; private set; }
    public string SupplierName  { get; private set; }
    public string Notes         { get; private set; }
    public Money TotalCost      { get; private set; }
    public Guid? FundTransactionId { get; private set; }
    public DateTime CreatedAt   { get; private set; }

    public IReadOnlyCollection<StockEntryLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<object> DomainEvents  => _domainEvents.AsReadOnly();

    public static OperationResult<StockEntry, DomainError> Create(
        Guid tenantId,
        DateTime purchasedAt,
        string? supplierName,
        string? notes)
    {
        if (tenantId == Guid.Empty)
            return OperationResult<StockEntry, DomainError>.Bad(DomainError.Validation("stock_entry.tenant_required"));

        return OperationResult<StockEntry, DomainError>.Good(new StockEntry(tenantId, purchasedAt, supplierName, notes));
    }

    public OperationResult<Unit, DomainError> AddLine(
        Guid presentationId,
        int containerCount,
        int unitsPerContainer,
        decimal nominalSizePerUnit,
        Money costPerUnit)
    {
        if (presentationId == Guid.Empty)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("stock_entry_line.presentation_required"));

        if (containerCount <= 0)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("stock_entry_line.container_count_must_be_positive"));

        if (unitsPerContainer <= 0)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("stock_entry_line.units_per_container_must_be_positive"));

        if (nominalSizePerUnit <= 0)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("stock_entry_line.nominal_size_must_be_positive"));

        var line = new StockEntryLine(Id, presentationId, containerCount, unitsPerContainer, nominalSizePerUnit, costPerUnit);
        _lines.Add(line);

        var lineTotal = costPerUnit.Amount * containerCount * unitsPerContainer;
        TotalCost = TotalCost + Money.FromDecimal(lineTotal).Result;

        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public OperationResult<Unit, DomainError> Confirm()
    {
        if (_lines.Count == 0)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("stock_entry.must_have_at_least_one_line"));

        AddDomainEvent(new StockEntryConfirmedDomainEvent(Id, TenantId.Value, TotalCost.Amount, _lines.Count, DateTime.UtcNow));
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    public void LinkFundTransaction(Guid fundTransactionId)
    {
        FundTransactionId = fundTransactionId;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(object @event) => _domainEvents.Add(@event);
}

public class StockEntryLine
{
    internal StockEntryLine(
        Guid stockEntryId,
        Guid presentationId,
        int containerCount,
        int unitsPerContainer,
        decimal nominalSizePerUnit,
        Money costPerUnit)
    {
        Id                 = Guid.NewGuid();
        StockEntryId       = stockEntryId;
        PresentationId     = presentationId;
        ContainerCount     = containerCount;
        UnitsPerContainer  = unitsPerContainer;
        NominalSizePerUnit = nominalSizePerUnit;
        CostPerUnit        = costPerUnit;
    }

    // EF Core needs a parameterless ctor
    private StockEntryLine() { CostPerUnit = Money.Zero; }

    public Guid Id                 { get; private set; }
    public Guid StockEntryId       { get; private set; }
    public Guid PresentationId     { get; private set; }

    /// <summary>Number of boxes / sacks purchased.</summary>
    public int ContainerCount      { get; private set; }

    /// <summary>Units inside each box (1 for bulk sacks).</summary>
    public int UnitsPerContainer   { get; private set; }

    /// <summary>Declared weight/volume per unit (e.g. 1 lb per bag).</summary>
    public decimal NominalSizePerUnit { get; private set; }

    public Money CostPerUnit       { get; private set; }

    public int TotalUnits => ContainerCount * UnitsPerContainer;
    public decimal TotalNominalWeight => TotalUnits * NominalSizePerUnit;
}
