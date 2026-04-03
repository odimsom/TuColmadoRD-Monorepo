using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using TuColmadoRD.Core.Application.Handlers.Sync;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Core.Application.Interfaces.Sync;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Tests.Phase3;

public class OutboxWorkerTests
{
    private const int WorkerRunWindowMilliseconds = 900;

    [Fact]
    public async Task ExecuteAsync_WhenNotConnected_DoesNotQueryOutbox()
    {
        // Arrange
        using var setup = CreateWorkerSetup(false, _ => OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Good(Unit.Value));

        await using (var seed = setup.CreateDbContext())
        {
            seed.OutboxMessages.Add(new OutboxMessage("SaleCreated", "{}"));
            await seed.SaveChangesAsync();
        }

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(WorkerRunWindowMilliseconds));

        // Assert
        await using var verify = setup.CreateDbContext();
        verify.OutboxMessages.Single().ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenConnected_ProcessesPendingMessagesInCreatedAtOrder()
    {
        // Arrange
        var processedIds = new List<Guid>();
        using var setup = CreateWorkerSetup(true, message =>
        {
            processedIds.Add(message.Id);
            return OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Good(Unit.Value);
        });

        Guid firstId;
        Guid secondId;

        await using (var seed = setup.CreateDbContext())
        {
            var first = new OutboxMessage("SaleCreated", "{}");
            var second = new OutboxMessage("SaleCreated", "{}");

            firstId = first.Id;
            secondId = second.Id;

            seed.OutboxMessages.AddRange(first, second);
            await seed.SaveChangesAsync();

            seed.Entry(first).Property(x => x.CreatedAt).CurrentValue = DateTime.UtcNow.AddMinutes(-2);
            seed.Entry(second).Property(x => x.CreatedAt).CurrentValue = DateTime.UtcNow.AddMinutes(-1);
            await seed.SaveChangesAsync();
        }

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(WorkerRunWindowMilliseconds));

        // Assert
        processedIds.Should().HaveCount(2);
        processedIds[0].Should().Be(firstId);
        processedIds[1].Should().Be(secondId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerSucceeds_SetsProcessedAtAndSaves()
    {
        // Arrange
        using var setup = CreateWorkerSetup(true, _ => OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Good(Unit.Value));

        await using (var seed = setup.CreateDbContext())
        {
            seed.OutboxMessages.Add(new OutboxMessage("SaleCreated", "{}"));
            await seed.SaveChangesAsync();
        }

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(WorkerRunWindowMilliseconds));

        // Assert
        await using var verify = setup.CreateDbContext();
        verify.OutboxMessages.Single().ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsTransientFailure_IncrementsRetryCountAndSetsLastError()
    {
        // Arrange
        using var setup = CreateWorkerSetup(true, _ =>
            OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Bad(new SyncError("TransientFailure", "transient:test")));

        await using (var seed = setup.CreateDbContext())
        {
            seed.OutboxMessages.Add(new OutboxMessage("SaleCreated", "{}"));
            await seed.SaveChangesAsync();
        }

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(WorkerRunWindowMilliseconds));

        // Assert
        await using var verify = setup.CreateDbContext();
        var message = verify.OutboxMessages.Single();
        message.RetryCount.Should().Be(1);
        message.LastError.Should().Contain("transient:test");
        message.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsPermanentFailure_IncrementsRetryCount()
    {
        // Arrange
        using var setup = CreateWorkerSetup(true, _ =>
            OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Bad(new SyncError("PermanentFailure", "cloud_rejected:400")));

        await using (var seed = setup.CreateDbContext())
        {
            seed.OutboxMessages.Add(new OutboxMessage("SaleCreated", "{}"));
            await seed.SaveChangesAsync();
        }

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(WorkerRunWindowMilliseconds));

        // Assert
        await using var verify = setup.CreateDbContext();
        var message = verify.OutboxMessages.Single();
        message.RetryCount.Should().Be(1);
        message.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenRetryCountReachesMaxRetries_MarksAsProcessedWithMaxRetriesError()
    {
        // Arrange
        using var setup = CreateWorkerSetup(true, _ =>
            OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Bad(new SyncError("TransientFailure", "transient:max-retries")), maxRetries: 1);

        await using (var seed = setup.CreateDbContext())
        {
            seed.OutboxMessages.Add(new OutboxMessage("SaleCreated", "{}"));
            await seed.SaveChangesAsync();
        }

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(WorkerRunWindowMilliseconds));

        // Assert
        await using var verify = setup.CreateDbContext();
        var message = verify.OutboxMessages.Single();
        message.ProcessedAt.Should().NotBeNull();
        message.LastError.Should().StartWith("MAX_RETRIES_EXCEEDED");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOneMessageFails_ContinuesProcessingRemainingMessages()
    {
        // Arrange
        var call = 0;
        using var setup = CreateWorkerSetup(true, _ =>
        {
            call++;
            if (call == 1)
            {
                return OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Bad(new SyncError("TransientFailure", "transient:fail-first"));
            }

            return OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Good(Unit.Value);
        });

        await using (var seed = setup.CreateDbContext())
        {
            seed.OutboxMessages.Add(new OutboxMessage("SaleCreated", "{}"));
            seed.OutboxMessages.Add(new OutboxMessage("SaleCreated", "{}"));
            await seed.SaveChangesAsync();
        }

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(WorkerRunWindowMilliseconds));

        // Assert
        await using var verify = setup.CreateDbContext();
        verify.OutboxMessages.Count(m => m.ProcessedAt != null).Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_OnlyProcessesMessagesWhereProcessedAtIsNull()
    {
        // Arrange
        using var setup = CreateWorkerSetup(true, _ => OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Good(Unit.Value));

        await using (var seed = setup.CreateDbContext())
        {
            var processed = new OutboxMessage("SaleCreated", "{}");
            processed.MarkAsProcessed();
            seed.OutboxMessages.Add(processed);
            seed.OutboxMessages.Add(new OutboxMessage("SaleCreated", "{}"));
            await seed.SaveChangesAsync();
        }

        // Act
        await setup.Worker.RunForAsync(TimeSpan.FromMilliseconds(WorkerRunWindowMilliseconds));

        // Assert
        await using var verify = setup.CreateDbContext();
        verify.OutboxMessages.Count(m => m.ProcessedAt != null).Should().Be(2);
    }

    private static WorkerSetup CreateWorkerSetup(
        bool isConnected,
        Func<OutboxMessage, OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>> dispatch,
        int maxRetries = 5)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.TenantId.Returns(TenantIdentifier.Validate(Guid.NewGuid()).Result);
        tenantProvider.TerminalId.Returns(Guid.NewGuid());
        tenantProvider.IsPaired.Returns(true);

        var handler = new LambdaOutboxHandler(dispatch);

        var services = new ServiceCollection();
        services.AddSingleton(tenantProvider);
        services.AddSingleton<IConnectionMonitor>(new FakeConnectionMonitor(isConnected));

        services.AddDbContext<TuColmadoDbContext>(opt => opt.UseSqlite(connection));
        services.AddScoped(_ => handler);
        services.AddKeyedScoped<IOutboxMessageHandler>("SaleCreated", (sp, _) => sp.GetRequiredService<LambdaOutboxHandler>());
        services.AddKeyedScoped<IOutboxMessageHandler>("ExpenseCreated", (sp, _) => sp.GetRequiredService<LambdaOutboxHandler>());
        services.AddScoped<OutboxMessageDispatcher>();

        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TuColmadoDbContext>();
            db.Database.EnsureCreated();
        }

        var options = new StaticOptionsMonitor<OutboxOptions>(new OutboxOptions
        {
            PollingIntervalSeconds = 1,
            BatchSize = 50,
            MaxRetries = maxRetries,
            CloudSyncBaseUrl = "http://localhost"
        });

        var monitor = provider.GetRequiredService<IConnectionMonitor>();
        var worker = new TestableOutboxWorker(provider, monitor, options);

        return new WorkerSetup(provider, connection, worker);
    }

    private sealed class WorkerSetup : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly SqliteConnection _connection;

        public WorkerSetup(ServiceProvider provider, SqliteConnection connection, TestableOutboxWorker worker)
        {
            _provider = provider;
            _connection = connection;
            Worker = worker;
        }

        public TestableOutboxWorker Worker { get; }

        public TuColmadoDbContext CreateDbContext()
        {
            var scope = _provider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<TuColmadoDbContext>();
        }

        public void Dispose()
        {
            _provider.Dispose();
            _connection.Dispose();
        }
    }

    private sealed class TestableOutboxWorker : OutboxWorker
    {
        public TestableOutboxWorker(
            IServiceProvider serviceProvider,
            IConnectionMonitor connectionMonitor,
            IOptionsMonitor<OutboxOptions> options)
            : base(serviceProvider, connectionMonitor, options, NullLogger<OutboxWorker>.Instance)
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

    private sealed class LambdaOutboxHandler(Func<OutboxMessage, OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>> fn)
        : IOutboxMessageHandler
    {
        public Task<OperationResult<Unit, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>> HandleAsync(OutboxMessage message, CancellationToken ct)
        {
            return Task.FromResult(fn(message));
        }
    }

    private sealed class FakeConnectionMonitor(bool isOnline) : IConnectionMonitor
    {
        public bool IsOnline { get; set; } = isOnline;
        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionChanged;

        public OperationResult<bool, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError> CheckStatus()
            => OperationResult<bool, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Good(IsOnline);

        public Task<OperationResult<bool, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>> CheckConnectionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<bool, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError>.Good(IsOnline));

        public Task StartAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }
}
