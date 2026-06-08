using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Payroll;

public sealed class PayrollPeriod : Entity
{
    private PayrollPeriod()
    {
        Code = string.Empty;
        Status = string.Empty;
    }

    public PayrollPeriod(Guid companyId, int year, int month)
    {
        CompanyId = companyId;
        Year = year;
        Month = month;
        Code = $"{year:D4}-{month:D2}";
        StartsOn = new DateOnly(year, month, 1);
        EndsOn = StartsOn.AddMonths(1).AddDays(-1);
        Status = "aberta";
        OpenedAt = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public string Code { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly EndsOn { get; private set; }
    public string Status { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    public bool CanApprove => Status == "calculada";
    public bool CanClose => Status == "aprovada";
    public bool CanReopen => Status == "fechada";
    public bool CanCalculate => Status is "aberta" or "reaberta" or "calculada";

    public void MarkCalculated()
    {
        if (!CanCalculate)
        {
            throw new InvalidOperationException("A competencia nao pode ser calculada no status atual.");
        }

        Status = "calculada";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (!CanApprove)
        {
            throw new InvalidOperationException("A competencia nao pode ser aprovada no status atual.");
        }

        Status = "aprovada";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        if (!CanClose)
        {
            throw new InvalidOperationException("A competencia nao pode ser fechada no status atual.");
        }

        Status = "fechada";
        ClosedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (!CanReopen)
        {
            throw new InvalidOperationException("A competencia nao pode ser reaberta no status atual.");
        }

        Status = "reaberta";
        ClosedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
