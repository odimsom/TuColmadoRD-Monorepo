using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using TuColmadoRD.Core.Application.Sales.Outbox;
using TuColmadoRD.Core.Application.Interfaces.Sync;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Infrastructure.CrossCutting.Sync;

public class SaleCreatedOutboxHandler : IOutboxMessageHandler
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SaleCreatedOutboxHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<OperationResult<Unit, DomainError>> HandleAsync(OutboxMessage message, CancellationToken ct)
    {
        SaleCreatedPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<SaleCreatedPayload>(message.Payload);
        }
        catch (Exception ex)
        {
            return OperationResult<Unit, DomainError>.Bad(new SyncError("PermanentFailure", $"cloud_rejected:payload_invalid:{ex.Message}"));
        }

        if (payload is null)
        {
            return OperationResult<Unit, DomainError>.Bad(new SyncError("PermanentFailure", "cloud_rejected:payload_null"));
        }

        var client = _httpClientFactory.CreateClient("CloudSyncAPI");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/sync/sales")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("X-Tenant-Id", payload.TenantId.ToString());
        request.Headers.Add("X-Terminal-Id", payload.TerminalId.ToString());

        try
        {
            var response = await client.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.Conflict)
            {
                return OperationResult<Unit, DomainError>.Good(new Unit());
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            {
                return OperationResult<Unit, DomainError>.Bad(new SyncError("PermanentFailure", $"cloud_rejected:{(int)response.StatusCode}"));
            }

            return OperationResult<Unit, DomainError>.Bad(new SyncError("TransientFailure", $"transient:{(int)response.StatusCode}"));
        }
        catch (HttpRequestException ex)
        {
            return OperationResult<Unit, DomainError>.Bad(new SyncError("TransientFailure", $"transient:{ex.Message}"));
        }
        catch (TaskCanceledException ex)
        {
            return OperationResult<Unit, DomainError>.Bad(new SyncError("TransientFailure", $"transient:{ex.Message}"));
        }
        catch (Exception ex)
        {
            return OperationResult<Unit, DomainError>.Bad(new SyncError("TransientFailure", $"transient:{ex.Message}"));
        }
    }
}
