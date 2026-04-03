using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Sales;

/// <summary>
/// Sale status enumeration value object.
/// </summary>
public sealed class SaleStatus : Enumeration
{
    /// <summary>
    /// Sale has been completed and finalized.
    /// </summary>
    public static readonly SaleStatus Completed = new(1, nameof(Completed), "Completada");

    /// <summary>
    /// Sale has been voided/cancelled.
    /// </summary>
    public static readonly SaleStatus Voided = new(2, nameof(Voided), "Anulada");

    /// <summary>
    /// Sale is on hold (partial payment, pending approval, etc.).
    /// </summary>
    public static readonly SaleStatus Held = new(3, nameof(Held), "Suspendida");

    /// <summary>
    /// Display name in Spanish.
    /// </summary>
    public string DisplayName { get; }

    private SaleStatus(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// Converts ID to SaleStatus enumeration value.
    /// </summary>
    public static OperationResult<SaleStatus, DomainError> FromId(int id)
    {
        return id switch
        {
            1 => OperationResult<SaleStatus, DomainError>.Good(Completed),
            2 => OperationResult<SaleStatus, DomainError>.Good(Voided),
            3 => OperationResult<SaleStatus, DomainError>.Good(Held),
            _ => OperationResult<SaleStatus, DomainError>.Bad(
                DomainError.Validation("salestatus.unknown_id"))
        };
    }
}
