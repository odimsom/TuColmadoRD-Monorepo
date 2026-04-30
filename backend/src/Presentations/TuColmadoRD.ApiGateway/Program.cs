using System.Text;
using System.IO;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TuColmadoRD.ApiGateway.Middlewares;

namespace TuColmadoRD.ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        // Entry point for standalone running
        if (args.Any(a => a.Contains("TuColmadoRD.ApiGateway.dll")) || args.Length == 0)
        {
            GatewayHostBuilder.BuildGateway(args).Run();
        }
    }
}

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

        var configOptions = builder.Configuration.GetSection("GatewayOptions").Get<GatewayOptions>() ?? new GatewayOptions();
        var authApiUrl = options?.AuthApiUrl ?? configOptions.AuthApiUrl;
        var coreApiUrl = options?.CoreApiUrl ?? configOptions.CoreApiUrl;
        var jwtSecret = options?.JwtSecret ?? configOptions.JwtSecret;
        var allowedOrigins = options?.AllowedOrigins ?? configOptions.AllowedOrigins;

        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient("AuthClient", client => client.BaseAddress = new Uri(authApiUrl));
        builder.Services.AddHttpClient("CoreClient", client => client.BaseAddress = new Uri(coreApiUrl));

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

        builder.Services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(policy =>
            {
                // If no origins specified, allow localhost and specific domains
                if (allowedOrigins.Length == 0)
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
                else
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<IdempotencyMiddleware>();

        app.MapGet("/gateway/health/cloud", async () =>
        {
            try
            {
                await Dns.GetHostEntryAsync("api.github.com");
                return Results.Ok(new { connected = true });
            }
            catch
            {
                return Results.Ok(new { connected = false });
            }
        });

        app.MapGet("/gateway/updates/latest-installer", async (string? channel) =>
        {
            var selectedChannel = string.Equals(channel, "production", StringComparison.OrdinalIgnoreCase) ? "production" : "test";
            var releasesUrls = new[]
            {
                "https://api.github.com/repos/synsetsolutions/TuColmadoRD-Monorepo/releases",
                "https://api.github.com/repos/odimsom/TuColmadoRD-Monorepo/releases"
            };

            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                http.DefaultRequestHeaders.UserAgent.ParseAdd("TuColmadoRD-Gateway/1.0");

                List<GitHubRelease>? releases = null;
                Exception? lastError = null;

                foreach (var releasesUrl in releasesUrls)
                {
                    try
                    {
                        using var response = await http.GetAsync(releasesUrl);
                        response.EnsureSuccessStatusCode();

                        await using var stream = await response.Content.ReadAsStreamAsync();
                        releases = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(stream, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<GitHubRelease>();

                        if (releases.Count > 0)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        lastError = ex;
                    }
                }

                if (releases == null)
                {
                    throw new InvalidOperationException("No se pudo consultar releases en los repositorios configurados.", lastError);
                }

                static Version? ParseVersion(string? tag)
                {
                    if (string.IsNullOrWhiteSpace(tag)) return null;
                    var clean = tag.Trim();
                    if (clean.StartsWith("v", StringComparison.OrdinalIgnoreCase)) clean = clean[1..];
                    var dash = clean.IndexOf('-');
                    if (dash >= 0) clean = clean[..dash];
                    return Version.TryParse(clean, out var version) ? version : null;
                }

                var filtered = releases
                    .Where(r => !string.IsNullOrWhiteSpace(r.TagName))
                    .Where(r =>
                    {
                        var isTestTag = r.TagName!.Contains("-test", StringComparison.OrdinalIgnoreCase) || r.Prerelease;
                        return selectedChannel == "test" ? isTestTag : !isTestTag;
                    })
                    .Select(r => new
                    {
                        Release = r,
                        Version = ParseVersion(r.TagName)
                    })
                    .Where(x => x.Version != null)
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefault();

                if (filtered == null)
                {
                    return Results.NotFound(new { message = "No release found for channel", channel = selectedChannel });
                }

                var installer = filtered.Release.Assets.FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
                if (installer == null)
                {
                    return Results.NotFound(new { message = "Installer asset not found", tag = filtered.Release.TagName });
                }

                return Results.Ok(new
                {
                    channel = selectedChannel,
                    tag = filtered.Release.TagName,
                    version = filtered.Version!.ToString(),
                    installerUrl = installer.BrowserDownloadUrl
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "Unable to resolve latest installer");
            }
        });

        var authGroup = app.MapGroup("/gateway/auth");

        authGroup.MapPost("/register", async (HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/api/auth/register"));

        authGroup.MapPost("/login", async (HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/api/auth/login"));

        app.MapPost("/gateway/devices/pair", async (HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/api/auth/pair-device"))
            .RequireAuthorization();

        // Employee management — proxied to auth service
        authGroup.MapGet("/employees", async (HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/api/auth/employees"))
            .RequireAuthorization();

        authGroup.MapPost("/employees", async (HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/api/auth/employees"))
            .RequireAuthorization();

        authGroup.MapPut("/employees/{id}", async (string id, HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), $"/api/auth/employees/{id}"))
            .RequireAuthorization();

        authGroup.MapMethods("/employees/{id}", ["PATCH"], async (string id, HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), $"/api/auth/employees/{id}"))
            .RequireAuthorization();

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
                || header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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

    private sealed class GitHubRelease
    {
        public string? TagName { get; set; }
        public bool Prerelease { get; set; }
        public List<GitHubAsset> Assets { get; set; } = new();
    }

    private sealed class GitHubAsset
    {
        public string BrowserDownloadUrl { get; set; } = string.Empty;
    }
}
