using FluentAssertions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Tests.Shared;

public static class ResultAssertionExtensions
{
    public static OperationResultAssertions<TValue, TError> Should<TValue, TError>(
        this OperationResult<TValue, TError> subject)
    {
        return new OperationResultAssertions<TValue, TError>(subject);
    }
}

public sealed class OperationResultAssertions<TValue, TError>
{
    private readonly OperationResult<TValue, TError> _subject;

    public OperationResultAssertions(OperationResult<TValue, TError> subject)
    {
        _subject = subject;
    }

    public AndConstraint<OperationResultAssertions<TValue, TError>> BeSuccess()
    {
        _subject.IsGood.Should().BeTrue();
        return new AndConstraint<OperationResultAssertions<TValue, TError>>(this);
    }

    public AndConstraint<OperationResultAssertions<TValue, TError>> BeFailure()
    {
        _subject.IsGood.Should().BeFalse();
        return new AndConstraint<OperationResultAssertions<TValue, TError>>(this);
    }

    public AndConstraint<OperationResultAssertions<TValue, TError>> HaveError(string expectedErrorCode)
    {
        _subject.TryGetError(out var error).Should().BeTrue();
        error.Should().NotBeNull();

        if (error is DomainError domainError)
        {
            domainError.Code.Should().Be(expectedErrorCode);
        }
        else
        {
            error!.ToString().Should().Contain(expectedErrorCode);
        }

        return new AndConstraint<OperationResultAssertions<TValue, TError>>(this);
    }

    public AndConstraint<OperationResultAssertions<TValue, TError>> HaveValue(TValue expectedValue)
    {
        _subject.TryGetResult(out var value).Should().BeTrue();
        value.Should().BeEquivalentTo(expectedValue);
        return new AndConstraint<OperationResultAssertions<TValue, TError>>(this);
    }
}
