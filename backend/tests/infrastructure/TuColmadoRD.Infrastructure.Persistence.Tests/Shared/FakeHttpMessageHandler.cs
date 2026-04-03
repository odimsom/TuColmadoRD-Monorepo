using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace TuColmadoRD.Tests.Shared;

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly ConcurrentQueue<HttpResponseMessage> _responses;

    public List<HttpRequestMessage> CapturedRequests { get; } = [];

    public FakeHttpMessageHandler(HttpStatusCode statusCode, object? responseBody = null)
    {
        _responses = new ConcurrentQueue<HttpResponseMessage>(
            [CreateResponse(statusCode, responseBody)]);
    }

    public FakeHttpMessageHandler(IEnumerable<HttpResponseMessage> responses)
    {
        _responses = new ConcurrentQueue<HttpResponseMessage>(responses);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CapturedRequests.Add(request);

        if (_responses.TryDequeue(out var response))
        {
            return Task.FromResult(response);
        }

        return Task.FromResult(CreateResponse(HttpStatusCode.OK, null));
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, object? body)
    {
        var response = new HttpResponseMessage(statusCode);
        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return response;
    }
}
