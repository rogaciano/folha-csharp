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

    [Fact]
    public void ReopenIntegration_ShouldReturnIntegratedEntryToApproved()
    {
        var entry = CreateEntry(quantity: 1m, unitValue: 10m);
        entry.Approve(Guid.NewGuid());
        entry.MarkIntegrated(Guid.NewGuid(), Guid.NewGuid());

        entry.ReopenIntegration();

        entry.Status.Should().Be("Approved");
        entry.IntegratedPayrollCalculationId.Should().BeNull();
        entry.IntegratedPayrollCalculationItemId.Should().BeNull();
    }

    [Fact]
    public void ReopenIntegration_ShouldRejectEntryNotIntegrated()
    {
        var entry = CreateEntry(quantity: 1m, unitValue: 10m);

        var action = entry.ReopenIntegration;

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ApplyRate_ShouldSetRateSourceAndRecalculateTotal()
    {
        var entry = CreateEntry(quantity: 8m, unitValue: 1m);
        var rateId = Guid.NewGuid();

        entry.ApplyRate(rateId, 2.75m);

        entry.ProductionRateId.Should().Be(rateId);
        entry.UnitValue.Should().Be(2.75m);
        entry.TotalAmount.Should().Be(22m);
        entry.RateSource.Should().Be("RateTable");
    }

    [Fact]
    public void Cancel_ShouldRejectIntegratedEntry()
    {
        var entry = CreateEntry(quantity: 1m, unitValue: 10m);
        entry.Approve(Guid.NewGuid());
        entry.MarkIntegrated(Guid.NewGuid(), Guid.NewGuid());

        var action = entry.Cancel;

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateDraft_ShouldChangeSnapshotsAndRecalculateTotal()
    {
        var entry = CreateEntry(quantity: 1m, unitValue: 10m);

        entry.UpdateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 6, 10),
            Guid.NewGuid(),
            Guid.NewGuid(),
            3m,
            "0004",
            "Novo colaborador",
            "REF-002",
            "Produto novo",
            "Acabamento");

        entry.Quantity.Should().Be(3m);
        entry.TotalAmount.Should().Be(30m);
        entry.EmployeeRegistrationSnapshot.Should().Be("0004");
        entry.ProductReferenceSnapshot.Should().Be("REF-002");
        entry.OperationNameSnapshot.Should().Be("Acabamento");
    }

    [Fact]
    public void UpdateDraft_ShouldRejectApprovedEntry()
    {
        var entry = CreateEntry(quantity: 1m, unitValue: 10m);
        entry.Approve(Guid.NewGuid());

        var action = () => entry.UpdateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 6, 10),
            Guid.NewGuid(),
            Guid.NewGuid(),
            3m,
            "0004",
            "Novo colaborador",
            "REF-002",
            "Produto novo",
            "Acabamento");

        action.Should().Throw<InvalidOperationException>();
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
