using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Audit;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.HumanResources;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Logistics;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Purchasing;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Treasury;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Audit;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Customers;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Fiscal;
using TuColmadoRD.Infrastructure.Persistence.Repositories.HumanResources;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Logistics;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Purchasing;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Treasury;

namespace TuColmadoRD.Infrastructure.Persistence;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        #region Database Context Registration
        var postgresConnection = configuration.GetConnectionString("PostgresSQLConnectionString");
        var sqlServerConnection = configuration.GetConnectionString("SQLServerConnectionString");
        var enableInMemoryFallback = configuration.GetValue<bool>("Persistence:EnableInMemoryFallback");

        services.AddDbContext<TuColmadoDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(postgresConnection))
            {
                options.UseNpgsql(
                    postgresConnection,
                    npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(TuColmadoDbContext).Assembly.FullName)
                );
                return;
            }

            if (!string.IsNullOrWhiteSpace(sqlServerConnection))
            {
                options.UseSqlServer(
                    sqlServerConnection,
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(TuColmadoDbContext).Assembly.FullName)
                );
                return;
            }

            if (enableInMemoryFallback)
            {
                options.UseInMemoryDatabase("TuColmadoRD.Local");
                return;
            }

            throw new InvalidOperationException(
                "Connection strings 'PostgresSQLConnectionString' and 'SQLServerConnectionString' not found."
            );
        });
        #endregion

        // Repositories Configuration
        services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        #region Registration of repositories for each module
        // System
        services.AddTransient<ISystemConfigRepository, SystemConfigRepository>();
        
        // Audit
        services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();

        // Customers
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerAccountRepository, CustomerAccountRepository>();
        services.AddScoped<IDebtTransactionRepository, DebtTransactionRepository>();

        // Fiscal
        services.AddScoped<IFiscalReceiptRepository, FiscalReceiptRepository>();
        services.AddScoped<IFiscalSequenceRepository, FiscalSequenceRepository>();
        services.AddScoped<ITaxRepository, TaxRepository>();

        // HumanResources
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IWorkShiftRepository, WorkShiftRepository>();

        // Inventory
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitConversionRepository, UnitConversionRepository>();
        services.AddScoped<TuColmadoRD.Core.Application.Inventory.Abstractions.IProductRepository, ProductRepository>();
        services.AddScoped<TuColmadoRD.Core.Application.Inventory.Abstractions.IProductReadRepository, ProductReadRepository>();
        services.AddScoped<TuColmadoRD.Core.Application.Inventory.Abstractions.IOutboxRepository, OutboxRepository>();
        services.AddScoped<TuColmadoRD.Core.Application.Inventory.Abstractions.IUnitOfWork, UnitOfWork>();

        // Logistics
        services.AddScoped<IDeliveryOrderRepository, DeliveryOrderRepository>();
        services.AddScoped<IDeliveryPersonRepository, DeliveryPersonRepository>();

        // Purchasing
        services.AddScoped<IPurchaseDetailRepository, PurchaseDetailRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();

        // Sales
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISaleDetailRepository, SaleDetailRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<TuColmadoRD.Core.Application.Sales.Abstractions.ISaleRepository, SaleRepository>();
        services.AddScoped<TuColmadoRD.Core.Application.Sales.Abstractions.ISaleService, TuColmadoRD.Core.Application.Sales.Queries.SaleService>();
        services.AddScoped<TuColmadoRD.Core.Application.Sales.Abstractions.ISaleSequenceService, SaleSequenceService>();
        services.AddScoped<TuColmadoRD.Core.Application.Sales.Abstractions.IShiftRepository, ShiftRepository>();
        services.AddScoped<TuColmadoRD.Core.Application.Sales.Abstractions.IShiftReadRepository, ShiftReadRepository>();
        services.AddScoped<TuColmadoRD.Core.Application.Sales.Abstractions.ICurrentShiftService, CurrentShiftService>();

        // Treasury
        services.AddScoped<ICashBoxRepository, CashBoxRepository>();
        services.AddScoped<ICashDrawerRepository, CashDrawerRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IPettyCashRepository, PettyCashRepository>();
        #endregion

        return services;
    }
}
