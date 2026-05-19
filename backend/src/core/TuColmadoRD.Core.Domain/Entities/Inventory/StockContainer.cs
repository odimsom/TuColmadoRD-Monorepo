using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory.Events;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory;

/// <summary>
/// Tracks an individual bulk container (sack, jar, bar).
/// One StockContainer = one physical sack/container purchased.
/// </summary>
public class StockContainer : ITenantEntity
{
    private readonly List<object> _domainEvents = [];

    private StockContainer()
    {
        TenantId      = TenantIdentifier.Empty;
        ContainerCode = string.Empty;
        Status        = ContainerStatus.Sealed;
        Notes         = string.Empty;
    }

    private StockContainer(
        Guid tenantId,
        Guid presentationId,
        string containerCode,
        decimal nominalCapacity,
        decimal? actualCapacity,
        DateTime purchasedAt,
        string? notes)
    {
        Id               = Guid.NewGuid();
        TenantId         = TenantIdentifier.Validate(tenantId).Result;
        PresentationId   = presentationId;
        ContainerCode    = containerCode;
        NominalCapacity  = nominalCapacity;
        ActualCapacity   = actualCapacity;
        CurrentRemaining = actualCapacity ?? nominalCapacity;
        Status           = ContainerStatus.Sealed;
        IsActiveSource   = false;
        Notes            = notes?.Trim() ?? string.Empty;
        PurchasedAt      = purchasedAt;
        CreatedAt        = DateTime.UtcNow;
        UpdatedAt        = DateTime.UtcNow;
    }

    public Guid Id              { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public Guid PresentationId  { get; private set; }
    public string ContainerCode { get; private set; }

    /// <summary>Declared capacity (e.g. 50 lbs as written on the sack).</summary>
    public decimal NominalCapacity  { get; private set; }

    /// <summary>Real weighed capacity — may differ from nominal. Null if not weighed.</summary>
    public decimal? ActualCapacity  { get; private set; }

    /// <summary>How much product remains in this container.</summary>
    public decimal CurrentRemaining { get; private set; }

    public ContainerStatus Status   { get; private set; }

    /// <summary>True when this is the container currently being served from.</summary>
    public bool IsActiveSource      { get; private set; }

    public string Notes             { get; private set; }
    public DateTime PurchasedAt     { get; private set; }
    public DateTime? OpenedAt       { get; private set; }
    public DateTime? EmptiedAt      { get; private set; }
    public DateTime CreatedAt       { get; private set; }
    public DateTime UpdatedAt       { get; private set; }

    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public static OperationResult<StockContainer, DomainError> Create(
        Guid tenantId,
        Guid presentationId,
        string containerCode,
        decimal nominalCapacity,
        decimal? actualCapacity,
        DateTime purchasedAt,
        string? notes)
    {
        if (tenantId == Guid.Empty)
            return OperationResult<StockContainer, DomainError>.Bad(DomainError.Validation("container.tenant_required"));

        if (presentationId == Guid.Empty)
            return OperationResult<StockContainer, DomainError>.Bad(DomainError.Validation("container.presentation_required"));

        if (string.IsNullOrWhiteSpace(containerCode))
            return OperationResult<StockContainer, DomainError>.Bad(DomainError.Validation("container.code_required"));

        if (nominalCapacity <= 0)
            return OperationResult<StockContainer, DomainError>.Bad(DomainError.Validation("container.nominal_capacity_must_be_positive"));

        if (actualCapacity is not null && actualCapacity <= 0)
            return OperationResult<StockContainer, DomainError>.Bad(DomainError.Validation("container.actual_capacity_must_be_positive"));

        return OperationResult<StockContainer, DomainError>.Good(new StockContainer(
            tenantId, presentationId, containerCode, nominalCapacity, actualCapacity, purchasedAt, notes));
    }

    /// <summary>Registers the real weighed capacity (can differ from nominal).</summary>
    public OperationResult<Unit, DomainError> SetActualCapacity(decimal actual)
    {
        if (actual <= 0)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("container.actual_capacity_must_be_positive"));

        ActualCapacity   = actual;
        CurrentRemaining = actual;
        UpdatedAt        = DateTime.UtcNow;
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    /// <summary>Opens the container so product can be drawn from it.</summary>
    public OperationResult<Unit, DomainError> Open()
    {
        if (Status != ContainerStatus.Sealed)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("container.must_be_sealed_to_open"));

        Status    = ContainerStatus.Open;
        OpenedAt  = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ContainerStatusChangedDomainEvent(Id, PresentationId, TenantId.Value, Status.Name, DateTime.UtcNow));
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    /// <summary>
    /// Draws a quantity from this container.
    /// When allowOverDraw is true the draw is accepted even if it exceeds remaining
    /// (models weight variation — a sack might have slightly more than declared).
    /// </summary>
    public OperationResult<Unit, DomainError> Draw(decimal amount, bool allowOverDraw = false)
    {
        if (Status != ContainerStatus.Open)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("container.must_be_open_to_draw"));

        if (amount <= 0)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Validation("container.draw_amount_must_be_positive"));

        if (!allowOverDraw && amount > CurrentRemaining)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("container.insufficient_remaining"));

        CurrentRemaining -= amount;
        UpdatedAt         = DateTime.UtcNow;
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    /// <summary>Marks the container as empty (fully consumed).</summary>
    public OperationResult<Unit, DomainError> MarkEmpty()
    {
        if (Status == ContainerStatus.Empty)
            return OperationResult<Unit, DomainError>.Bad(DomainError.Business("container.already_empty"));

        Status       = ContainerStatus.Empty;
        IsActiveSource = false;
        EmptiedAt    = DateTime.UtcNow;
        UpdatedAt    = DateTime.UtcNow;
        AddDomainEvent(new ContainerStatusChangedDomainEvent(Id, PresentationId, TenantId.Value, Status.Name, DateTime.UtcNow));
        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }

    /// <summary>Flags this container as the active source to draw from.</summary>
    public void SetAsActiveSource(bool active)
    {
        IsActiveSource = active;
        UpdatedAt      = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(object @event) => _domainEvents.Add(@event);
}
