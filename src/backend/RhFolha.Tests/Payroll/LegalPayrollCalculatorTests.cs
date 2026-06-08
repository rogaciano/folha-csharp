using FluentAssertions;
using RhFolha.Application.Payroll;

namespace RhFolha.Tests.Payroll;

public sealed class LegalPayrollCalculatorTests
{
    [Fact]
    public void CalculatePercentage_ShouldCalculateFgtsAtEightPercent()
    {
        var amount = LegalPayrollCalculator.CalculatePercentage(3200m, 8m);

        amount.Should().Be(256m);
    }

    [Fact]
    public void CalculateProgressiveTableAmount_ShouldApplyInssCeiling()
    {
        var amount = LegalPayrollCalculator.CalculateProgressiveTableAmount(9500m, Inss2026Ranges());

        amount.Should().Be(988.09m);
    }

    [Fact]
    public void CalculateProgressiveTableAmount_ShouldApplyInssIntermediateRange()
    {
        var amount = LegalPayrollCalculator.CalculateProgressiveTableAmount(3200m, Inss2026Ranges());

        amount.Should().Be(272.60m);
    }

    [Fact]
    public void CalculateProgressiveTableAmount_ShouldCalculateIrrfProgressiveAmount()
    {
        var amount = LegalPayrollCalculator.CalculateProgressiveTableAmount(8511.91m, Irrf2026Ranges());

        amount.Should().Be(1432.05m);
    }

    [Fact]
    public void CalculateIrrfReductionAmount_ShouldEliminateTaxUpToFiveThousand()
    {
        var progressiveAmount = LegalPayrollCalculator.CalculateProgressiveTableAmount(3200m, Irrf2026Ranges());

        var reduction = LegalPayrollCalculator.CalculateIrrfReductionAmount(3200m, progressiveAmount, IrrfReduction2026Ranges());

        reduction.Should().Be(progressiveAmount);
    }

    [Fact]
    public void CalculateIrrfReductionAmount_ShouldApplyIntermediateReduction()
    {
        var progressiveAmount = LegalPayrollCalculator.CalculateProgressiveTableAmount(6000m, Irrf2026Ranges());

        var reduction = LegalPayrollCalculator.CalculateIrrfReductionAmount(6000m, progressiveAmount, IrrfReduction2026Ranges());

        reduction.Should().Be(179.75m);
    }

    private static IReadOnlyCollection<LegalTableRange> Inss2026Ranges()
    {
        return
        [
            new(0m, 1621.00m, 7.5m, 0m),
            new(1621.01m, 2902.84m, 9.0m, 24.32m),
            new(2902.85m, 4354.27m, 12.0m, 111.40m),
            new(4354.28m, 8475.55m, 14.0m, 198.49m),
        ];
    }

    private static IReadOnlyCollection<LegalTableRange> Irrf2026Ranges()
    {
        return
        [
            new(0m, 2428.80m, 0m, 0m),
            new(2428.81m, 2826.65m, 7.5m, 182.16m),
            new(2826.66m, 3751.05m, 15.0m, 394.16m),
            new(3751.06m, 4664.68m, 22.5m, 675.49m),
            new(4664.69m, null, 27.5m, 908.73m),
        ];
    }

    private static IReadOnlyCollection<LegalTableRange> IrrfReduction2026Ranges()
    {
        return
        [
            new(0m, 5000.00m, 0m, 312.89m),
            new(5000.01m, 7350.00m, 13.3145m, 978.62m),
        ];
    }
}
