using RhFolha.Domain.Common;

namespace RhFolha.Domain.Payroll;

public sealed class RubricValidity : Entity
{
    private RubricValidity()
    {
        CalculationMethod = string.Empty;
        CalculationBase = string.Empty;
    }

    public RubricValidity(
        Guid rubricId,
        DateOnly startsOn,
        DateOnly? endsOn,
        bool incidenceInss,
        bool incidenceFgts,
        bool incidenceIrrf,
        bool incidenceDsr,
        string calculationMethod,
        string calculationBase)
    {
        RubricId = rubricId;
        StartsOn = startsOn;
        EndsOn = endsOn;
        IncidenceInss = incidenceInss;
        IncidenceFgts = incidenceFgts;
        IncidenceIrrf = incidenceIrrf;
        IncidenceDsr = incidenceDsr;
        CalculationMethod = calculationMethod.Trim();
        CalculationBase = calculationBase.Trim();
        IsActive = true;
    }

    public Guid RubricId { get; private set; }
    public Rubric? Rubric { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public bool IncidenceInss { get; private set; }
    public bool IncidenceFgts { get; private set; }
    public bool IncidenceIrrf { get; private set; }
    public bool IncidenceDsr { get; private set; }
    public string CalculationMethod { get; private set; }
    public string CalculationBase { get; private set; }
    public bool IsActive { get; private set; }
}

