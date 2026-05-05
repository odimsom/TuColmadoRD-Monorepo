using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Entities.Audit;
using TuColmadoRD.Core.Domain.Entities.System;
using System.Reflection;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Entities.Fiscal;
using TuColmadoRD.Core.Domain.Entities.HumanResources;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Entities.Logistics;
using TuColmadoRD.Core.Domain.Entities.Purchasing;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.Treasury;

namespace TuColmadoRD.Infrastructure.Persistence.Contexts;

public class TuColmadoDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public Guid CurrentTenantId => _tenantProvider.TenantId?.Value ?? Guid.Empty;

    public TuColmadoDbContext(DbContextOptions<TuColmadoDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<TenantProfile> TenantProfiles => Set<TenantProfile>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAccount> CustomerAccounts => Set<CustomerAccount>();
    public DbSet<DebtTransaction> DebtTransactions => Set<DebtTransaction>();

    public DbSet<FiscalSequence> FiscalSequences => Set<FiscalSequence>();
    public DbSet<FiscalReceipt> FiscalReceipts => Set<FiscalReceipt>();
    public DbSet<NcfAnnulmentLog> NcfAnnulmentLogs => Set<NcfAnnulmentLog>();
    public DbSet<Tax> Taxes => Set<Tax>();

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<UnitConversion> UnitConversions => Set<UnitConversion>();
    public DbSet<UnitOfMeasureEntity> UnitOfMeasures => Set<UnitOfMeasureEntity>();

    public DbSet<DeliveryOrder> DeliveryOrders => Set<DeliveryOrder>();
    public DbSet<DeliveryPerson> DeliveryPersons => Set<DeliveryPerson>();

    public DbSet<PurchaseDetail> PurchaseDetails => Set<PurchaseDetail>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
    public DbSet<Shift> Shifts => Set<Shift>();

    public DbSet<CashBox> CashBoxes => Set<CashBox>();
    public DbSet<CashDrawer> CashDrawers => Set<CashDrawer>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<PettyCash> PettyCashes => Set<PettyCash>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType is null || entityType.IsOwned())
                continue;

            var ns = clrType.Namespace;
            if (!string.IsNullOrEmpty(ns) && ns.Contains("Entities."))
            {
                var schemaName = ns.Split('.').Last();
                var tableName = entityType.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                    entityType.SetSchema(schemaName);
            }

            if (typeof(ITenantScoped).IsAssignableFrom(clrType))
                ApplyTenantFilter(modelBuilder, clrType);
        }
    }

    private void ApplyTenantFilter(ModelBuilder modelBuilder, Type entityType)
    {
        var method = typeof(TuColmadoDbContext)
            .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(entityType);

        method.Invoke(this, [modelBuilder]);
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    }
}
