using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Infrastructure.IOC.ServiceRegistrations;
using TuColmadoRD.Presentation.API.Endpoints.Customers;
using TuColmadoRD.Presentation.API.Endpoints.Expenses;
using TuColmadoRD.Presentation.API.Endpoints.Inventory;
using TuColmadoRD.Presentation.API.Endpoints.Purchasing;
using TuColmadoRD.Presentation.API.Endpoints.Sales;
using TuColmadoRD.Presentation.API.Endpoints.Sales.Shifts;

namespace TuColmadoRD.Presentation.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Entry point for standalone running
        if (args.Any(a => a.Contains("TuColmadoRD.Presentation.API.dll")) || args.Length == 0)
        {
            CoreApiHostBuilder.BuildCoreApi(args).Run();
        }
    }
}

public static class CoreApiHostBuilder
{
    public static WebApplication BuildCoreApi(string[] args, bool isLocal = false)
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

        return app;
    }
}
