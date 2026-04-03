using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace TuColmadoRD.ApiGateway.Middlewares;

public class CachedResponse
{
    public int StatusCode { get; set; }
    public string? ContentType { get; set; }
    public byte[] BodyBytes { get; set; } = Array.Empty<byte>();
}

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private const string IdempotencyHeader = "X-Idempotency-Key";

    public IdempotencyMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsOptions(method) || HttpMethods.IsHead(method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(IdempotencyHeader, out var idempotencyKeyValues))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = idempotencyKeyValues.ToString();
        var cacheKey = $"Idempotency_{idempotencyKey}";

        if (_cache.TryGetValue<CachedResponse>(cacheKey, out var cachedResponse) && cachedResponse != null)
        {
            context.Response.StatusCode = cachedResponse.StatusCode;
            if (!string.IsNullOrEmpty(cachedResponse.ContentType))
            {
                context.Response.ContentType = cachedResponse.ContentType;
            }

            if (cachedResponse.BodyBytes.Length > 0)
            {
                await context.Response.Body.WriteAsync(cachedResponse.BodyBytes, 0, cachedResponse.BodyBytes.Length);
            }
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode <= 299)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseBytes = memoryStream.ToArray();

                var responseToCache = new CachedResponse
                {
                    StatusCode = context.Response.StatusCode,
                    ContentType = context.Response.ContentType,
                    BodyBytes = responseBytes
                };

                _cache.Set(cacheKey, responseToCache, TimeSpan.FromHours(24));
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
