using RhFolha.Domain.Common;
using RhFolha.Domain.Employees;

namespace RhFolha.Domain.Payroll;

public sealed class PayrollCalculationItem : Entity
{
    private PayrollCalculationItem()
    {
        EmployeeRegistration = string.Empty;
        EmployeeName = string.Empty;
        RubricCode = string.Empty;
        RubricName = string.Empty;
        RubricType = string.Empty;
        Origin = string.Empty;
    }

    public PayrollCalculationItem(
        Guid payrollCalculationId,
        Guid employeeId,
        string employeeRegistration,
        string employeeName,
        Guid rubricId,
        string rubricCode,
        string rubricName,
        string rubricType,
        string origin,
        decimal amount,
        decimal? quantity,
        decimal? baseAmount = null,
        decimal? calculationRate = null)
    {
        PayrollCalculationId = payrollCalculationId;
        EmployeeId = employeeId;
        EmployeeRegistration = employeeRegistration.Trim();
        EmployeeName = employeeName.Trim();
        RubricId = rubricId;
        RubricCode = rubricCode.Trim();
        RubricName = rubricName.Trim();
        RubricType = rubricType.Trim();
        Origin = origin.Trim();
        Amount = amount;
        Quantity = quantity;
        BaseAmount = baseAmount;
        CalculationRate = calculationRate;
    }

    public Guid PayrollCalculationId { get; private set; }
    public PayrollCalculation? PayrollCalculation { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public string EmployeeRegistration { get; private set; }
    public string EmployeeName { get; private set; }
    public Guid RubricId { get; private set; }
    public Rubric? Rubric { get; private set; }
    public string RubricCode { get; private set; }
    public string RubricName { get; private set; }
    public string RubricType { get; private set; }
    public string Origin { get; private set; }
    public decimal Amount { get; private set; }
    public decimal? Quantity { get; private set; }
    public decimal? BaseAmount { get; private set; }
    public decimal? CalculationRate { get; private set; }
}
