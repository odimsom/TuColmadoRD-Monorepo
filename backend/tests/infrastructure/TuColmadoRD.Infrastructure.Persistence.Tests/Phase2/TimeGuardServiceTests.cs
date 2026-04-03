using FluentAssertions;
using System.Globalization;
using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Infrastructure.Persistence.Repositories;
using TuColmadoRD.Tests.Shared;

namespace TuColmadoRD.Tests.Phase2;

public class TimeGuardServiceTests
{
    [Fact]
    public async Task AdvanceTimeAsync_WhenNoLktStored_PersistsNewTimeAndReturnsSuccess()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        await using var db = factory.CreateDbContext();

        var repo = new SystemConfigRepository(db);
        var sut = new TimeGuardService(repo);
        var newTime = DateTime.UtcNow;

        // Act
        var result = await sut.AdvanceTimeAsync(newTime);

        // Assert
        result.IsGood.Should().BeTrue();
        var stored = await repo.GetLastKnownTimeAsync();
        DateTimeOffset.Parse(stored!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            .UtcDateTime
            .Should()
            .BeCloseTo(newTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AdvanceTimeAsync_WhenNewTimeIsGreaterThanLkt_UpdatesLktAndReturnsSuccess()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        await using var db = factory.CreateDbContext();

        var repo = new SystemConfigRepository(db);
        var sut = new TimeGuardService(repo);

        var oldTime = DateTime.UtcNow.AddMinutes(-10);
        await repo.UpdateLastKnownTimeAsync(oldTime.ToString("O"));

        var newTime = DateTime.UtcNow;

        // Act
        var result = await sut.AdvanceTimeAsync(newTime);

        // Assert
        result.IsGood.Should().BeTrue();
        var stored = await repo.GetLastKnownTimeAsync();
        DateTimeOffset.Parse(stored!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            .UtcDateTime
            .Should()
            .BeCloseTo(newTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AdvanceTimeAsync_WhenNewTimeEqualsLkt_ReturnsSuccess()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        await using var db = factory.CreateDbContext();

        var repo = new SystemConfigRepository(db);
        var sut = new TimeGuardService(repo);

        var current = DateTime.UtcNow;
        await repo.UpdateLastKnownTimeAsync(current.ToString("O"));

        // Act
        var result = await sut.AdvanceTimeAsync(current);

        // Assert
        result.IsGood.Should().BeTrue();
    }

    [Fact]
    public async Task AdvanceTimeAsync_WhenNewTimeIsLessThanLkt_ReturnsClockTamperDetectedError()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        await using var db = factory.CreateDbContext();

        var repo = new SystemConfigRepository(db);
        var sut = new TimeGuardService(repo);

        var lkt = DateTime.UtcNow;
        await repo.UpdateLastKnownTimeAsync(lkt.ToString("O"));

        var tampered = lkt.AddTicks(-1);

        // Act
        var result = await sut.AdvanceTimeAsync(tampered);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("clock_tamper_detected");
    }

    [Fact]
    public async Task AdvanceTimeAsync_WhenNewTimeLessThanLkt_DoesNotUpdatePersistedLkt()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        await using var db = factory.CreateDbContext();

        var repo = new SystemConfigRepository(db);
        var sut = new TimeGuardService(repo);

        var lkt = DateTime.UtcNow;
        await repo.UpdateLastKnownTimeAsync(lkt.ToString("O"));

        // Act
        _ = await sut.AdvanceTimeAsync(lkt.AddSeconds(-5));

        // Assert
        var stored = await repo.GetLastKnownTimeAsync();
        DateTimeOffset.Parse(stored!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            .UtcDateTime
            .Should()
            .BeCloseTo(lkt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetLastKnownTimeAsync_WhenNoLktStored_ReturnsMinValue()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        await using var db = factory.CreateDbContext();

        var repo = new SystemConfigRepository(db);
        var sut = new TimeGuardService(repo);

        // Act
        var result = await sut.GetLastKnownTimeAsync();

        // Assert
        result.IsGood.Should().BeTrue();
        result.Result.Should().Be(DateTime.MinValue);
    }
}
