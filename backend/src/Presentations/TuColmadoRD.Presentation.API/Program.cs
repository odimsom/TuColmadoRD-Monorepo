using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Infrastructure.IOC.ServiceRegistrations;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Presentation.API.Endpoints.Customers;
using TuColmadoRD.Presentation.API.Endpoints.Expenses;
using TuColmadoRD.Presentation.API.Endpoints.Inventory;
using TuColmadoRD.Presentation.API.Endpoints.Purchasing;
using TuColmadoRD.Presentation.API.Endpoints.Sales;
using TuColmadoRD.Presentation.API.Endpoints.Sales.Shifts;
using TuColmadoRD.Presentation.API.Endpoints.Settings;

namespace TuColmadoRD.Presentation.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Any(a => a.Contains("TuColmadoRD.Presentation.API.dll")) || args.Length == 0)
        {
            var app = await CoreApiHostBuilder.BuildCoreApi(args);
            app.Run();
        }
    }
}

public static class CoreApiHostBuilder
{
    public static async Task<WebApplication> BuildCoreApi(string[] args, bool isLocal = false)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (isLocal)
        {
            builder.Configuration["GatewayOptions:IsLocalMode"] = "true";
            builder.Configuration["Persistence:EnableInMemoryFallback"] = "true";
            builder.Configuration["BackgroundWorkers:Enabled"] = "false";
            builder.Configuration["AuthApi:BaseUrl"] = "http://localhost:5300";
        }

        builder.Configuration
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddAuthorization();

        builder.Services.AddGlobalServices(builder.Configuration);
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CoreApiHostBuilder).Assembly));

        var app = builder.Build();

        // Apply EF Core migrations automatically on startup
        if (!isLocal)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                var db = scope.ServiceProvider.GetRequiredService<TuColmadoDbContext>();
                logger.LogInformation("Applying database migrations...");
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying database migrations.");
                throw;
            }
        }

        app.UseSwagger();
        if (isLocal || app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI();
        }

        if (!isLocal)
        {
            app.UseHttpsRedirection();
        }
        app.UseMiddleware<SubscriptionGuardMiddleware>();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        app.MapInventoryEndpoints();
        app.MapPurchasingEndpoints();
        app.MapCustomerEndpoints();
        app.MapExpenseEndpoints();
        app.MapSalesEndpoints();
        app.MapShiftEndpoints();
        app.MapSettingsEndpoints();

        return app;
    }
}
