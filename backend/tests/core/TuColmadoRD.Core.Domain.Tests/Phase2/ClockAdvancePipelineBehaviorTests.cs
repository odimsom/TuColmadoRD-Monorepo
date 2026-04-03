using MediatR;
using FluentAssertions;
using NSubstitute;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Application.Interfaces.Security;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Tests.Phase2;

public class ClockAdvancePipelineBehaviorTests
{
    private readonly ITimeGuard _timeGuard = Substitute.For<ITimeGuard>();

    [Fact]
    public async Task Handle_WhenClockAdvanceSucceeds_CallsNextHandler()
    {
        // Arrange
        _timeGuard.AdvanceTimeAsync(Arg.Any<DateTime>())
            .Returns(OperationResult<ResultUnit, SubscriptionError>.Good(ResultUnit.Value));

        var sut = new ClockAdvancePipelineBehavior<TestCommand, OperationResult<ResultUnit, SubscriptionError>>(_timeGuard);
        var request = new TestCommand();

        var nextCalled = false;
        RequestHandlerDelegate<OperationResult<ResultUnit, SubscriptionError>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(OperationResult<ResultUnit, SubscriptionError>.Good(ResultUnit.Value));
        };

        // Act
        await sut.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenClockAdvanceSucceeds_ReturnsNextHandlerResult()
    {
        // Arrange
        _timeGuard.AdvanceTimeAsync(Arg.Any<DateTime>())
            .Returns(OperationResult<ResultUnit, SubscriptionError>.Good(ResultUnit.Value));

        var expected = OperationResult<ResultUnit, SubscriptionError>.Good(ResultUnit.Value);

        var sut = new ClockAdvancePipelineBehavior<TestCommand, OperationResult<ResultUnit, SubscriptionError>>(_timeGuard);
        var request = new TestCommand();

        // Act
        var result = await sut.Handle(request, () => Task.FromResult(expected), CancellationToken.None);

        // Assert
        result.IsGood.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenClockTamperDetected_DoesNotCallNextHandler()
    {
        // Arrange
        _timeGuard.AdvanceTimeAsync(Arg.Any<DateTime>())
            .Returns(OperationResult<ResultUnit, SubscriptionError>.Bad(SubscriptionError.ClockTamperDetected));

        var sut = new ClockAdvancePipelineBehavior<TestCommand, OperationResult<ResultUnit, SubscriptionError>>(_timeGuard);
        var request = new TestCommand();

        var nextCalled = false;
        RequestHandlerDelegate<OperationResult<ResultUnit, SubscriptionError>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(OperationResult<ResultUnit, SubscriptionError>.Good(ResultUnit.Value));
        };

        // Act
        _ = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenClockTamperDetected_ReturnsClockTamperDetectedFailure()
    {
        // Arrange
        _timeGuard.AdvanceTimeAsync(Arg.Any<DateTime>())
            .Returns(OperationResult<ResultUnit, SubscriptionError>.Bad(SubscriptionError.ClockTamperDetected));

        var sut = new ClockAdvancePipelineBehavior<TestCommand, OperationResult<ResultUnit, SubscriptionError>>(_timeGuard);
        var request = new TestCommand();

        // Act
        var result = await sut.Handle(request, () => Task.FromResult(OperationResult<ResultUnit, SubscriptionError>.Good(ResultUnit.Value)), CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("clock_tamper_detected");
    }

    [Fact]
    public async Task Handle_WhenRequestIsIQuery_SkipsClockAdvanceAndCallsNext()
    {
        // Arrange
        var sut = new ClockAdvancePipelineBehavior<TestQuery, OperationResult<ResultUnit, SubscriptionError>>(_timeGuard);
        var request = new TestQuery();

        var nextCalled = false;
        RequestHandlerDelegate<OperationResult<ResultUnit, SubscriptionError>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(OperationResult<ResultUnit, SubscriptionError>.Good(ResultUnit.Value));
        };

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsGood.Should().BeTrue();
        await _timeGuard.DidNotReceiveWithAnyArgs().AdvanceTimeAsync(default);
    }

    private sealed record TestCommand : IRequest<OperationResult<ResultUnit, SubscriptionError>>, ICommandMarker;

    private sealed record TestQuery : IRequest<OperationResult<ResultUnit, SubscriptionError>>;
}
