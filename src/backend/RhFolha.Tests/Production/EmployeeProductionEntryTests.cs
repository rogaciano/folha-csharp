using FluentAssertions;
using RhFolha.Domain.Production;

namespace RhFolha.Tests.Production;

public sealed class EmployeeProductionEntryTests
{
    [Fact]
    public void Constructor_ShouldCalculateTotalAmount()
    {
        var entry = CreateEntry(quantity: 12m, unitValue: 3.5m);

        entry.TotalAmount.Should().Be(42m);
        entry.Status.Should().Be("Draft");
        entry.RateSource.Should().Be("Manual");
        entry.Origin.Should().Be("Manual");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidQuantity(decimal quantity)
    {
        var action = () => CreateEntry(quantity: quantity, unitValue: 3.5m);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeUnitValue()
    {
        var action = () => CreateEntry(quantity: 1m, unitValue: -0.01m);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MarkIntegrated_ShouldRequireApprovedStatus()
    {
        var entry = CreateEntry(quantity: 1m, unitValue: 10m);

        var action = () => entry.MarkIntegrated(Guid.NewGuid(), Guid.NewGuid());

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkIntegrated_ShouldSetPayrollReferences_WhenApproved()
    {
        var entry = CreateEntry(quantity: 1m, unitValue: 10m);
        var calculationId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        entry.Approve(Guid.NewGuid());
        entry.MarkIntegrated(calculationId, itemId);

        entry.Status.Should().Be("IntegratedIntoPayroll");
        entry.IntegratedPayrollCalculationId.Should().Be(calculationId);
        entry.IntegratedPayrollCalculationItemId.Should().Be(itemId);
    }

    private static EmployeeProductionEntry CreateEntry(decimal quantity, decimal unitValue)
    {
        return new EmployeeProductionEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 6, 9),
            Guid.NewGuid(),
            Guid.NewGuid(),
            quantity,
            unitValue,
            "0003",
            "Ana Beatris Oliveira",
            "REF-001",
            "Camisa teste",
            "Costura");
    }
}
