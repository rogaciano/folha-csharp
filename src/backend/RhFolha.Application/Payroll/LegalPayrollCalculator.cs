namespace RhFolha.Application.Payroll;

public static class LegalPayrollCalculator
{
    public static decimal CalculatePercentage(decimal calculationBase, decimal ratePercent)
    {
        if (calculationBase <= 0 || ratePercent <= 0)
        {
            return 0m;
        }

        return Math.Round(calculationBase * ratePercent / 100m, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal CalculateProgressiveTableAmount(decimal calculationBase, IEnumerable<LegalTableRange> ranges)
    {
        if (calculationBase <= 0)
        {
            return 0m;
        }

        var orderedRanges = ranges.OrderBy(range => range.LowerLimit).ToList();

        if (orderedRanges.Count == 0)
        {
            return 0m;
        }

        var highestUpperLimit = orderedRanges
            .Where(range => range.UpperLimit.HasValue)
            .Select(range => range.UpperLimit!.Value)
            .DefaultIfEmpty(calculationBase)
            .Max();
        var effectiveBase = orderedRanges.Any(range => !range.UpperLimit.HasValue)
            ? calculationBase
            : Math.Min(calculationBase, highestUpperLimit);
        var selectedRange = orderedRanges.FirstOrDefault(range =>
            effectiveBase >= range.LowerLimit &&
            (!range.UpperLimit.HasValue || effectiveBase <= range.UpperLimit.Value));

        if (selectedRange is null)
        {
            return 0m;
        }

        var amount = effectiveBase * selectedRange.RatePercent / 100m - selectedRange.DeductionAmount;
        return Math.Max(0m, Math.Round(amount, 2, MidpointRounding.AwayFromZero));
    }

    public static decimal CalculateIrrfReductionAmount(
        decimal calculationBase,
        decimal progressiveAmount,
        IEnumerable<LegalTableRange> ranges)
    {
        if (calculationBase <= 0 || progressiveAmount <= 0)
        {
            return 0m;
        }

        var selectedRange = ranges.OrderBy(range => range.LowerLimit).FirstOrDefault(range =>
            calculationBase >= range.LowerLimit &&
            (!range.UpperLimit.HasValue || calculationBase <= range.UpperLimit.Value));

        if (selectedRange is null)
        {
            return 0m;
        }

        if (selectedRange.RatePercent == 0m)
        {
            return progressiveAmount;
        }

        var reduction = selectedRange.DeductionAmount - calculationBase * selectedRange.RatePercent / 100m;
        return Math.Min(progressiveAmount, Math.Max(0m, Math.Round(reduction, 2, MidpointRounding.AwayFromZero)));
    }
}

public sealed record LegalTableRange(
    decimal LowerLimit,
    decimal? UpperLimit,
    decimal RatePercent,
    decimal DeductionAmount);
