using RhFolha.Domain.Common;

namespace RhFolha.Domain.Companies;

public sealed class Company : Entity
{
    private Company()
    {
        LegalName = string.Empty;
        DocumentNumber = string.Empty;
    }

    public Company(string legalName, string documentNumber)
    {
        LegalName = legalName.Trim();
        DocumentNumber = documentNumber.Trim();
        IsActive = true;
    }

    public string LegalName { get; private set; }
    public string? TradeName { get; private set; }
    public string DocumentNumber { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<Departments.Department> Departments { get; private set; } = [];
    public ICollection<JobPositions.JobPosition> JobPositions { get; private set; } = [];
    public ICollection<Employees.Employee> Employees { get; private set; } = [];
    public ICollection<Employees.EmployeeEvent> EmployeeEvents { get; private set; } = [];
    public ICollection<Payroll.Rubric> Rubrics { get; private set; } = [];
    public ICollection<Payroll.PayrollPeriod> PayrollPeriods { get; private set; } = [];
    public ICollection<Payroll.PayrollEntry> PayrollEntries { get; private set; } = [];
    public ICollection<Payroll.FixedPayrollEntry> FixedPayrollEntries { get; private set; } = [];
    public ICollection<Payroll.PayrollCalculation> PayrollCalculations { get; private set; } = [];
    public ICollection<Payroll.StatutoryTable> StatutoryTables { get; private set; } = [];
}
