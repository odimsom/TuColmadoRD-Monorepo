using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using TuColmadoRD.Core.Application.DTOs.Tenancy;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Infrastructure.CrossCutting.Tenancy;

namespace TuColmadoRD.Tests.Phase1;

public class LocalDeviceTenantProviderTests
{
    [Fact]
    public void GetTenantId_WhenFileDoesNotExist_ReturnsGuidEmpty()
    {
        // Arrange
        var store = Substitute.For<IDeviceIdentityFileStore>();
        store.Exists(Arg.Any<string>()).Returns(false);

        var options = Options.Create(new LocalDeviceOptions { IdentityFilePath = "identity.dat" });
        var sut = new LocalDeviceTenantProvider(options, store);

        // Act
        var tenantId = (Guid)sut.TenantId;

        // Assert
        tenantId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetTenantId_WhenFileExistsAndIsValid_ReturnsTenantId()
    {
        // Arrange
        var identity = new DeviceIdentity(Guid.NewGuid(), Guid.NewGuid(), "PUBLIC_KEY", DateTimeOffset.UtcNow, "token");
        byte[]? encrypted = null;

        var writerStore = Substitute.For<IDeviceIdentityFileStore>();
        writerStore.Exists(Arg.Any<string>()).Returns(false);
        writerStore.WriteAllBytes(Arg.Any<string>(), Arg.Any<byte[]>())
            .Returns(call =>
            {
                encrypted = call.Arg<byte[]>();
                return OperationResult<Unit, DevicePairingError>.Good(Unit.Value);
            });

        var options = Options.Create(new LocalDeviceOptions { IdentityFilePath = "identity.dat" });
        var writerProvider = new LocalDeviceTenantProvider(options, writerStore);
        writerProvider.Persist(identity);

        var readerStore = Substitute.For<IDeviceIdentityFileStore>();
        readerStore.Exists("identity.dat").Returns(true);
        readerStore.ReadAllBytes("identity.dat")
            .Returns(OperationResult<byte[], DevicePairingError>.Good(encrypted!));

        var sut = new LocalDeviceTenantProvider(options, readerStore);

        // Act
        var tenantId = (Guid)sut.TenantId;

        // Assert
        tenantId.Should().Be(identity.TenantId);
    }

    [Fact]
    public void IsPaired_WhenFileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var store = Substitute.For<IDeviceIdentityFileStore>();
        store.Exists(Arg.Any<string>()).Returns(false);

        var options = Options.Create(new LocalDeviceOptions { IdentityFilePath = "identity.dat" });
        var sut = new LocalDeviceTenantProvider(options, store);

        // Act
        var isPaired = sut.IsPaired;

        // Assert
        isPaired.Should().BeFalse();
    }

    [Fact]
    public void IsPaired_WhenFileExistsAndIsValid_ReturnsTrue()
    {
        // Arrange
        var identity = new DeviceIdentity(Guid.NewGuid(), Guid.NewGuid(), "PUBLIC_KEY", DateTimeOffset.UtcNow, "token");
        byte[]? encrypted = null;

        var writerStore = Substitute.For<IDeviceIdentityFileStore>();
        writerStore.Exists(Arg.Any<string>()).Returns(false);
        writerStore.WriteAllBytes(Arg.Any<string>(), Arg.Any<byte[]>())
            .Returns(call =>
            {
                encrypted = call.Arg<byte[]>();
                return OperationResult<Unit, DevicePairingError>.Good(Unit.Value);
            });

        var options = Options.Create(new LocalDeviceOptions { IdentityFilePath = "identity.dat" });
        var writerProvider = new LocalDeviceTenantProvider(options, writerStore);
        writerProvider.Persist(identity);

        var readerStore = Substitute.For<IDeviceIdentityFileStore>();
        readerStore.Exists("identity.dat").Returns(true);
        readerStore.ReadAllBytes("identity.dat")
            .Returns(OperationResult<byte[], DevicePairingError>.Good(encrypted!));

        var sut = new LocalDeviceTenantProvider(options, readerStore);

        // Act
        var isPaired = sut.IsPaired;

        // Assert
        isPaired.Should().BeTrue();
    }

    [Fact]
    public void IsPaired_WhenFileIsCorrupted_ReturnsFalse()
    {
        // Arrange
        var store = Substitute.For<IDeviceIdentityFileStore>();
        store.Exists(Arg.Any<string>()).Returns(true);
        store.ReadAllBytes(Arg.Any<string>())
            .Returns(OperationResult<byte[], DevicePairingError>.Good([1, 2, 3, 4, 5]));

        var options = Options.Create(new LocalDeviceOptions { IdentityFilePath = "identity.dat" });
        var sut = new LocalDeviceTenantProvider(options, store);

        // Act
        var isPaired = sut.IsPaired;

        // Assert
        isPaired.Should().BeFalse();
    }
}
