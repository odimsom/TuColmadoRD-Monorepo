using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.ValueObjects
{
    /// <summary>
    /// Represents a tax rate using a fractional format (0.18 = 18%).
    /// </summary>
    public record TaxRate
    {
        /// <summary>
        /// Fractional tax rate between 0 and 1.
        /// </summary>
        public decimal Rate { get; init; }

        // Backward-compatible alias for existing mappings/usages.
        public decimal Percentage => Rate;

        private TaxRate(decimal rate)
        {
            Rate = rate;
        }

        /// <summary>
        /// Zero tax rate.
        /// </summary>
        public static readonly TaxRate Zero = new(0m);

        /// <summary>
        /// Creates a tax rate if it is within the allowed range.
        /// </summary>
        public static OperationResult<TaxRate, DomainError> Create(decimal rate)
        {
            if (rate < 0m || rate > 1m)
                return OperationResult<TaxRate, DomainError>.Bad(DomainError.Validation("taxrate.out_of_range"));

            return OperationResult<TaxRate, DomainError>.Good(new TaxRate(rate));
        }

        /// <summary>
        /// Calculates tax amount for a base amount.
        /// </summary>
        public OperationResult<Money, DomainError> CalculateTax(Money baseAmount)
        {
            decimal taxAmount = baseAmount.Amount * Rate;
            return Money.FromDecimal(taxAmount);
        }

        /// <summary>
        /// Converts a tax rate to decimal fraction.
        /// </summary>
        public static implicit operator decimal(TaxRate taxRate) => taxRate.Rate;
    }
}
