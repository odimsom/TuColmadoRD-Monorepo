namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;

public interface ISystemConfigRepository
{
    Task<string?> GetLastKnownTimeAsync();
    Task UpdateLastKnownTimeAsync(string newTime);
    
    Task<TuColmadoRD.Core.Domain.Base.Result.OperationResult<string?, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>> GetAsync(string key);
    Task<TuColmadoRD.Core.Domain.Base.Result.OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>> SetAsync(string key, string value);
}
