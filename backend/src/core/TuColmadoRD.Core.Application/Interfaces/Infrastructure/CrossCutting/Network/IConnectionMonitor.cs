using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;

public interface IConnectionMonitor : IAsyncDisposable
{
    bool IsOnline { get; }
    event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionChanged;

    OperationResult<bool, DomainError> CheckStatus();

    Task<OperationResult<bool, DomainError>> CheckConnectionAsync(
        CancellationToken cancellationToken = default);

    Task StartAsync(CancellationToken cancellationToken = default);
}

public sealed record ConnectionStatusChangedEventArgs(
    bool IsOnline,
    DateTimeOffset Timestamp,
    string Reason
);