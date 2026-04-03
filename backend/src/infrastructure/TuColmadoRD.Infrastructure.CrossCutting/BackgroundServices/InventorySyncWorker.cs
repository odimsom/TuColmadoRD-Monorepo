using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;

namespace TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices;

public class InventorySyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionMonitor _connectionMonitor;
    private readonly IOptionsMonitor<OutboxOptions> _outboxOptions;
    private readonly ILogger<InventorySyncWorker> _logger;

    public InventorySyncWorker(
        IServiceProvider serviceProvider,
        IConnectionMonitor connectionMonitor,
        IOptionsMonitor<OutboxOptions> outboxOptions,
        ILogger<InventorySyncWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _connectionMonitor = connectionMonitor;
        _outboxOptions = outboxOptions;
        _logger = logger;

        _connectionMonitor.ConnectionChanged += OnConnectionRestored!;
    }

    private void OnConnectionRestored(object sender, ConnectionStatusChangedEventArgs e)
    {
        if (e.IsOnline)
        {
            _ = RunInventorySyncAsync(CancellationToken.None);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunInventorySyncAsync(stoppingToken);

        var interval = TimeSpan.FromMinutes(_outboxOptions.CurrentValue.InventorySyncIntervalMinutes);
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunInventorySyncAsync(stoppingToken);
        }
    }

    private Task RunInventorySyncAsync(CancellationToken ct)
    {
        if (!_connectionMonitor.IsOnline)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("Inventory sync tick executed.");
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _connectionMonitor.ConnectionChanged -= OnConnectionRestored!;
        base.Dispose();
    }
}
