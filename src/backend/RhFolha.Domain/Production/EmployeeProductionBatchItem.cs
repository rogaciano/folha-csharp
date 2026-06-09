using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Employees;

namespace RhFolha.Domain.Production;

public sealed class EmployeeProductionBatchItem : Entity
{
    private EmployeeProductionBatchItem()
    {
        Status = string.Empty;
    }

    public EmployeeProductionBatchItem(
        Guid companyId,
        Guid employeeProductionBatchId,
        Guid employeeId,
        decimal quantity,
        decimal unitValue)
    {
        CompanyId = companyId;
        EmployeeProductionBatchId = employeeProductionBatchId;
        EmployeeId = employeeId;
        Quantity = quantity;
        UnitValue = unitValue;
        TotalAmount = quantity * unitValue;
        Status = "Generated";
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid EmployeeProductionBatchId { get; private set; }
    public EmployeeProductionBatch? EmployeeProductionBatch { get; private set; }
    public Guid? EmployeeProductionEntryId { get; private set; }
    public EmployeeProductionEntry? EmployeeProductionEntry { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitValue { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; }
    public string? ErrorMessage { get; private set; }
}
