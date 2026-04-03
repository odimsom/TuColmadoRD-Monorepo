using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Tests.Shared;

public sealed class InMemoryDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ITenantProvider _tenantProvider;

    public InMemoryDbContextFactory(Guid? tenantId = null)
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var resolvedTenantId = tenantId ?? Guid.NewGuid();
        _tenantProvider = new TestTenantProvider(resolvedTenantId, Guid.NewGuid(), true);
    }

    public TuColmadoDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TuColmadoDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new TuColmadoDbContext(options, _tenantProvider);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private sealed class TestTenantProvider : ITenantProvider
    {
        public TestTenantProvider(Guid tenantId, Guid terminalId, bool isPaired)
        {
            TenantId = TenantIdentifier.Validate(tenantId).Result;
            TerminalId = terminalId;
            IsPaired = isPaired;
        }

        public TenantIdentifier TenantId { get; }
        public Guid TerminalId { get; }
        public bool IsPaired { get; }
    }
}
