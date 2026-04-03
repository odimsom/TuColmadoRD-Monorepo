using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Sales;

/// <summary>
/// Represents a quantity in a sale item. Supports fractional quantities for items sold by weight or volume.
/// </summary>
public sealed record Quantity
{
    /// <summary>
    /// The numeric quantity value.
    /// </summary>
    public decimal Value { get; }

    private Quantity(decimal value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a Quantity from a decimal value. Value must be greater than zero.
    /// </summary>
    /// <param name="value">The quantity value.</param>
    /// <returns>OperationResult with Quantity or DomainError if validation fails.</returns>
    public static OperationResult<Quantity, DomainError> Of(decimal value)
    {
        if (value <= 0)
        {
            return OperationResult<Quantity, DomainError>.Bad(
                DomainError.Validation("quantity.must_be_positive"));
        }

        return OperationResult<Quantity, DomainError>.Good(new Quantity(value));
    }

    /// <summary>
    /// Implicit conversion from Quantity to decimal.
    /// </summary>
    public static implicit operator decimal(Quantity quantity) => quantity.Value;

    /// <summary>
    /// Returns the string representation of the quantity value.
    /// </summary>
    public override string ToString() => Value.ToString();
}
