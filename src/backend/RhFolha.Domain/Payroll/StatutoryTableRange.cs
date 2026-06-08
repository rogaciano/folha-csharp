using RhFolha.Domain.Common;

namespace RhFolha.Domain.Payroll;

public sealed class StatutoryTableRange : Entity
{
    private StatutoryTableRange()
    {
    }

    public StatutoryTableRange(
        Guid statutoryTableId,
        decimal lowerLimit,
        decimal? upperLimit,
        decimal ratePercent,
        decimal deductionAmount)
    {
        if (lowerLimit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lowerLimit), "Limite inicial nao pode ser negativo.");
        }

        if (upperLimit.HasValue && upperLimit.Value < lowerLimit)
        {
            throw new ArgumentException("Limite final nao pode ser menor que o limite inicial.");
        }

        if (ratePercent < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ratePercent), "Aliquota nao pode ser negativa.");
        }

        if (deductionAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deductionAmount), "Parcela a deduzir nao pode ser negativa.");
        }

        StatutoryTableId = statutoryTableId;
        LowerLimit = lowerLimit;
        UpperLimit = upperLimit;
        RatePercent = ratePercent;
        DeductionAmount = deductionAmount;
    }

    public Guid StatutoryTableId { get; private set; }
    public StatutoryTable? StatutoryTable { get; private set; }
    public decimal LowerLimit { get; private set; }
    public decimal? UpperLimit { get; private set; }
    public decimal RatePercent { get; private set; }
    public decimal DeductionAmount { get; private set; }
}
