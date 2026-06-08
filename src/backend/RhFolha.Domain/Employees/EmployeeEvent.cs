using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Employees;

public sealed class EmployeeEvent : Entity
{
    private EmployeeEvent()
    {
        Type = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
        Responsible = string.Empty;
        Status = string.Empty;
    }

    public EmployeeEvent(
        Guid companyId,
        Guid employeeId,
        DateOnly eventDate,
        string type,
        string title,
        string description,
        string responsible)
    {
        CompanyId = companyId;
        EmployeeId = employeeId;
        EventDate = eventDate;
        Type = type.Trim();
        Title = title.Trim();
        Description = description.Trim();
        Responsible = responsible.Trim();
        Status = "ativo";
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public DateOnly EventDate { get; private set; }
    public string Type { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Responsible { get; private set; }
    public string Status { get; private set; }

    public void Cancel()
    {
        Status = "cancelado";
        UpdatedAt = DateTime.UtcNow;
    }
}
