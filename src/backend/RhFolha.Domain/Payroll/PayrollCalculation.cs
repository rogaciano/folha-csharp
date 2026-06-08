using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Payroll;

public sealed class PayrollCalculation : Entity
{
    private PayrollCalculation()
    {
        PeriodCode = string.Empty;
        Status = string.Empty;
    }

    public PayrollCalculation(Guid companyId, Guid payrollPeriodId, string periodCode)
    {
        CompanyId = companyId;
        PayrollPeriodId = payrollPeriodId;
        PeriodCode = periodCode.Trim();
        CalculatedAt = DateTime.UtcNow;
        Status = "calculada";
        IsCurrent = true;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid PayrollPeriodId { get; private set; }
    public PayrollPeriod? PayrollPeriod { get; private set; }
    public string PeriodCode { get; private set; }
    public DateTime CalculatedAt { get; private set; }
    public decimal TotalProventos { get; private set; }
    public decimal TotalDescontos { get; private set; }
    public decimal TotalLiquido { get; private set; }
    public int EmployeeCount { get; private set; }
    public string Status { get; private set; }
    public bool IsCurrent { get; private set; }
    public ICollection<PayrollCalculationItem> Items { get; private set; } = [];

    public void SetTotals(decimal totalProventos, decimal totalDescontos, int employeeCount)
    {
        TotalProventos = totalProventos;
        TotalDescontos = totalDescontos;
        TotalLiquido = totalProventos - totalDescontos;
        EmployeeCount = employeeCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsHistorical()
    {
        IsCurrent = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
