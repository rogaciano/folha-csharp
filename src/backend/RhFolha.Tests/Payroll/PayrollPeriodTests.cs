using FluentAssertions;
using RhFolha.Domain.Payroll;

namespace RhFolha.Tests.Payroll;

public sealed class PayrollPeriodTests
{
    [Fact]
    public void Close_ShouldRejectOpenPeriod()
    {
        var period = new PayrollPeriod(Guid.NewGuid(), 2026, 6);

        var act = period.Close;

        act.Should().Throw<InvalidOperationException>();
        period.Status.Should().Be("aberta");
    }

    [Fact]
    public void Close_ShouldCloseCalculatedPeriod()
    {
        var period = new PayrollPeriod(Guid.NewGuid(), 2026, 6);
        period.MarkCalculated();
        period.Approve();

        period.Close();

        period.Status.Should().Be("fechada");
        period.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_ShouldApproveCalculatedPeriod()
    {
        var period = new PayrollPeriod(Guid.NewGuid(), 2026, 6);
        period.MarkCalculated();

        period.Approve();

        period.Status.Should().Be("aprovada");
        period.CanCalculate.Should().BeFalse();
        period.CanClose.Should().BeTrue();
    }

    [Fact]
    public void Close_ShouldRejectCalculatedPeriodBeforeApproval()
    {
        var period = new PayrollPeriod(Guid.NewGuid(), 2026, 6);
        period.MarkCalculated();

        var act = period.Close;

        act.Should().Throw<InvalidOperationException>();
        period.Status.Should().Be("calculada");
    }

    [Fact]
    public void Reopen_ShouldAllowClosedPeriodToBeRecalculated()
    {
        var period = new PayrollPeriod(Guid.NewGuid(), 2026, 6);
        period.MarkCalculated();
        period.Approve();
        period.Close();

        period.Reopen();

        period.Status.Should().Be("reaberta");
        period.CanCalculate.Should().BeTrue();
    }
}
