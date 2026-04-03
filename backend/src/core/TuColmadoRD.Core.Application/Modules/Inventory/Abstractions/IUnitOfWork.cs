namespace TuColmadoRD.Core.Application.Inventory.Abstractions;

/// <summary>
/// Unit of work abstraction for atomic commits.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits pending changes atomically.
    /// </summary>
    Task CommitAsync(CancellationToken ct);
}
