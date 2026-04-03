using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Sales;

/// <summary>
/// Payment method enumeration value object for sales transactions.
/// </summary>
public sealed class PaymentMethod : Enumeration
{
    /// <summary>
    /// Cash payment.
    /// </summary>
    public static readonly PaymentMethod Cash = new(1, nameof(Cash), "Efectivo");

    /// <summary>
    /// Card payment (debit or credit).
    /// </summary>
    public static readonly PaymentMethod Card = new(2, nameof(Card), "Tarjeta");

    /// <summary>
    /// Bank transfer.
    /// </summary>
    public static readonly PaymentMethod Transfer = new(3, nameof(Transfer), "Transferencia");

    /// <summary>
    /// Credit (owed by customer).
    /// </summary>
    public static readonly PaymentMethod Credit = new(4, nameof(Credit), "Crédito");

    /// <summary>
    /// Display name in Spanish.
    /// </summary>
    public string DisplayName { get; }

    private PaymentMethod(int id, string name, string displayName)
        : base(id, name)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// Converts ID to PaymentMethod enumeration value.
    /// </summary>
    public static OperationResult<PaymentMethod, DomainError> FromId(int id)
    {
        return id switch
        {
            1 => OperationResult<PaymentMethod, DomainError>.Good(Cash),
            2 => OperationResult<PaymentMethod, DomainError>.Good(Card),
            3 => OperationResult<PaymentMethod, DomainError>.Good(Transfer),
            4 => OperationResult<PaymentMethod, DomainError>.Good(Credit),
            _ => OperationResult<PaymentMethod, DomainError>.Bad(
                DomainError.Validation("paymentmethod.unknown_id"))
        };
    }
}
