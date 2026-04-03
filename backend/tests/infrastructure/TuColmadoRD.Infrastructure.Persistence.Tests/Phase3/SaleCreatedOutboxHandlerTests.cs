using System.Net;
using FluentAssertions;
using NSubstitute;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Infrastructure.CrossCutting.Sync;
using TuColmadoRD.Tests.Shared;

namespace TuColmadoRD.Tests.Phase3;

public class SaleCreatedOutboxHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCloudReturns201_ReturnsSuccess()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.Created, new { ok = true });
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("http://localhost/") };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("CloudSyncAPI").Returns(httpClient);

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.TenantId.Returns(TenantIdentifier.Validate(Guid.NewGuid()).Result);
        tenantProvider.TerminalId.Returns(Guid.NewGuid());

        var sut = new SaleCreatedOutboxHandler(factory, tenantProvider);

        var message = new OutboxMessage("SaleCreated", "{\"SaleId\":\"" + Guid.NewGuid() + "\",\"TotalAmount\":100,\"Date\":\"2026-03-29T00:00:00Z\",\"Items\":[]}");

        // Act
        var result = await sut.HandleAsync(message, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenCloudReturns409_TreatsAsSuccessForIdempotency()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.Conflict, new { conflict = true });
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("http://localhost/") };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("CloudSyncAPI").Returns(httpClient);

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.TenantId.Returns(TenantIdentifier.Validate(Guid.NewGuid()).Result);
        tenantProvider.TerminalId.Returns(Guid.NewGuid());

        var sut = new SaleCreatedOutboxHandler(factory, tenantProvider);

        var message = new OutboxMessage("SaleCreated", "{\"SaleId\":\"" + Guid.NewGuid() + "\",\"TotalAmount\":100,\"Date\":\"2026-03-29T00:00:00Z\",\"Items\":[]}");

        // Act
        var result = await sut.HandleAsync(message, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenCloudReturns400_ReturnsPermanentFailure()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, new { error = "bad_request" });
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("http://localhost/") };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("CloudSyncAPI").Returns(httpClient);

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.TenantId.Returns(TenantIdentifier.Validate(Guid.NewGuid()).Result);
        tenantProvider.TerminalId.Returns(Guid.NewGuid());

        var sut = new SaleCreatedOutboxHandler(factory, tenantProvider);

        var message = new OutboxMessage("SaleCreated", "{\"SaleId\":\"" + Guid.NewGuid() + "\",\"TotalAmount\":100,\"Date\":\"2026-03-29T00:00:00Z\",\"Items\":[]}");

        // Act
        var result = await sut.HandleAsync(message, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("PermanentFailure");
    }

    [Fact]
    public async Task HandleAsync_WhenCloudReturns500_ReturnsTransientFailure()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, new { error = "server_error" });
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("http://localhost/") };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("CloudSyncAPI").Returns(httpClient);

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.TenantId.Returns(TenantIdentifier.Validate(Guid.NewGuid()).Result);
        tenantProvider.TerminalId.Returns(Guid.NewGuid());

        var sut = new SaleCreatedOutboxHandler(factory, tenantProvider);

        var message = new OutboxMessage("SaleCreated", "{\"SaleId\":\"" + Guid.NewGuid() + "\",\"TotalAmount\":100,\"Date\":\"2026-03-29T00:00:00Z\",\"Items\":[]}");

        // Act
        var result = await sut.HandleAsync(message, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("TransientFailure");
    }

    [Fact]
    public async Task HandleAsync_WhenPayloadIsInvalid_ReturnsDeserializationError()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.Created, new { ok = true });
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("http://localhost/") };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("CloudSyncAPI").Returns(httpClient);

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.TenantId.Returns(TenantIdentifier.Validate(Guid.NewGuid()).Result);
        tenantProvider.TerminalId.Returns(Guid.NewGuid());

        var sut = new SaleCreatedOutboxHandler(factory, tenantProvider);

        var message = new OutboxMessage("SaleCreated", "{invalid_json}");

        // Act
        var result = await sut.HandleAsync(message, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("PermanentFailure");
    }

    [Fact]
    public async Task HandleAsync_SentRequestContainsTenantIdAndTerminalIdHeaders()
    {
        // Arrange
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.Created, new { ok = true });
        var httpClient = new HttpClient(fakeHandler) { BaseAddress = new Uri("http://localhost/") };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("CloudSyncAPI").Returns(httpClient);

        var tenantId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.TenantId.Returns(TenantIdentifier.Validate(tenantId).Result);
        tenantProvider.TerminalId.Returns(terminalId);

        var sut = new SaleCreatedOutboxHandler(factory, tenantProvider);

        var message = new OutboxMessage("SaleCreated", "{\"SaleId\":\"" + Guid.NewGuid() + "\",\"TotalAmount\":100,\"Date\":\"2026-03-29T00:00:00Z\",\"Items\":[]}");

        // Act
        _ = await sut.HandleAsync(message, CancellationToken.None);

        // Assert
        fakeHandler.CapturedRequests.Should().HaveCount(1);
        var request = fakeHandler.CapturedRequests.Single();
        request.Headers.GetValues("X-Tenant-Id").Single().Should().Be(tenantId.ToString());
        request.Headers.GetValues("X-Terminal-Id").Single().Should().Be(terminalId.ToString());
    }
}
