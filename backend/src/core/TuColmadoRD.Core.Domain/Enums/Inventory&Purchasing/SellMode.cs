using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

public sealed class SellMode : Enumeration
{
    /// <summary>Sold as a closed unit (1 bag, 1 bottle).</summary>
    public static readonly SellMode ByUnit = new(1, nameof(ByUnit));

    /// <summary>Sold by weight/volume drawn from an open bulk container.</summary>
    public static readonly SellMode ByWeight = new(2, nameof(ByWeight));

    /// <summary>Sold as a whole container (e.g. a 10-lb portion-sack).</summary>
    public static readonly SellMode ByContainer = new(3, nameof(ByContainer));

    private SellMode(int id, string name) : base(id, name) { }

    public static OperationResult<SellMode, DomainError> FromId(int id)
    {
        var result = id switch
        {
            1 => ByUnit,
            2 => ByWeight,
            3 => ByContainer,
            _ => null
        };

        return result is null
            ? OperationResult<SellMode, DomainError>.Bad(DomainError.Validation("sell_mode.unknown_id"))
            : OperationResult<SellMode, DomainError>.Good(result);
    }

    public static implicit operator int(SellMode s) => s.Id;
}
