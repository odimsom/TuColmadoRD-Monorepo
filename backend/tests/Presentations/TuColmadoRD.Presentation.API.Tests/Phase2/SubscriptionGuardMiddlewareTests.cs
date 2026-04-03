using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using TuColmadoRD.Core.Application.DTOs.Security;
using TuColmadoRD.Core.Application.Interfaces.Security;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Infrastructure.CrossCutting.Security;

namespace TuColmadoRD.Tests.Phase2;

public class SubscriptionGuardMiddlewareTests
{
    private readonly ILicenseVerifier _licenseVerifier = Substitute.For<ILicenseVerifier>();

    [Theory]
    [InlineData("/api/device/pair")]
    [InlineData("/api/device/renew-license")]
    [InlineData("/api/device/status")]
    [InlineData("/health")]
    public async Task InvokeAsync_WhenPathIsWhitelisted_CallsNext(string path)
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var sut = new SubscriptionGuardMiddleware(next);

        // Act
        await sut.InvokeAsync(context, _licenseVerifier);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenLicenseIsValid_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/sales";

        _licenseVerifier.VerifyAsync().Returns(
            OperationResult<LicenseStatus, SubscriptionError>.Good(
                new LicenseStatus(true, DateTime.UtcNow.AddDays(2), null)));

        var sut = new SubscriptionGuardMiddleware(next);

        // Act
        await sut.InvokeAsync(context, _licenseVerifier);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenLicenseInvalid_Returns402AndDoesNotCallNext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/sales";
        context.Response.Body = new MemoryStream();

        _licenseVerifier.VerifyAsync().Returns(
            OperationResult<LicenseStatus, SubscriptionError>.Good(
                new LicenseStatus(false, DateTime.UtcNow.AddDays(1), "expired")));

        var sut = new SubscriptionGuardMiddleware(next);

        // Act
        await sut.InvokeAsync(context, _licenseVerifier);

        // Assert
        context.Response.StatusCode.Should().Be(402);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WhenLicenseInvalid_ResponseBodyContainsFailureReason()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/sales";
        context.Response.Body = new MemoryStream();

        _licenseVerifier.VerifyAsync().Returns(
            OperationResult<LicenseStatus, SubscriptionError>.Good(
                new LicenseStatus(false, DateTime.UtcNow.AddDays(1), "clock_tamper_detected")));

        var sut = new SubscriptionGuardMiddleware(next);

        // Act
        await sut.InvokeAsync(context, _licenseVerifier);

        // Assert
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("clock_tamper_detected");
    }

    [Fact]
    public async Task InvokeAsync_WhenLicenseVerifierReturnsFailureResult_Returns402()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/sales";
        context.Response.Body = new MemoryStream();

        _licenseVerifier.VerifyAsync().Returns(
            OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.VerificationFailed));

        var sut = new SubscriptionGuardMiddleware(next);

        // Act
        await sut.InvokeAsync(context, _licenseVerifier);

        // Assert
        context.Response.StatusCode.Should().Be(402);
    }
}
