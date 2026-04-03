using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Infrastructure.IOC.ServiceRegistrations;
using TuColmadoRD.Presentation.API.Endpoints.Customers;
using TuColmadoRD.Presentation.API.Endpoints.Expenses;
using TuColmadoRD.Presentation.API.Endpoints.Inventory;
using TuColmadoRD.Presentation.API.Endpoints.Sales;
using TuColmadoRD.Presentation.API.Endpoints.Sales.Shifts;
using TuColmadoRD.Presentation.API.Endpoints.Purchasing;

namespace TuColmadoRD.Infrastructure.Hosts;

public static class CoreApiHostBuilder
{
    public static WebApplication BuildCoreApi(string[] args, bool isLocal = false)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (isLocal)
        {
            builder.Configuration["GatewayOptions:IsLocalMode"] = "true";
        }

        builder.Configuration
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddAuthorization();

        // Services registrations
        builder.Services.AddGlobalServices(builder.Configuration);
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CoreApiHostBuilder).Assembly));

        var app = builder.Build();

        app.UseSwagger();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<SubscriptionGuardMiddleware>();

        app.MapInventoryEndpoints();
        app.MapPurchasingEndpoints();
        app.MapCustomerEndpoints();
        app.MapExpenseEndpoints();
        app.MapSalesEndpoints();
        app.MapShiftEndpoints();

        return app;
    }
}
