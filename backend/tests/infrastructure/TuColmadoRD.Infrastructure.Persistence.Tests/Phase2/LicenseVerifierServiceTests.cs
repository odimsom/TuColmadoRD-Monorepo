using FluentAssertions;
using NSubstitute;
using TuColmadoRD.Core.Application.DTOs.Tenancy;
using TuColmadoRD.Core.Application.Interfaces.Security;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Tests.Shared;

namespace TuColmadoRD.Tests.Phase2;

public class LicenseVerifierServiceTests
{
    private readonly IDeviceIdentityStore _identityStore = Substitute.For<IDeviceIdentityStore>();
    private readonly ITimeGuard _timeGuard = Substitute.For<ITimeGuard>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();

    [Fact]
    public async Task VerifyAsync_WhenNoLicenseTokenStored_ReturnsLicenseNotFoundError()
    {
        // Arrange
        _identityStore.Read().Returns(OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.IoError));
        _tenantProvider.TerminalId.Returns(Guid.NewGuid());

        var sut = new LicenseVerifierService(_identityStore, _timeGuard, _clock, _tenantProvider);

        // Act
        var result = await sut.VerifyAsync();

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("license_not_found");
    }

    [Fact]
    public async Task VerifyAsync_WhenSignatureIsInvalid_ReturnsInvalidSignatureError()
    {
        // Arrange
        var (_, signingPrivate) = TestJwtFactory.GenerateKeyPair();
        var (wrongPublic, _) = TestJwtFactory.GenerateKeyPair();

        var terminalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var jwt = TestJwtFactory.GenerateToken(signingPrivate, tenantId, terminalId, DateTime.UtcNow.AddDays(1));

        var identity = new DeviceIdentity(tenantId, terminalId, wrongPublic, DateTimeOffset.UtcNow, jwt);

        _identityStore.Read().Returns(OperationResult<DeviceIdentity, DevicePairingError>.Good(identity));
        _tenantProvider.TerminalId.Returns(terminalId);

        var sut = new LicenseVerifierService(_identityStore, _timeGuard, _clock, _tenantProvider);

        // Act
        var result = await sut.VerifyAsync();

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("invalid_signature");
    }

    [Fact]
    public async Task VerifyAsync_WhenClockTamperDetected_ReturnsClockTamperDetectedError()
    {
        // Arrange
        var (publicKey, privateKey) = TestJwtFactory.GenerateKeyPair();
        var terminalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var now = DateTime.UtcNow;
        var jwt = TestJwtFactory.GenerateToken(privateKey, tenantId, terminalId, now.AddDays(1));

        var identity = new DeviceIdentity(tenantId, terminalId, publicKey, DateTimeOffset.UtcNow, jwt);

        _identityStore.Read().Returns(OperationResult<DeviceIdentity, DevicePairingError>.Good(identity));
        _tenantProvider.TerminalId.Returns(terminalId);
        _clock.UtcNow.Returns(now);
        _timeGuard.AdvanceTimeAsync(now).Returns(OperationResult<Unit, SubscriptionError>.Bad(SubscriptionError.ClockTamperDetected));

        var sut = new LicenseVerifierService(_identityStore, _timeGuard, _clock, _tenantProvider);

        // Act
        var result = await sut.VerifyAsync();

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("clock_tamper_detected");
    }

    [Fact]
    public async Task VerifyAsync_WhenTokenExpired_ReturnsSubscriptionExpiredError()
    {
        // Arrange
        var (publicKey, privateKey) = TestJwtFactory.GenerateKeyPair();
        var terminalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var now = DateTime.UtcNow;
        var jwt = TestJwtFactory.GenerateToken(privateKey, tenantId, terminalId, now.AddDays(-1));

        var identity = new DeviceIdentity(tenantId, terminalId, publicKey, DateTimeOffset.UtcNow, jwt);

        _identityStore.Read().Returns(OperationResult<DeviceIdentity, DevicePairingError>.Good(identity));
        _tenantProvider.TerminalId.Returns(terminalId);
        _clock.UtcNow.Returns(now);
        _timeGuard.AdvanceTimeAsync(now).Returns(OperationResult<Unit, SubscriptionError>.Good(Unit.Value));

        var sut = new LicenseVerifierService(_identityStore, _timeGuard, _clock, _tenantProvider);

        // Act
        var result = await sut.VerifyAsync();

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("subscription_expired");
    }

    [Fact]
    public async Task VerifyAsync_WhenTerminalIdMismatch_ReturnsTerminalMismatchError()
    {
        // Arrange
        var (publicKey, privateKey) = TestJwtFactory.GenerateKeyPair();
        var tokenTerminal = Guid.NewGuid();
        var runtimeTerminal = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var jwt = TestJwtFactory.GenerateToken(privateKey, tenantId, tokenTerminal, DateTime.UtcNow.AddDays(1));
        var identity = new DeviceIdentity(tenantId, tokenTerminal, publicKey, DateTimeOffset.UtcNow, jwt);

        _identityStore.Read().Returns(OperationResult<DeviceIdentity, DevicePairingError>.Good(identity));
        _tenantProvider.TerminalId.Returns(runtimeTerminal);

        var sut = new LicenseVerifierService(_identityStore, _timeGuard, _clock, _tenantProvider);

        // Act
        var result = await sut.VerifyAsync();

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("terminal_mismatch");
    }

    [Fact]
    public async Task VerifyAsync_WhenAllChecksPass_ReturnsValidLicenseStatus()
    {
        // Arrange
        var (publicKey, privateKey) = TestJwtFactory.GenerateKeyPair();
        var terminalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var jwt = TestJwtFactory.GenerateToken(privateKey, tenantId, terminalId, now.AddDays(7));
        var identity = new DeviceIdentity(tenantId, terminalId, publicKey, DateTimeOffset.UtcNow, jwt);

        _identityStore.Read().Returns(OperationResult<DeviceIdentity, DevicePairingError>.Good(identity));
        _tenantProvider.TerminalId.Returns(terminalId);
        _clock.UtcNow.Returns(now);
        _timeGuard.AdvanceTimeAsync(now).Returns(OperationResult<Unit, SubscriptionError>.Good(Unit.Value));

        var sut = new LicenseVerifierService(_identityStore, _timeGuard, _clock, _tenantProvider);

        // Act
        var result = await sut.VerifyAsync();

        // Assert
        result.IsGood.Should().BeTrue();
        result.Result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyAsync_WhenExpired_LicenseStatusContainsValidUntilDate()
    {
        // Arrange
        var (publicKey, privateKey) = TestJwtFactory.GenerateKeyPair();
        var terminalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var validUntil = DateTime.UtcNow.AddHours(-12);
        var now = DateTime.UtcNow;

        var jwt = TestJwtFactory.GenerateToken(privateKey, tenantId, terminalId, validUntil);
        var identity = new DeviceIdentity(tenantId, terminalId, publicKey, DateTimeOffset.UtcNow, jwt);

        _identityStore.Read().Returns(OperationResult<DeviceIdentity, DevicePairingError>.Good(identity));
        _tenantProvider.TerminalId.Returns(terminalId);
        _clock.UtcNow.Returns(now);
        _timeGuard.AdvanceTimeAsync(now).Returns(OperationResult<Unit, SubscriptionError>.Good(Unit.Value));

        var sut = new LicenseVerifierService(_identityStore, _timeGuard, _clock, _tenantProvider);

        // Act
        var result = await sut.VerifyAsync();

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("subscription_expired");
    }
}
