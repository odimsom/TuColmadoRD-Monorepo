using System.Net;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using TuColmadoRD.Core.Application.DTOs.Sync;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Tests.Shared;

namespace TuColmadoRD.Tests.Phase3;

public class CatalogSyncWorkerTests
{
    [Fact]
    public async Task SyncAsync_WhenNotConnected_DoesNotCallCloudApi()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK, new List<ProductSyncDto>());
        using var setup = CreateSetup(false, fakeHandler, lastCatalogSync: null);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        fakeHandler.CapturedRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task SyncAsync_WhenLastCatalogSyncIsNull_SendsSinceAsMinValue()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK, new List<ProductSyncDto>());
        using var setup = CreateSetup(true, fakeHandler, lastCatalogSync: null);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        fakeHandler.CapturedRequests.Should().HaveCount(1);
        fakeHandler.CapturedRequests.Single().RequestUri!.Query.Should().Contain("since=0001-01-01T00:00:00.0000000");
    }

    [Fact]
    public async Task SyncAsync_WhenLastCatalogSyncExists_SendsCorrectSinceParameter()
    {
        // Arrange
        var since = DateTime.UtcNow.AddHours(-2).ToString("O");
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK, new List<ProductSyncDto>());
        using var setup = CreateSetup(true, fakeHandler, lastCatalogSync: since);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        fakeHandler.CapturedRequests.Should().HaveCount(1);
        fakeHandler.CapturedRequests.Single().RequestUri!.Query.Should().Contain("since=");
        fakeHandler.CapturedRequests.Single().RequestUri!.Query.Should().Contain("tenantId=");
    }

    [Fact]
    public async Task SyncAsync_WhenCloudReturnsProducts_UpsertsExistingProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var payload = new List<ProductSyncDto>
        {
            new(productId, "Refresco Nuevo", 150m, categoryId, true, DateTime.UtcNow)
        };

        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK, payload);
        using var setup = CreateSetup(true, fakeHandler, null);

        var existing = Product.RehydrateForCatalogSync(
            productId,
            setup.TenantProvider.TenantId,
            Guid.NewGuid(),
            "Viejo",
            Money.FromDecimal(0).Result,
            Money.FromDecimal(100).Result,
            TaxRate.Create(0).Result).Result;

        setup.DbContext.Products.Add(existing);
        await setup.DbContext.SaveChangesAsync();

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        setup.DbContext.ChangeTracker.Clear();
        setup.DbContext.Products.Count(p => p.Id == productId).Should().Be(1);
        setup.DbContext.Products.Single(p => p.Id == productId).Name.Should().Be("Refresco Nuevo");
    }

    [Fact]
    public async Task SyncAsync_WhenCloudReturnsProducts_InsertsNewProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var payload = new List<ProductSyncDto>
        {
            new(productId, "Galleta", 50m, Guid.NewGuid(), true, DateTime.UtcNow)
        };

        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK, payload);
        using var setup = CreateSetup(true, fakeHandler, null);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        setup.DbContext.ChangeTracker.Clear();
        setup.DbContext.Products.Any(p => p.Id == productId).Should().BeTrue();
    }

    [Fact]
    public async Task SyncAsync_WhenUpsertSucceeds_UpdatesLastCatalogSyncInSystemConfig()
    {
        // Arrange
        var payload = new List<ProductSyncDto>();
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK, payload);
        using var setup = CreateSetup(true, fakeHandler, null);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        await setup.ConfigRepo.ReceivedWithAnyArgs(1).SetAsync("LastCatalogSync", Arg.Any<string>());
    }

    [Fact]
    public async Task SyncAsync_WhenCloudReturnsError_DoesNotUpdateLastCatalogSync()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, new { error = "boom" });
        using var setup = CreateSetup(true, fakeHandler, null);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        await setup.ConfigRepo.DidNotReceiveWithAnyArgs().SetAsync("LastCatalogSync", Arg.Any<string>());
    }

    [Fact]
    public async Task SyncAsync_WhenCloudReturnsEmptyList_UpdatesLastCatalogSyncAnyway()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK, new List<ProductSyncDto>());
        using var setup = CreateSetup(true, fakeHandler, null);

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(1500));

        // Assert
        await setup.ConfigRepo.ReceivedWithAnyArgs(1).SetAsync("LastCatalogSync", Arg.Any<string>());
    }

    private static CatalogSetup CreateSetup(bool connected, FakeHttpMessageHandler handler, string? lastCatalogSync)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.TenantId.Returns(TenantIdentifier.Validate(Guid.NewGuid()).Result);
        tenantProvider.TerminalId.Returns(Guid.NewGuid());
        tenantProvider.IsPaired.Returns(true);

        var monitor = Substitute.For<IConnectionMonitor>();
        monitor.IsOnline.Returns(connected);

        var configRepo = Substitute.For<ISystemConfigRepository>();
        configRepo.GetAsync("LastCatalogSync")
            .Returns(OperationResult<string?, DomainError>.Good(lastCatalogSync));
        configRepo.SetAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(OperationResult<Unit, DomainError>.Good(Unit.Value));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("CloudSyncAPI").Returns(httpClient);

        var services = new ServiceCollection();
        services.AddSingleton(tenantProvider);
        services.AddSingleton(monitor);
        services.AddSingleton(configRepo);
        services.AddSingleton(factory);
        services.AddDbContext<TuColmadoDbContext>(opt => opt.UseSqlite(connection));

        var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<TuColmadoDbContext>();
        db.Database.EnsureCreated();

        var options = new StaticOptionsMonitor<OutboxOptions>(new OutboxOptions
        {
            CatalogSyncIntervalMinutes = 30,
            CloudSyncBaseUrl = "http://localhost"
        });

        var worker = new TestableCatalogSyncWorker(provider, monitor, options);

        return new CatalogSetup(connection, provider, db, worker, tenantProvider, configRepo);
    }

    private sealed class CatalogSetup : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;

        public CatalogSetup(
            SqliteConnection connection,
            ServiceProvider provider,
            TuColmadoDbContext dbContext,
            TestableCatalogSyncWorker worker,
            ITenantProvider tenantProvider,
            ISystemConfigRepository configRepo)
        {
            _connection = connection;
            _provider = provider;
            DbContext = dbContext;
            Worker = worker;
            TenantProvider = tenantProvider;
            ConfigRepo = configRepo;
        }

        public TuColmadoDbContext DbContext { get; }
        public TestableCatalogSyncWorker Worker { get; }
        public ITenantProvider TenantProvider { get; }
        public ISystemConfigRepository ConfigRepo { get; }

        public void Dispose()
        {
            DbContext.Dispose();
            _provider.Dispose();
            _connection.Dispose();
        }
    }

    private sealed class TestableCatalogSyncWorker : CatalogSyncWorker
    {
        public TestableCatalogSyncWorker(
            IServiceProvider serviceProvider,
            IConnectionMonitor connectionMonitor,
            IOptionsMonitor<OutboxOptions> outboxOptions)
            : base(serviceProvider, connectionMonitor, outboxOptions, NullLogger<CatalogSyncWorker>.Instance)
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
