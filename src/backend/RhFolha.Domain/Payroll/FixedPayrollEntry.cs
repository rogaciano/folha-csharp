using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Employees;

namespace RhFolha.Domain.Payroll;

public sealed class FixedPayrollEntry : Entity
{
    private FixedPayrollEntry()
    {
        Notes = string.Empty;
    }

    public FixedPayrollEntry(
        Guid companyId,
        Guid employeeId,
        Guid rubricId,
        DateOnly startsOn,
        DateOnly? endsOn,
        decimal amount,
        decimal? quantity,
        string? notes)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor do lancamento fixo deve ser maior que zero.");
        }

        if (quantity is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser maior que zero quando informada.");
        }

        if (endsOn.HasValue && endsOn.Value < startsOn)
        {
            throw new ArgumentException("Fim da vigencia nao pode ser anterior ao inicio.");
        }

        CompanyId = companyId;
        EmployeeId = employeeId;
        RubricId = rubricId;
        StartsOn = startsOn;
        EndsOn = endsOn;
        Amount = amount;
        Quantity = quantity;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsActive = true;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public Guid RubricId { get; private set; }
    public Rubric? Rubric { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public decimal Amount { get; private set; }
    public decimal? Quantity { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    public bool IsEffectiveFor(DateOnly startsOn, DateOnly endsOn)
    {
        return IsActive && StartsOn <= endsOn && (!EndsOn.HasValue || EndsOn.Value >= startsOn);
    }

    public void Update(Guid rubricId, DateOnly startsOn, DateOnly? endsOn, decimal amount, decimal? quantity, string? notes)
    {
        Validate(startsOn, endsOn, amount, quantity);

        RubricId = rubricId;
        StartsOn = startsOn;
        EndsOn = endsOn;
        Amount = amount;
        Quantity = quantity;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close(DateOnly endsOn)
    {
        if (endsOn < StartsOn)
        {
            throw new ArgumentException("Fim da vigencia nao pode ser anterior ao inicio.");
        }

        EndsOn = endsOn;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(DateOnly startsOn, DateOnly? endsOn, decimal amount, decimal? quantity)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor do lancamento fixo deve ser maior que zero.");
        }

        if (quantity is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser maior que zero quando informada.");
        }

        if (endsOn.HasValue && endsOn.Value < startsOn)
        {
            throw new ArgumentException("Fim da vigencia nao pode ser anterior ao inicio.");
        }
    }
}
