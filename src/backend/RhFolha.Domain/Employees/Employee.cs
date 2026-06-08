using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Departments;
using RhFolha.Domain.JobPositions;

namespace RhFolha.Domain.Employees;

public sealed class Employee : Entity
{
    private Employee()
    {
        Registration = string.Empty;
        Name = string.Empty;
        DocumentNumber = string.Empty;
        CompensationModel = string.Empty;
    }

    public Employee(
        Guid companyId,
        Guid departmentId,
        Guid jobPositionId,
        string registration,
        string name,
        string documentNumber,
        DateOnly admissionDate,
        string compensationModel,
        decimal baseSalary,
        decimal productionUnitValue,
        Guid? responsibleEmployeeId = null)
    {
        CompanyId = companyId;
        DepartmentId = departmentId;
        JobPositionId = jobPositionId;
        ResponsibleEmployeeId = responsibleEmployeeId;
        Registration = registration.Trim();
        Name = name.Trim();
        DocumentNumber = documentNumber.Trim();
        AdmissionDate = admissionDate;
        CompensationModel = compensationModel.Trim();
        BaseSalary = baseSalary;
        ProductionUnitValue = productionUnitValue;
        Status = EmployeeStatus.Active;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Department? Department { get; private set; }
    public Guid JobPositionId { get; private set; }
    public JobPosition? JobPosition { get; private set; }
    public Guid? ResponsibleEmployeeId { get; private set; }
    public Employee? ResponsibleEmployee { get; private set; }
    public string Registration { get; private set; }
    public string Name { get; private set; }
    public string DocumentNumber { get; private set; }
    public DateOnly AdmissionDate { get; private set; }
    public string CompensationModel { get; private set; }
    public decimal BaseSalary { get; private set; }
    public decimal ProductionUnitValue { get; private set; }
    public string? PhotoUrl { get; private set; }
    public EmployeeStatus Status { get; private set; }

    public void UpdatePhoto(string? photoUrl)
    {
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateResponsible(Guid? responsibleEmployeeId)
    {
        ResponsibleEmployeeId = responsibleEmployeeId == Id ? null : responsibleEmployeeId;
        UpdatedAt = DateTime.UtcNow;
    }
}
