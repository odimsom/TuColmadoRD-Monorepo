using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using TuColmadoRD.Core.Application.Handlers.Sync;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices;

public class OutboxWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionMonitor _connectionMonitor;
    private readonly IOptionsMonitor<OutboxOptions> _options;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(IServiceProvider serviceProvider, IConnectionMonitor connectionMonitor, IOptionsMonitor<OutboxOptions> options, ILogger<OutboxWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _connectionMonitor = connectionMonitor;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _options.CurrentValue;

            try
            {
                if (!_connectionMonitor.IsOnline)
                {
                    await Task.Delay(TimeSpan.FromSeconds(options.PollingIntervalSeconds), stoppingToken);
                    continue;
                }

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TuColmadoDbContext>();

                List<long> pendingMessageIds;
                
                try
                {
                    pendingMessageIds = await dbContext.OutboxMessages
                        .Where(m => m.ProcessedAt == null)
                        .OrderBy(m => m.CreatedAt)
                        .Take(options.BatchSize)
                        .Select(m => m.Id)
                        .ToListAsync(stoppingToken);
                }
                catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01") // Table doesn't exist
                {
                    _logger.LogWarning("OutboxMessages table not found. Database migrations may still be running...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                if (!pendingMessageIds.Any())
                {
                    await Task.Delay(TimeSpan.FromSeconds(options.PollingIntervalSeconds), stoppingToken);
                    continue;
                }

                foreach (var messageId in pendingMessageIds)
                {
                    try
                    {
                        using var messageScope = _serviceProvider.CreateScope();
                        var messageDbContext = messageScope.ServiceProvider.GetRequiredService<TuColmadoDbContext>();
                        var dispatcher = messageScope.ServiceProvider.GetRequiredService<OutboxMessageDispatcher>();

                        using var transaction = await messageDbContext.Database.BeginTransactionAsync(stoppingToken);

                        var message = await messageDbContext.OutboxMessages
                            .FirstOrDefaultAsync(m => m.Id == messageId, stoppingToken);

                        if (message is null || message.ProcessedAt is not null)
                        {
                            await transaction.CommitAsync(stoppingToken);
                            continue;
                        }

                        var dispatchResult = await dispatcher.DispatchAsync(message, stoppingToken);

                        if (dispatchResult.IsGood)
                        {
                            message.MarkAsProcessed();
                        }
                        else if (dispatchResult.TryGetError(out var error) && error is not null)
                        {
                            ProcessFailure(message, error, options.MaxRetries);
                        }

                        await messageDbContext.SaveChangesAsync(stoppingToken);
                        await transaction.CommitAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error processing OutboxMessage {MessageId}", messageId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in OutboxWorker");
            }

            await Task.Delay(TimeSpan.FromSeconds(options.PollingIntervalSeconds), stoppingToken);
        }
    }

    private void ProcessFailure(TuColmadoRD.Core.Domain.Entities.System.OutboxMessage message, DomainError error, int maxRetries)
    {
        if (error.Code == "PermanentFailure" || error.Code == "unknown_outbox_type" || error.Message.StartsWith("cloud_rejected:", StringComparison.OrdinalIgnoreCase))
        {
            message.RecordTransientFailure(error.Message);
            message.RecordPermanentFailure(error.Message);
            _logger.LogCritical("Outbox permanent failure {MessageId}: {Error}", message.Id, error.Message);
            return;
        }

        message.RecordTransientFailure(error.Message);

        if (message.RetryCount >= maxRetries)
        {
            message.RecordPermanentFailure(error.Message);
            _logger.LogCritical("Outbox max retries exceeded {MessageId}: {Error}", message.Id, error.Message);
        }
    }
}
