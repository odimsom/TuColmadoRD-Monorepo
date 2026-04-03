using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

namespace TuColmadoRD.Core.Domain.Entities.Inventory
{
    public class UnitOfMeasureEntity(UnitOfMeasure id, string name, string abbreviation, bool isFractionable)
    {
        public UnitOfMeasure Id { get; private set; } = id;
        public string Name { get; private set; } = name;
        public string Abbreviation { get; private set; } = abbreviation;
        public bool IsFractionable { get; private set; } = isFractionable;
    }
}
