using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TuColmadoRD.Infrastructure.CrossCutting;
using TuColmadoRD.Infrastructure.IOC.ServiceRegistrations;
using TuColmadoRD.Presentation.API.Endpoints.Customers;
using TuColmadoRD.Presentation.API.Endpoints.Expenses;
using TuColmadoRD.Presentation.API.Endpoints.Inventory;
using TuColmadoRD.Presentation.API.Endpoints.Purchasing;
using TuColmadoRD.Presentation.API.Endpoints.Sales;
using TuColmadoRD.Presentation.API.Endpoints.Sales.Shifts;
using TuColmadoRD.Presentation.API.Endpoints.Settings;
using TuColmadoRD.Presentation.API.Endpoints.Logistics;

namespace TuColmadoRD.Presentation.API;

public class Program
{
    public static void Main(string[] args)
    {
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

        var jwtSecret = builder.Configuration["JwtSettings:Secret"]
            ?? builder.Configuration["GatewayOptions:JwtSecret"]
            ?? "dominican-street-premium-secret-key-2026";

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddAuthorization();

        builder.Services.AddGlobalServices(builder.Configuration);
        // Cloud API uses JWT-based tenant resolution, not device file
        builder.Services.AddCloudTenancy();
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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        app.MapInventoryEndpoints();
        app.MapPurchasingEndpoints();
        app.MapCustomerEndpoints();
        app.MapExpenseEndpoints();
        app.MapSalesEndpoints();
        app.MapShiftEndpoints();
        app.MapSettingsEndpoints();
        app.MapDeliveryEndpoints();

        return app;
    }
}
