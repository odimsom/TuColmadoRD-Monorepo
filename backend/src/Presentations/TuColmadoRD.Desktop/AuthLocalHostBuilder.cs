using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using TuColmadoRD.Core.Application.Commands.Tenancy;
using TuColmadoRD.Infrastructure.IOC.ServiceRegistrations;

namespace TuColmadoRD.Desktop;

public static class AuthLocalHostBuilder
{
    public static WebApplication BuildAuthLocal(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration["Persistence:EnableInMemoryFallback"] = "true";
        builder.Configuration["BackgroundWorkers:Enabled"] = "false";
        builder.Configuration["AuthApi:BaseUrl"] = "http://localhost:5300";
        
        // Add services needed by Auth logic (MediatR, Persistence, etc)
        // These are usually registered in GlobalServices
        builder.Services.AddGlobalServices(builder.Configuration);
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(PairDeviceCommand).Assembly,
            typeof(AuthLocalHostBuilder).Assembly
        ));

        var app = builder.Build();

        // 1. POST /auth/register
        app.MapPost("/auth/register", async (HttpContext ctx, IMediator mediator) =>
        {
            // For local mockup, skip full tenant creation if not needed,
            // or use specific commands if they exist.
            // Simplified for MVP as it's a local install.
            var token = GenerateToken("LocalUser", "local-tenant-123");
            return Results.Ok(new 
            { 
                token, 
                tenantId = "local-tenant-123",
                user = new { id = "1", email = "local@tucolmadord.com", firstName = "Admin", lastName = "Local" }
            });
        });

        // 2. POST /auth/login
        app.MapPost("/auth/login", async (HttpContext ctx, IMediator mediator) =>
        {
            var token = GenerateToken("LocalUser", "local-tenant-123");
            return Results.Ok(new 
            { 
                token, 
                tenantId = "local-tenant-123",
                user = new { id = "1", email = "local@tucolmadord.com", firstName = "Admin", lastName = "Local" }
            });
        });

        // 3. POST /pair-device
        app.MapPost("/pair-device", async (HttpContext ctx, IMediator mediator) =>
        {
            // Example body: { email, password, deviceName }
            using var reader = new StreamReader(ctx.Request.Body);
            var body = await reader.ReadToEndAsync();
            var data = System.Text.Json.JsonSerializer.Deserialize<PairDeviceRequest>(body);

            if (data == null) return Results.BadRequest();

            var command = new PairDeviceCommand(data.Email, data.Password, data.DeviceName);
            var result = await mediator.Send(command);

            if (result.IsGood)
            {
                return Results.Ok(result.Result);
            }
            return Results.BadRequest(new { message = result.Error.ToString() });
        });

        return app;
    }

    private static string GenerateToken(string username, string tenantId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("dominican-street-premium-secret-key-2026");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("tenant_id", tenantId),
                new Claim("terminal_id", "00000000-0000-0000-0000-000000000000")
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public class PairDeviceRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string DeviceName { get; set; } = "";
    }
}
