using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Employees;

namespace RhFolha.Domain.Payroll;

public sealed class PayrollEntry : Entity
{
    private PayrollEntry()
    {
        Origin = string.Empty;
        Status = string.Empty;
    }

    public PayrollEntry(
        Guid companyId,
        Guid payrollPeriodId,
        Guid employeeId,
        Guid rubricId,
        DateOnly entryDate,
        decimal amount,
        decimal? quantity,
        string? reference,
        string? notes,
        string origin = "manual")
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor do lancamento deve ser maior que zero.");
        }

        if (quantity is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser maior que zero quando informada.");
        }

        CompanyId = companyId;
        PayrollPeriodId = payrollPeriodId;
        EmployeeId = employeeId;
        RubricId = rubricId;
        EntryDate = entryDate;
        Amount = amount;
        Quantity = quantity;
        Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Origin = string.IsNullOrWhiteSpace(origin) ? "manual" : origin.Trim();
        Status = "aprovado";
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid PayrollPeriodId { get; private set; }
    public PayrollPeriod? PayrollPeriod { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public Guid RubricId { get; private set; }
    public Rubric? Rubric { get; private set; }
    public DateOnly EntryDate { get; private set; }
    public decimal Amount { get; private set; }
    public decimal? Quantity { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public string Origin { get; private set; }
    public string Status { get; private set; }
}
