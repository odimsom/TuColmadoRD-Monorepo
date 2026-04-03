using FluentAssertions;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.Sales.Events;
using TuColmadoRD.Core.Domain.Enums.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Tests.Sales;

public class ShiftTests
{
    [Fact]
    public void Open_WhenDataIsValid_ReturnsOpenShiftAndEvent()
    {
        var openingCash = Money.FromDecimal(500m).Result!;

        var result = Shift.Open(Guid.NewGuid(), Guid.NewGuid(), openingCash, "Juan Perez");

        result.IsGood.Should().BeTrue();
        result.Result.Status.Should().Be(ShiftStatus.Open);
        result.Result.OpeningCashAmount.Amount.Should().Be(500m);
        result.Result.TotalSalesCount.Should().Be(0);
        result.Result.TotalSalesAmount.Amount.Should().Be(0m);
        result.Result.DomainEvents.Should().ContainSingle(e => e is ShiftOpenedDomainEvent);
    }

    [Fact]
    public void Open_WhenCashierNameIsEmpty_ReturnsValidationError()
    {
        var openingCash = Money.FromDecimal(100m).Result!;

        var result = Shift.Open(Guid.NewGuid(), Guid.NewGuid(), openingCash, " ");

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("shift.cashier_name_required");
    }

    [Fact]
    public void Open_WhenCashierNameExceeds100_ReturnsValidationError()
    {
        var openingCash = Money.FromDecimal(100m).Result!;
        var cashierName = new string('A', 101);

        var result = Shift.Open(Guid.NewGuid(), Guid.NewGuid(), openingCash, cashierName);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("shift.cashier_name_too_long");
    }

    [Fact]
    public void Close_WhenShiftIsOpen_ClosesShiftAndComputesDifference()
    {
        var shift = CreateOpenShift();
        var actualCash = Money.FromDecimal(150m).Result!;
        var expectedCash = Money.FromDecimal(120m).Result!;

        var result = shift.Close(actualCash, expectedCash, "Cierre correcto");

        result.IsGood.Should().BeTrue();
        shift.Status.Should().Be(ShiftStatus.Closed);
        shift.ActualCashAmount!.Amount.Should().Be(150m);
        shift.ExpectedCashAmount!.Amount.Should().Be(120m);
        shift.CashDifferenceAmount.Should().Be(30m);
        shift.DomainEvents.Should().Contain(e => e is ShiftClosedDomainEvent);
    }

    [Fact]
    public void Close_WhenActualCashIsLessThanExpected_KeepsSignedNegativeDifference()
    {
        var shift = CreateOpenShift();
        var actualCash = Money.FromDecimal(90m).Result!;
        var expectedCash = Money.FromDecimal(120m).Result!;

        var result = shift.Close(actualCash, expectedCash, null);

        result.IsGood.Should().BeTrue();
        shift.CashDifferenceAmount.Should().Be(-30m);
    }

    [Fact]
    public void Close_WhenShiftAlreadyClosed_ReturnsBusinessError()
    {
        var shift = CreateOpenShift();
        var actualCash = Money.FromDecimal(100m).Result!;
        var expectedCash = Money.FromDecimal(100m).Result!;
        var firstClose = shift.Close(actualCash, expectedCash, null);
        firstClose.IsGood.Should().BeTrue();

        var secondClose = shift.Close(actualCash, expectedCash, null);

        secondClose.IsGood.Should().BeFalse();
        secondClose.Error.Code.Should().Be("shift.already_closed");
    }

    [Fact]
    public void Close_WhenNotesExceed500_ReturnsValidationError()
    {
        var shift = CreateOpenShift();
        var actualCash = Money.FromDecimal(100m).Result!;
        var expectedCash = Money.FromDecimal(100m).Result!;
        var notes = new string('N', 501);

        var result = shift.Close(actualCash, expectedCash, notes);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("shift.notes_too_long");
    }

    private static Shift CreateOpenShift()
    {
        var openingCash = Money.FromDecimal(100m).Result!;
        var result = Shift.Open(Guid.NewGuid(), Guid.NewGuid(), openingCash, "Cajero");
        return result.Result;
    }
}
