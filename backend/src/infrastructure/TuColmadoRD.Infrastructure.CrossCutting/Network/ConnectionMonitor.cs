using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using System.Threading.Channels;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;

namespace TuColmadoRD.Infrastructure.CrossCutting.Network;

public sealed class ConnectionMonitor : IConnectionMonitor
{
    private volatile bool _isOnline;
    private int _checkRunningFlag;
    private int _consecutiveFailures;

    private readonly Channel<ConnectionStatusChangedEventArgs> _eventChannel;
    private readonly ConnectionMonitorOptions _options;
    private readonly ILogger<ConnectionMonitor> _logger;
    private readonly SemaphoreSlim _updateLock = new(1, 1);

    private CancellationTokenSource? _cts;
    private Task? _dispatcherTask;
    private Task? _timerTask;

    public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionChanged;
    public bool IsOnline => _isOnline;

    public ConnectionMonitor(
        IOptions<ConnectionMonitorOptions> options,
        ILogger<ConnectionMonitor> logger)
    {
        _options = options.Value;
        _logger = logger;
        _isOnline = NetworkInterface.GetIsNetworkAvailable();
        _eventChannel = Channel.CreateBounded<ConnectionStatusChangedEventArgs>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        _dispatcherTask = RunEventDispatcherAsync(_cts.Token);
        _timerTask = RunPeriodicCheckAsync(_cts.Token);
        _ = Task.Run(() => SafeVerifyAndUpdateAsync("startup", _cts.Token), _cts.Token);
        await Task.CompletedTask;
    }

    public OperationResult<bool, DomainError> CheckStatus() =>
        _isOnline
            ? OperationResult<bool, DomainError>.Good(true)
            : OperationResult<bool, DomainError>.Bad(ConnectionError.Offline);

    public async Task<OperationResult<bool, DomainError>> CheckConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = _options.PingEndpoints
                .Select(ep => PingEndpointAsync(ep, cancellationToken));

            bool[] results = await Task.WhenAll(tasks);
            int successCount = results.Count(r => r);
            bool hasInternet = successCount > results.Length / 2;

            _logger.LogDebug(
                "Ping results: {Success}/{Total} endpoints responded",
                successCount, results.Length);

            return hasInternet
                ? OperationResult<bool, DomainError>.Good(true)
                : OperationResult<bool, DomainError>.Bad(ConnectionError.Offline);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<bool, DomainError>.Bad(ConnectionError.Timeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during connectivity check");
            return OperationResult<bool, DomainError>.Bad(
                new ConnectionError($"Error inesperado: {ex.Message}"));
        }
    }

    private async Task RunPeriodicCheckAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(_options.CheckInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
                await SafeVerifyAndUpdateAsync("periodic", ct);
        }
        catch (OperationCanceledException) { }
    }

    private async Task SafeVerifyAndUpdateAsync(string reason, CancellationToken ct)
    {
        if (Interlocked.CompareExchange(ref _checkRunningFlag, 1, 0) != 0)
        {
            _logger.LogDebug("Check skipped (already running). Reason: {Reason}", reason);
            return;
        }

        try
        {
            if (_consecutiveFailures >= _options.CircuitBreakerThreshold)
            {
                _logger.LogWarning(
                    "Circuit breaker OPEN. Consecutive failures: {Count}",
                    _consecutiveFailures);
                await Task.Delay(_options.CircuitBreakerCooldown, ct);
            }

            var result = await CheckConnectionAsync(ct);

            result.Match(
                onGood: _ =>
                {
                    Interlocked.Exchange(ref _consecutiveFailures, 0);
                    return true;
                },
                onBad: _ =>
                {
                    Interlocked.Increment(ref _consecutiveFailures);
                    return false;
                });

            await UpdateStateAsync(result.IsGood, reason, ct);
        }
        finally
        {
            Interlocked.Exchange(ref _checkRunningFlag, 0);
        }
    }

    private async Task UpdateStateAsync(bool isNowOnline, string reason, CancellationToken ct)
    {
        await _updateLock.WaitAsync(ct);
        try
        {
            if (_isOnline == isNowOnline) return;

            _isOnline = isNowOnline;
            _logger.LogInformation(
                "Connection state changed → {State} (reason: {Reason})",
                isNowOnline ? "ONLINE" : "OFFLINE", reason);

            _eventChannel.Writer.TryWrite(
                new ConnectionStatusChangedEventArgs(isNowOnline, DateTimeOffset.UtcNow, reason));
        }
        finally
        {
            _updateLock.Release();
        }
    }

    private async Task RunEventDispatcherAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var args in _eventChannel.Reader.ReadAllAsync(ct))
            {
                await Task.Delay(_options.EventDebounce, ct);

                if (_isOnline != args.IsOnline)
                {
                    _logger.LogDebug("State changed during debounce, skipping stale event");
                    continue;
                }

                try
                {
                    ConnectionChanged?.Invoke(this, args);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in ConnectionChanged subscriber");
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e) =>
        _ = Task.Run(() => SafeVerifyAndUpdateAsync("nic-change", _cts?.Token ?? default));

    private async Task<bool> PingEndpointAsync(string host, CancellationToken ct)
    {
        try
        {
            using var ping = new Ping();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(_options.PingTimeout);

            var reply = await ping.SendPingAsync(
                host,
                (int)_options.PingTimeout.TotalMilliseconds);

            return reply.Status == IPStatus.Success;
        }
        catch (OperationCanceledException) { return false; }
        catch (PingException ex)
        {
            _logger.LogDebug(ex, "Ping failed for host {Host}", host);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error pinging {Host}", host);
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
        _cts?.Cancel();
        _eventChannel.Writer.TryComplete();

        if (_timerTask is not null) await _timerTask.ConfigureAwait(false);
        if (_dispatcherTask is not null) await _dispatcherTask.ConfigureAwait(false);

        _updateLock.Dispose();
        _cts?.Dispose();
    }
}