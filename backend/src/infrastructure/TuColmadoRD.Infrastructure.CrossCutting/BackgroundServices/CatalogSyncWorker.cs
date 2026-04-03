using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuColmadoRD.Core.Application.DTOs.Sync;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices;

public class CatalogSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionMonitor _connectionMonitor;
    private readonly IOptionsMonitor<OutboxOptions> _outboxOptions;
    private readonly ILogger<CatalogSyncWorker> _logger;

    public CatalogSyncWorker(
        IServiceProvider serviceProvider,
        IConnectionMonitor connectionMonitor,
        IOptionsMonitor<OutboxOptions> outboxOptions,
        ILogger<CatalogSyncWorker> logger)
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
            _ = RunCatalogSyncAsync(CancellationToken.None);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunCatalogSyncAsync(stoppingToken);

        var interval = TimeSpan.FromMinutes(_outboxOptions.CurrentValue.CatalogSyncIntervalMinutes);
        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCatalogSyncAsync(stoppingToken);
        }
    }

    private async Task RunCatalogSyncAsync(CancellationToken stoppingToken)
    {
        if (!_connectionMonitor.IsOnline)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        var configRepo = scope.ServiceProvider.GetRequiredService<ISystemConfigRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TuColmadoDbContext>();

        var syncResult = await ExecuteSyncAsync(httpClientFactory, tenantProvider, configRepo, dbContext, stoppingToken);
        if (!syncResult.IsGood && syncResult.TryGetError(out var error) && error is not null)
        {
            _logger.LogWarning("Catalog sync failed: {Code} - {Message}", error.Code, error.Message);
        }
    }

    private async Task<OperationResult<Unit, DomainError>> ExecuteSyncAsync(
        IHttpClientFactory httpClientFactory,
        ITenantProvider tenantProvider,
        ISystemConfigRepository configRepo,
        TuColmadoDbContext dbContext,
        CancellationToken ct)
    {
        try
        {
            var lastSyncResult = await configRepo.GetAsync("LastCatalogSync");
            string? lastSyncRaw = null;
            if (lastSyncResult.IsGood)
            {
                lastSyncResult.TryGetResult(out lastSyncRaw);
            }

            var sinceUtc = DateTime.TryParse(lastSyncRaw, out var parsed)
                ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
                : DateTime.MinValue;

            var client = httpClientFactory.CreateClient("CloudSyncAPI");
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/sync/catalog?since={sinceUtc:O}&tenantId={tenantProvider.TenantId}");
            request.Headers.Add("X-Terminal-Id", tenantProvider.TerminalId.ToString());

            var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                return OperationResult<Unit, DomainError>.Bad(
                    new SyncError("CatalogHttpFailure", $"cloud_rejected:{(int)response.StatusCode}"));
            }

            var products = await response.Content.ReadFromJsonAsync<List<ProductSyncDto>>(cancellationToken: ct);
            products ??= [];

            using var tx = await dbContext.Database.BeginTransactionAsync(ct);
            foreach (var dto in products)
            {
                var upsertResult = await UpsertProductAsync(dbContext, tenantProvider.TenantId, dto, ct);
                if (!upsertResult.IsGood)
                {
                    await tx.RollbackAsync(ct);
                    return upsertResult;
                }
            }

            await dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            var setResult = await configRepo.SetAsync("LastCatalogSync", DateTime.UtcNow.ToString("O"));
            if (!setResult.IsGood)
            {
                return OperationResult<Unit, DomainError>.Bad(
                    new SyncError("CatalogSyncCheckpointFailed", "failed_to_update_last_catalog_sync"));
            }

            _logger.LogInformation("Catalog sync successful. Products synchronized: {Count}", products.Count);
            return OperationResult<Unit, DomainError>.Good(new Unit());
        }
        catch (Exception ex)
        {
            return OperationResult<Unit, DomainError>.Bad(new SyncError("CatalogSyncUnexpectedError", ex.Message));
        }
    }

    private static async Task<OperationResult<Unit, DomainError>> UpsertProductAsync(
        TuColmadoDbContext dbContext,
        TenantIdentifier tenantId,
        ProductSyncDto dto,
        CancellationToken ct)
    {
        var costResult = Money.FromDecimal(0m);
        var saleResult = Money.FromDecimal(dto.Price);
        var itbisResult = TaxRate.Create(0m);

        if (!costResult.IsGood || !saleResult.IsGood || !itbisResult.IsGood)
        {
            return OperationResult<Unit, DomainError>.Bad(
                new SyncError("ProductMappingFailed", "invalid_product_price_or_tax"));
        }

        var cost = costResult.Result!;
        var sale = saleResult.Result!;
        var itbis = itbisResult.Result!;

        var existing = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId, ct);
        if (existing is null)
        {
            var createResult = Product.RehydrateForCatalogSync(dto.ProductId, tenantId, dto.CategoryId, dto.Name, cost, sale, itbis);
            if (!createResult.IsGood || !createResult.TryGetResult(out var created) || created is null)
            {
                return OperationResult<Unit, DomainError>.Bad(
                    new SyncError("ProductCreateFailed", "failed_to_create_local_product"));
            }
            dbContext.Products.Add(created);

            return OperationResult<Unit, DomainError>.Good(new Unit());
        }

        var updateResult = existing.UpdateFromCatalogSync(dto.CategoryId, dto.Name, cost, sale);
        if (!updateResult.IsGood)
        {
            return OperationResult<Unit, DomainError>.Bad(
                new SyncError("ProductUpdateFailed", "failed_to_update_local_product"));
        }

        return OperationResult<Unit, DomainError>.Good(new Unit());
    }
    
    public override void Dispose()
    {
        _connectionMonitor.ConnectionChanged -= OnConnectionRestored!;
        base.Dispose();
    }
}
