namespace TuColmadoRD.Core.Domain.Base;

public interface ITenantScoped
{
    Guid TenantId { get; }
}
