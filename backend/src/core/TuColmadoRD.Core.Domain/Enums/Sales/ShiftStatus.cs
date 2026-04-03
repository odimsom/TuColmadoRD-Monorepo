using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Enums.Sales;

public sealed class ShiftStatus : Enumeration
{
    public static readonly ShiftStatus Open = new(1, nameof(Open));
    public static readonly ShiftStatus Closed = new(2, nameof(Closed));

    private ShiftStatus(int id, string name) : base(id, name)
    {
    }

    public static OperationResult<ShiftStatus, DomainError> FromId(int id)
    {
        var status = id switch
        {
            1 => Open,
            2 => Closed,
            _ => null
        };

        return status is null
            ? OperationResult<ShiftStatus, DomainError>.Bad(DomainError.Validation("shiftstatus.unknown_id"))
            : OperationResult<ShiftStatus, DomainError>.Good(status);
    }

    public static implicit operator int(ShiftStatus status) => status.Id;
}
