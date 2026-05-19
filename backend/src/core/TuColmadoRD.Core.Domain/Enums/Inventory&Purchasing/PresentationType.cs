using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

public sealed class PresentationType : Enumeration
{
    /// <summary>Bulk container sold by weight (sacks, jars, bars).</summary>
    public static readonly PresentationType BulkContainer = new(1, nameof(BulkContainer));

    /// <summary>Pre-packaged unit sold as a closed unit (bags, bottles).</summary>
    public static readonly PresentationType PackagedUnit = new(2, nameof(PackagedUnit));

    private PresentationType(int id, string name) : base(id, name) { }

    public static OperationResult<PresentationType, DomainError> FromId(int id)
    {
        var result = id switch
        {
            1 => BulkContainer,
            2 => PackagedUnit,
            _ => null
        };

        return result is null
            ? OperationResult<PresentationType, DomainError>.Bad(DomainError.Validation("presentation_type.unknown_id"))
            : OperationResult<PresentationType, DomainError>.Good(result);
    }

    public static implicit operator int(PresentationType t) => t.Id;
}
