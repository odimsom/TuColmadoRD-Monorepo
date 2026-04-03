using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Inventory
{
    public class UnitConversion : ITenantEntity
    {
    private UnitConversion() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid ProductId { get; private set; }

        public UnitOfMeasureEntity FromUnit { get; private set; }
        public UnitOfMeasureEntity ToUnit { get; private set; }
        public decimal Factor { get; private set; }

        private UnitConversion(TenantIdentifier tenantId, Guid productId, UnitOfMeasureEntity fromUnit, UnitOfMeasureEntity toUnit, decimal factor)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            ProductId = productId;
            FromUnit = fromUnit;
            ToUnit = toUnit;
            Factor = factor;
        }

        public static OperationResult<UnitConversion, string> Create(
            TenantIdentifier tenantId,
            Guid productId,
            UnitOfMeasureEntity fromUnit,
            UnitOfMeasureEntity toUnit,
            decimal factor)
        {
            if (factor <= 0) return OperationResult<UnitConversion, string>.Bad("El factor debe ser mayor a cero.");
            if (fromUnit is null) return OperationResult<UnitConversion, string>.Bad("Unidad de origen requerida.");
            if (toUnit is null) return OperationResult<UnitConversion, string>.Bad("Unidad de destino requerida.");
            if (fromUnit.Id == toUnit.Id) return OperationResult<UnitConversion, string>.Bad("No se requiere conversión para la misma unidad.");

            return OperationResult<UnitConversion, string>.Good(
                new UnitConversion(tenantId, productId, fromUnit, toUnit, factor)
            );
        }
    }
}
