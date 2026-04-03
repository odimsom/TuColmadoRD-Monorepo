using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    /// <summary>
    /// Represents a monetary amount in DOP.
    /// </summary>
    public record Money
    {
        /// <summary>
        /// Decimal amount.
        /// </summary>
        public decimal Amount { get; init; }

        private Money(decimal amount)
        {
            Amount = amount;
        }

        /// <summary>
        /// Creates a money value object from decimal.
        /// </summary>
        public static OperationResult<Money, DomainError> FromDecimal(decimal amount)
        {
            return amount < 0
                ? OperationResult<Money, DomainError>.Bad(DomainError.Validation("money.negative_value"))
                : OperationResult<Money, DomainError>.Good(new Money(amount));
        }

        /// <summary>
        /// Zero constant amount.
        /// </summary>
        public static Money Zero
            => new(0);

        /// <summary>
        /// Adds two money values.
        /// </summary>
        public static Money operator +(Money a, Money b)
            => new(a.Amount + b.Amount);

        /// <summary>
        /// Subtracts one amount from another, returning a validation error if the result is negative.
        /// </summary>
        public static OperationResult<Money, DomainError> operator -(Money a, Money b)
            => a.Amount < b.Amount
                ? OperationResult<Money, DomainError>.Bad(DomainError.Validation("money.insufficient_amount"))
                : OperationResult<Money, DomainError>.Good(new Money(a.Amount - b.Amount));

        /// <summary>
        /// Converts money to decimal amount.
        /// </summary>
        public static implicit operator decimal(Money money) => money.Amount;

        override public string ToString()
            => $"RD$ {Amount}";
    }
}
