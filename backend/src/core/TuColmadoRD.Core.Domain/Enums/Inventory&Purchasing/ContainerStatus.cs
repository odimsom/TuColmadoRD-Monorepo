using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

public sealed class ContainerStatus : Enumeration
{
    /// <summary>Container received but not yet opened.</summary>
    public static readonly ContainerStatus Sealed = new(1, nameof(Sealed));

    /// <summary>Container is open and being drawn from.</summary>
    public static readonly ContainerStatus Open = new(2, nameof(Open));

    /// <summary>Container is fully consumed.</summary>
    public static readonly ContainerStatus Empty = new(3, nameof(Empty));

    private ContainerStatus(int id, string name) : base(id, name) { }

    public static OperationResult<ContainerStatus, DomainError> FromId(int id)
    {
        var result = id switch
        {
            1 => Sealed,
            2 => Open,
            3 => Empty,
            _ => null
        };

        return result is null
            ? OperationResult<ContainerStatus, DomainError>.Bad(DomainError.Validation("container_status.unknown_id"))
            : OperationResult<ContainerStatus, DomainError>.Good(result);
    }

    public static implicit operator int(ContainerStatus s) => s.Id;
}
