using FluentAssertions;
using NSubstitute;
using TuColmadoRD.Core.Application.Commands.Tenancy;
using TuColmadoRD.Core.Application.DTOs.Tenancy;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Tests.Phase1;

public class PairDeviceCommandHandlerTests
{
    private readonly IDevicePairingService _pairingService = Substitute.For<IDevicePairingService>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();

    [Fact]
    public async Task Handle_WhenAlreadyPaired_ReturnsDeviceAlreadyPairedError()
    {
        // Arrange
        _tenantProvider.IsPaired.Returns(true);

        var sut = new PairDeviceCommandHandler(_pairingService, _tenantProvider);
        var command = new PairDeviceCommand("email@test.com", "123456", "Caja 1");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be(DevicePairingError.AlreadyPaired.Code);
    }

    [Fact]
    public async Task Handle_WhenHttpReturns401_ReturnsAuthFailedError()
    {
        // Arrange
        _tenantProvider.IsPaired.Returns(false);

        _pairingService.PairAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.AuthFailed));

        var sut = new PairDeviceCommandHandler(_pairingService, _tenantProvider);
        var command = new PairDeviceCommand("email@test.com", "bad", "Caja 1");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be(DevicePairingError.AuthFailed.Code);
    }

    [Fact]
    public async Task Handle_WhenHttpReturns409_ReturnsTerminalConflictError()
    {
        // Arrange
        _tenantProvider.IsPaired.Returns(false);

        _pairingService.PairAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.TerminalConflict));

        var sut = new PairDeviceCommandHandler(_pairingService, _tenantProvider);
        var command = new PairDeviceCommand("email@test.com", "123456", "Caja 1");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be(DevicePairingError.TerminalConflict.Code);
    }

    [Fact]
    public async Task Handle_WhenHttpRequestExceptionThrown_ReturnsNoInternetError()
    {
        // Arrange
        _tenantProvider.IsPaired.Returns(false);

        _pairingService.PairAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.NoInternet));

        var sut = new PairDeviceCommandHandler(_pairingService, _tenantProvider);
        var command = new PairDeviceCommand("email@test.com", "123456", "Caja 1");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be(DevicePairingError.NoInternet.Code);
    }

    [Fact]
    public async Task Handle_WhenHttpReturns200_SavesDeviceIdentityAndReturnsSuccess()
    {
        // Arrange
        _tenantProvider.IsPaired.Returns(false);

        var identity = new DeviceIdentity(Guid.NewGuid(), Guid.NewGuid(), "PUBLIC_KEY", DateTimeOffset.UtcNow, "token");
        _pairingService.PairAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(OperationResult<DeviceIdentity, DevicePairingError>.Good(identity));

        var sut = new PairDeviceCommandHandler(_pairingService, _tenantProvider);
        var command = new PairDeviceCommand("email@test.com", "123456", "Caja 1");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeTrue();
        result.Result.Should().Be(identity);
    }

    [Fact]
    public async Task Handle_WhenFileSaveFails_ReturnsIoError()
    {
        // Arrange
        _tenantProvider.IsPaired.Returns(false);

        _pairingService.PairAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.IoError));

        var sut = new PairDeviceCommandHandler(_pairingService, _tenantProvider);
        var command = new PairDeviceCommand("email@test.com", "123456", "Caja 1");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be(DevicePairingError.IoError.Code);
    }
}
