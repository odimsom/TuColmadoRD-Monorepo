using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;
using TuColmadoRD.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Infrastructure.CrossCutting.Tenancy;

namespace TuColmadoRD.Infrastructure.CrossCutting;

public static class ServiceRegistration
{
    public static IServiceCollection AddCrossCuttingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConnectionMonitorOptions>(
            opts => configuration.GetSection(ConnectionMonitorOptions.SectionName).Bind(opts));

        services.AddSingleton<IConnectionMonitor, ConnectionMonitor>();
        services.AddHostedService<ConnectionMonitorHostedService>();

        services.AddScoped<TuColmadoRD.Core.Application.Interfaces.Security.ITimeGuard, TimeGuardService>();
        services.AddScoped<TuColmadoRD.Core.Application.Interfaces.Security.ILicenseVerifier, LicenseVerifierService>();
        services.AddSingleton<TuColmadoRD.Core.Application.Interfaces.Security.IClock, SystemClock>();
        services.AddScoped<TuColmadoRD.Core.Application.Interfaces.Services.IEcfSignerService, EcfSignerService>();

        services.AddHttpClient<TuColmadoRD.Core.Application.Interfaces.Services.IEcfGeneratorClient, EcfGeneratorClient>(client => 
        {
            var baseUrl = configuration["EcfGenerator:BaseUrl"] ?? "http://ecf-generator:5000";
            client.BaseAddress = new Uri(baseUrl);
        });

        var applicationAssembly = typeof(TuColmadoRD.Core.Application.Behaviors.ICommandMarker).Assembly;
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(TuColmadoRD.Core.Application.Behaviors.ValidationBehavior<,>));
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(TuColmadoRD.Core.Application.Behaviors.ClockAdvancePipelineBehavior<,>));
        });
        services.AddValidatorsFromAssembly(applicationAssembly);

        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.Configure<RetentionOptions>(configuration.GetSection(RetentionOptions.SectionName));

        services.AddHttpClient("CloudSyncAPI", (sp, client) => 
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<OutboxOptions>>().CurrentValue;
            client.BaseAddress = new Uri(opts.CloudSyncBaseUrl);
        });

        services.AddKeyedScoped<TuColmadoRD.Core.Application.Interfaces.Sync.IOutboxMessageHandler, TuColmadoRD.Infrastructure.CrossCutting.Sync.SaleCreatedOutboxHandler>("SaleCreated");
        services.AddKeyedScoped<TuColmadoRD.Core.Application.Interfaces.Sync.IOutboxMessageHandler, TuColmadoRD.Infrastructure.CrossCutting.Sync.ExpenseCreatedOutboxHandler>("ExpenseCreated");
        services.AddScoped<TuColmadoRD.Core.Application.Handlers.Sync.OutboxMessageDispatcher>();

        var backgroundWorkersEnabled = configuration.GetValue<bool?>("BackgroundWorkers:Enabled") ?? true;
        if (backgroundWorkersEnabled)
        {
            services.AddHostedService<TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices.OutboxWorker>();
            services.AddHostedService<TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices.LocalRetentionWorker>();
            services.AddHostedService<TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices.CatalogSyncWorker>();
            services.AddHostedService<TuColmadoRD.Infrastructure.CrossCutting.BackgroundServices.InventorySyncWorker>();
        }

        return services;
    }

    public static IServiceCollection AddCloudTenancy(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, JwtTenantProvider>();
        return services;
    }

    public static IServiceCollection AddLocalTenancy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LocalDeviceOptions>(configuration.GetSection("LocalDevice"));
        services.AddSingleton<IDeviceIdentityFileStore, DeviceIdentityFileStore>();
        services.AddSingleton<LocalDeviceTenantProvider>();
        services.AddSingleton<ITenantProvider>(sp => sp.GetRequiredService<LocalDeviceTenantProvider>());
        services.AddSingleton<IDeviceIdentityStore>(sp => sp.GetRequiredService<LocalDeviceTenantProvider>());
        services.AddHttpClient<DevicePairingService>(client =>
        {
            var authBaseUrl = configuration["AuthApi:BaseUrl"]
                ?? throw new InvalidOperationException("AuthApi:BaseUrl is not configured.");
            client.BaseAddress = new Uri(authBaseUrl);
        });
        services.AddScoped<IDevicePairingService, DevicePairingService>();
        return services;
    }
}