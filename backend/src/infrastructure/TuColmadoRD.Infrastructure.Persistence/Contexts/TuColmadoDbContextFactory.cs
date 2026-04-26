using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Infrastructure.Persistence.Contexts;

public sealed class TuColmadoDbContextFactory : IDesignTimeDbContextFactory<TuColmadoDbContext>
{
    public TuColmadoDbContext CreateDbContext(string[] args)
    {
        // For local development / migration generation
        var connectionString = "Host=localhost;Port=54329;Database=TuColmadoDb;Username=postgres;Password=1234";

        var optionsBuilder = new DbContextOptionsBuilder<TuColmadoDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new TuColmadoDbContext(optionsBuilder.Options, new DesignTimeTenantProvider());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public TenantIdentifier TenantId => TenantIdentifier.Empty;
        public Guid TerminalId => Guid.Empty;
        public bool IsPaired => false;
    }
}
