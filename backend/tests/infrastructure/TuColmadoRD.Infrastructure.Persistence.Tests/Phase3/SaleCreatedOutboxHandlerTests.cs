using System.Net;
using FluentAssertions;
using NSubstitute;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Infrastructure.CrossCutting.Sync;
using TuColmadoRD.Tests.Shared;

namespace TuColmadoRD.Tests.Phase3;

public class SaleCreatedOutboxHandlerTests
{
    private static string BuildPayload(Guid? tenantId = null, Guid? terminalId = null) =>
        $$"""
        {
          "SaleId": "{{Guid.NewGuid()}}",
          "ShiftId": "{{Guid.NewGuid()}}",
          "TenantId": "{{tenantId ?? Guid.NewGuid()}}",
          "TerminalId": "{{terminalId ?? Guid.NewGuid()}}",
          "ReceiptNumber": "B01-00001",
          "CashierName": "Tester",
          "Subtotal": 100,
          "TotalItbis": 18,
          "Total": 118,
          "TotalPaid": 118,
          "ChangeDue": 0,
          "Notes": null,
          "CreatedAt": "2026-03-29T00:00:00Z",
          "Items": [],
          "Payments": []
        }
        """;

    private static SaleCreatedOutboxHandler BuildSut(HttpStatusCode statusCode, object responseBody)
    {
        var fakeHandler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("http://localhost/") };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("CloudSyncAPI").Returns(httpClient);

        return new SaleCreatedOutboxHandler(factory);
    }

    [Fact]
    public async Task HandleAsync_WhenCloudReturns201_ReturnsSuccess()
    {
        var sut = BuildSut(HttpStatusCode.Created, new { ok = true });
        var message = new OutboxMessage("SaleCreated", BuildPayload());

        var result = await sut.HandleAsync(message, CancellationToken.None);

        result.IsGood.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenCloudReturns409_TreatsAsSuccessForIdempotency()
    {
        var sut = BuildSut(HttpStatusCode.Conflict, new { conflict = true });
        var message = new OutboxMessage("SaleCreated", BuildPayload());

        var result = await sut.HandleAsync(message, CancellationToken.None);

        result.IsGood.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenCloudReturns400_ReturnsPermanentFailure()
    {
        var sut = BuildSut(HttpStatusCode.BadRequest, new { error = "bad_request" });
        var message = new OutboxMessage("SaleCreated", BuildPayload());

        var result = await sut.HandleAsync(message, CancellationToken.None);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("PermanentFailure");
    }

    [Fact]
    public async Task HandleAsync_WhenCloudReturns500_ReturnsTransientFailure()
    {
        var sut = BuildSut(HttpStatusCode.InternalServerError, new { error = "server_error" });
        var message = new OutboxMessage("SaleCreated", BuildPayload());

        var result = await sut.HandleAsync(message, CancellationToken.None);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("TransientFailure");
    }

    [Fact]
    public async Task HandleAsync_WhenPayloadIsInvalid_ReturnsDeserializationError()
    {
        var sut = BuildSut(HttpStatusCode.Created, new { ok = true });
        var message = new OutboxMessage("SaleCreated", "{invalid_json}");

        var result = await sut.HandleAsync(message, CancellationToken.None);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("PermanentFailure");
    }

    [Fact]
    public async Task HandleAsync_SentRequestContainsTenantIdAndTerminalIdHeaders()
    {
        var tenantId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();

        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.Created, new { ok = true });
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("http://localhost/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("CloudSyncAPI").Returns(httpClient);

        var sut = new SaleCreatedOutboxHandler(factory);
        var message = new OutboxMessage("SaleCreated", BuildPayload(tenantId, terminalId));

        _ = await sut.HandleAsync(message, CancellationToken.None);

        fakeHandler.CapturedRequests.Should().HaveCount(1);
        var request = fakeHandler.CapturedRequests.Single();
        request.Headers.GetValues("X-Tenant-Id").Single().Should().Be(tenantId.ToString());
        request.Headers.GetValues("X-Terminal-Id").Single().Should().Be(terminalId.ToString());
    }
}
