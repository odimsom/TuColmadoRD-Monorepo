using System.Reflection;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using TuColmadoRD.Core.Application.DTOs.Sync;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Tests.Phase3;

public class LocalRetentionWorkerTests
{
    [Fact]
    public async Task ExecuteAsync_WhenLocalStorageModeIsFull_DeletesNothing()
    {
        // Arrange
        using var setup = CreateSetup("Full", "7");
        await SeedSalesAndOutboxAsync(setup.DbContext, uploaded: true, oldEnough: true);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        setup.DbContext.Sales.Count().Should().Be(1);
        setup.DbContext.OutboxMessages.Count().Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDefaultRetentionDays_DeletesSalesOlderThan7DaysThatAreUploaded()
    {
        // Arrange
        using var setup = CreateSetup("Compact", null);
        await SeedSalesAndOutboxAsync(setup.DbContext, uploaded: true, oldEnough: true);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        setup.DbContext.Sales.Count().Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotDeleteSalesNotYetUploaded()
    {
        // Arrange
        using var setup = CreateSetup("Compact", null);
        await SeedSalesAndOutboxAsync(setup.DbContext, uploaded: false, oldEnough: true);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        setup.DbContext.Sales.Count().Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesProcessedOutboxMessagesOlderThanRetentionWindow()
    {
        // Arrange
        using var setup = CreateSetup("Compact", null);
        await SeedSalesAndOutboxAsync(setup.DbContext, uploaded: true, oldEnough: true);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        setup.DbContext.OutboxMessages.Count().Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotDeletePendingOutboxMessages()
    {
        // Arrange
        using var setup = CreateSetup("Compact", null);
        await SeedSalesAndOutboxAsync(setup.DbContext, uploaded: false, oldEnough: true);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        setup.DbContext.OutboxMessages.Count().Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRetentionDaysIsCustom_RespectsConfiguredWindow()
    {
        // Arrange
        using var setup = CreateSetup("Compact", "3");
        await SeedSalesAndOutboxAsync(setup.DbContext, uploaded: true, oldEnough: true, ageInDays: 4);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        setup.DbContext.Sales.Count().Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOneDeleteStepFails_ContinuesToNextStep()
    {
        // Arrange
        using var setup = CreateSetup("Compact", null);

        // Outbox processed old enough without corresponding sale payload to keep sales delete step effectively skipped.
        var outbox = new OutboxMessage("SaleCreated", "{\"SaleId\":\"00000000-0000-0000-0000-000000000001\",\"TotalAmount\":0,\"Date\":\"2026-03-01T00:00:00Z\",\"Items\":[]}");
        outbox.MarkAsProcessed();
        SetPrivate(outbox, "ProcessedAt", DateTime.UtcNow.AddDays(-10));

        setup.DbContext.OutboxMessages.Add(outbox);
        await setup.DbContext.SaveChangesAsync();

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        setup.DbContext.OutboxMessages.Count().Should().Be(0);
    }

    private static async Task SeedSalesAndOutboxAsync(TuColmadoDbContext db, bool uploaded, bool oldEnough, int ageInDays = 10)
    {
        var tenantId = TenantIdentifier.Validate(Guid.NewGuid()).Result;
        var sale = Sale.Create(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "RetentionWorker",
            $"RET-{Guid.NewGuid():N}"[..20],
            null).Result;

        var createdAt = oldEnough ? DateTime.UtcNow.AddDays(-ageInDays) : DateTime.UtcNow;
        SetPrivate(sale, "CreatedAt", createdAt);

        db.Sales.Add(sale);

        var payload = new SaleCreatedPayload
        {
            SaleId = sale.Id,
            TotalAmount = 0,
            Date = createdAt,
            Items = []
        };

        var outbox = new OutboxMessage("SaleCreated", System.Text.Json.JsonSerializer.Serialize(payload));

        if (uploaded)
        {
            outbox.MarkAsProcessed();
            SetPrivate(outbox, "ProcessedAt", DateTime.UtcNow.AddDays(-ageInDays));
        }

        db.OutboxMessages.Add(outbox);
        await db.SaveChangesAsync();
    }

    private static void SetPrivate(object target, string propertyName, object value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        property.SetValue(target, value);
    }

    private static RetentionSetup CreateSetup(string localStorageMode, string? retentionDays)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.TenantId.Returns(TenantIdentifier.Validate(Guid.NewGuid()).Result);
        tenantProvider.TerminalId.Returns(Guid.NewGuid());
        tenantProvider.IsPaired.Returns(true);

        var configRepo = Substitute.For<ISystemConfigRepository>();
        configRepo.GetAsync("LocalStorageMode")
            .Returns(OperationResult<string?, DomainError>.Good(localStorageMode));

        if (retentionDays is null)
        {
            configRepo.GetAsync("RetentionDays")
                .Returns(OperationResult<string?, DomainError>.Good(null));
        }
        else
        {
            configRepo.GetAsync("RetentionDays")
                .Returns(OperationResult<string?, DomainError>.Good(retentionDays));
        }

        var services = new ServiceCollection();
        services.AddSingleton(tenantProvider);
        services.AddSingleton(configRepo);
        services.AddDbContext<TuColmadoDbContext>(opt => opt.UseSqlite(connection));

        var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<TuColmadoDbContext>();
        db.Database.EnsureCreated();

        var options = new StaticOptionsMonitor<RetentionOptions>(new RetentionOptions
        {
            RunAtStartup = true,
            RetentionDays = 7
        });

        var worker = new TestableLocalRetentionWorker(provider, options);

        return new RetentionSetup(connection, provider, db, worker);
    }

    private sealed class RetentionSetup : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;

        public RetentionSetup(SqliteConnection connection, ServiceProvider provider, TuColmadoDbContext dbContext, TestableLocalRetentionWorker worker)
        {
            _connection = connection;
            _provider = provider;
            DbContext = dbContext;
            Worker = worker;
        }

        public TuColmadoDbContext DbContext { get; }
        public TestableLocalRetentionWorker Worker { get; }

        public void Dispose()
        {
            DbContext.Dispose();
            _provider.Dispose();
            _connection.Dispose();
        }
    }

    private sealed class TestableLocalRetentionWorker : LocalRetentionWorker
    {
        public TestableLocalRetentionWorker(IServiceProvider serviceProvider, IOptionsMonitor<RetentionOptions> options)
            : base(serviceProvider, options, NullLogger<LocalRetentionWorker>.Instance)
        {
        }

        public async Task RunForAsync(TimeSpan duration)
        {
            using var cts = new CancellationTokenSource(duration);
            try
            {
                await ExecuteAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;
        public T Get(string? name) => value;
        public IDisposable OnChange(Action<T, string?> listener) => Substitute.For<IDisposable>();
    }
}
