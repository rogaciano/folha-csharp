using FluentAssertions;
using RhFolha.Domain.Payroll;

namespace RhFolha.Tests.Payroll;

public sealed class PayrollEntryTests
{
    [Fact]
    public void Constructor_ShouldCreateApprovedManualEntry_WhenDataIsValid()
    {
        var entry = new PayrollEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 6, 15),
            150.75m,
            2.5m,
            "ADV-06/2026",
            "Adiantamento quinzenal");

        entry.Amount.Should().Be(150.75m);
        entry.Quantity.Should().Be(2.5m);
        entry.Origin.Should().Be("manual");
        entry.Status.Should().Be("aprovado");
        entry.Reference.Should().Be("ADV-06/2026");
    }

    [Fact]
    public void Constructor_ShouldUseProvidedOrigin_WhenOriginIsInformed()
    {
        var entry = new PayrollEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 6, 15),
            100m,
            null,
            null,
            null,
            "massa");

        entry.Origin.Should().Be("massa");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_ShouldRejectInvalidAmount(decimal amount)
    {
        var act = () => new PayrollEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 6, 15),
            amount,
            null,
            null,
            null);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidQuantity_WhenQuantityIsProvided(decimal quantity)
    {
        var act = () => new PayrollEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 6, 15),
            100m,
            quantity,
            null,
            null);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
