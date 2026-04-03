using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices;

public class LocalRetentionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<RetentionOptions> _options;
    private readonly ILogger<LocalRetentionWorker> _logger;

    public LocalRetentionWorker(IServiceProvider serviceProvider, IOptionsMonitor<RetentionOptions> options, ILogger<LocalRetentionWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var runOnStartup = _options.CurrentValue.RunAtStartup;
        if (runOnStartup)
        {
            await RunRetentionCycleAsync(stoppingToken);
        }

        var runAtLocalHour = _options.CurrentValue.RunAtLocalHour;
        if (runAtLocalHour is >= 0 and <= 23)
        {
            await WaitUntilNextLocalHourAsync(runAtLocalHour.Value, stoppingToken);
        }

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunRetentionCycleAsync(stoppingToken);
        }
    }

    private static async Task WaitUntilNextLocalHourAsync(int hour, CancellationToken ct)
    {
        var now = DateTime.Now;
        var next = new DateTime(now.Year, now.Month, now.Day, hour, 0, 0, DateTimeKind.Local);
        if (next <= now)
        {
            next = next.AddDays(1);
        }

        var delay = next - now;
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, ct);
        }
    }

    private async Task RunRetentionCycleAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var configRepo = scope.ServiceProvider.GetRequiredService<ISystemConfigRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TuColmadoDbContext>();

        var modeResult = await configRepo.GetAsync("LocalStorageMode");
        if (!modeResult.IsGood)
        {
            _logger.LogWarning("Could not load LocalStorageMode. Continuing retention with defaults.");
        }

        if (modeResult.IsGood && modeResult.TryGetResult(out var mode) && string.Equals(mode, "Full", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Retention skipped (Full mode)");
            return;
        }

        var daysResult = await configRepo.GetAsync("RetentionDays");
        int retentionDays = _options.CurrentValue.RetentionDays;
        if (daysResult.IsGood && daysResult.TryGetResult(out var value) && int.TryParse(value, out int parsedDays))
        {
            retentionDays = parsedDays;
        }

        var threshold = DateTime.UtcNow.AddDays(-retentionDays);

        var salesDeleteResult = await DeleteUploadedSalesAsync(dbContext, threshold, stoppingToken);
        if (salesDeleteResult.IsGood)
        {
            _logger.LogInformation("Retention pruned uploaded sales older than {Threshold}", threshold);
        }
        else if (salesDeleteResult.TryGetError(out var salesError) && salesError is not null)
        {
            _logger.LogError("Retention failed deleting sales: {Error}", salesError.Message);
        }

        var outboxDeleteResult = await DeleteProcessedOutboxAsync(dbContext, threshold, stoppingToken);
        if (outboxDeleteResult.IsGood)
        {
            _logger.LogInformation("Retention pruned processed outbox messages older than {Threshold}", threshold);
        }
        else if (outboxDeleteResult.TryGetError(out var outboxError) && outboxError is not null)
        {
            _logger.LogError("Retention failed deleting outbox messages: {Error}", outboxError.Message);
        }
    }

    private async Task<OperationResult<Unit, DomainError>> DeleteUploadedSalesAsync(
        TuColmadoDbContext dbContext,
        DateTime threshold,
        CancellationToken ct)
    {
        try
        {
            var uploadedSaleIds = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt != null && m.Type == "SaleCreated")
                .Select(m => m.Payload)
                .ToListAsync(ct);

            var saleIds = new HashSet<Guid>();
            foreach (var payload in uploadedSaleIds)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<TuColmadoRD.Core.Application.DTOs.Sync.SaleCreatedPayload>(payload);
                    if (dto is not null)
                    {
                        saleIds.Add(dto.SaleId);
                    }
                }
                catch
                {
                    // Ignore malformed payloads during retention; they remain until manually reviewed.
                }
            }

            if (saleIds.Count == 0)
            {
                _logger.LogInformation("Retention sales step skipped: no uploaded sale IDs found.");
                return OperationResult<Unit, DomainError>.Good(new Unit());
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            var deletedRows = await dbContext.Sales
                .Where(s => s.CreatedAt < threshold && saleIds.Contains(s.Id))
                .ExecuteDeleteAsync(ct);

            await transaction.CommitAsync(ct);
            _logger.LogInformation("Retention deleted {Count} sales", deletedRows);

            return OperationResult<Unit, DomainError>.Good(new Unit());
        }
        catch (Exception ex)
        {
            return OperationResult<Unit, DomainError>.Bad(
                new SyncError("RetentionSalesDeleteFailed", ex.Message));
        }
    }

    private async Task<OperationResult<Unit, DomainError>> DeleteProcessedOutboxAsync(
        TuColmadoDbContext dbContext,
        DateTime threshold,
        CancellationToken ct)
    {
        try
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            var deletedRows = await dbContext.OutboxMessages
                .Where(o => o.ProcessedAt != null && o.ProcessedAt < threshold)
                .ExecuteDeleteAsync(ct);

            await transaction.CommitAsync(ct);
            _logger.LogInformation("Retention deleted {Count} outbox messages", deletedRows);

            return OperationResult<Unit, DomainError>.Good(new Unit());
        }
        catch (Exception ex)
        {
            return OperationResult<Unit, DomainError>.Bad(
                new SyncError("RetentionOutboxDeleteFailed", ex.Message));
        }
    }
}
