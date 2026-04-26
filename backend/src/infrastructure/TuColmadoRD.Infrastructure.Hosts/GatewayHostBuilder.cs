using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TuColmadoRD.ApiGateway.Middlewares;

namespace TuColmadoRD.Infrastructure.Hosts;

public class GatewayOptions
{
    public bool IsLocalMode { get; set; }
    public string AuthApiUrl { get; set; } = "http://localhost:3000";
    public string CoreApiUrl { get; set; } = "http://localhost:5000";
    public string JwtSecret { get; set; } = "dominican-street-premium-secret-key-2026";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

public static class GatewayHostBuilder
{
    public static WebApplication BuildGateway(string[] args, GatewayOptions? options = null)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Load Configuration
        var configOptions = builder.Configuration.GetSection("GatewayOptions").Get<GatewayOptions>() ?? new GatewayOptions();
        var isLocal = options?.IsLocalMode ?? configOptions.IsLocalMode;
        var authApiUrl = options?.AuthApiUrl ?? configOptions.AuthApiUrl;
        var coreApiUrl = options?.CoreApiUrl ?? configOptions.CoreApiUrl;
        var jwtSecret = options?.JwtSecret ?? configOptions.JwtSecret;
        var allowedOrigins = options?.AllowedOrigins ?? configOptions.AllowedOrigins;

        // 2. Register Services
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient("AuthClient", client => client.BaseAddress = new Uri(authApiUrl));
        builder.Services.AddHttpClient("CoreClient", client => client.BaseAddress = new Uri(coreApiUrl));

        // 3. Configure Authentication
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

        builder.Services.AddAuthorization();

        // 4. Configure CORS
        builder.Services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // 5. Build Middlewares
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<IdempotencyMiddleware>();

        // 6. Endpoints

        // AUTH GROUP
        var authGroup = app.MapGroup("/gateway/auth");

        authGroup.MapPost("/register", async (HttpContext ctx, IHttpClientFactory factory) => 
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/api/auth/register"));

        authGroup.MapPost("/login", async (HttpContext ctx, IHttpClientFactory factory) => 
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/api/auth/login"));

        // DEVICE PAIRING (Requires Auth)
        app.MapPost("/gateway/devices/pair", async (HttpContext ctx, IHttpClientFactory factory) => 
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/api/auth/pair-device"))
            .RequireAuthorization();

        // GENERIC API PROXY (Requires Auth)
        app.Map("/gateway/{**path}", async (string path, HttpContext ctx, IHttpClientFactory factory) => 
        {
            return await ProxyRequest(ctx, factory.CreateClient("CoreClient"), $"/{path}");
        }).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> ProxyRequest(HttpContext ctx, HttpClient client, string targetPath)
    {
        var request = ctx.Request;
        var targetUri = new Uri(client.BaseAddress!, targetPath + request.QueryString);
        
        var proxyRequest = new HttpRequestMessage(new HttpMethod(request.Method), targetUri);

        if (request.ContentLength > 0 || request.HasJsonContentType())
        {
            using var requestBodyBuffer = new MemoryStream();
            await request.Body.CopyToAsync(requestBodyBuffer);
            proxyRequest.Content = new ByteArrayContent(requestBodyBuffer.ToArray());

            if (request.ContentType != null)
            {
                proxyRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(request.ContentType);
            }
        }

        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) continue;
            
            if (!proxyRequest.Headers.TryAddWithoutValidation(header.Key, (IEnumerable<string?>)header.Value) && proxyRequest.Content != null)
            {
                proxyRequest.Content.Headers.TryAddWithoutValidation(header.Key, (IEnumerable<string?>)header.Value);
            }
        }

        try
        {
            var response = await client.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead);
            
            ctx.Response.StatusCode = (int)response.StatusCode;
            return Microsoft.AspNetCore.Http.Results.Stream(
                await response.Content.ReadAsStreamAsync(),
                response.Content.Headers.ContentType?.ToString() ?? "application/json"
            );
        }
        catch (Exception ex)
        {
            return Microsoft.AspNetCore.Http.Results.Json(new { message = ex.Message }, statusCode: 500);
        }
    }
}
