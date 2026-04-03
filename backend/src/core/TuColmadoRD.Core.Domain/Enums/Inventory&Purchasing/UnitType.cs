using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing
{
    /// <summary>
    /// Unit type smart enumeration used by inventory entities.
    /// </summary>
    public sealed class UnitType : Enumeration
    {
        public static readonly UnitType Unit = new(1, nameof(Unit));
        public static readonly UnitType Pound = new(2, nameof(Pound));
        public static readonly UnitType Liter = new(3, nameof(Liter));
        public static readonly UnitType Box = new(4, nameof(Box));

        private UnitType(int id, string name) : base(id, name)
        {
        }

        /// <summary>
        /// Creates a unit type from persisted numeric id.
        /// </summary>
        public static OperationResult<UnitType, DomainError> FromId(int id)
        {
            var unitType = id switch
            {
                1 => Unit,
                2 => Pound,
                3 => Liter,
                4 => Box,
                _ => null
            };

            return unitType is null
                ? OperationResult<UnitType, DomainError>.Bad(DomainError.Validation("unittype.unknown_id"))
                : OperationResult<UnitType, DomainError>.Good(unitType);
        }

        public static implicit operator int(UnitType unitType) => unitType.Id;
    }
}
