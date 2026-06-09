using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Payroll;
using RhFolha.Domain.Security;

namespace RhFolha.Domain.Production;

public sealed class EmployeeProductionBatch : Entity
{
    private EmployeeProductionBatch()
    {
        Name = string.Empty;
        Status = string.Empty;
    }

    public EmployeeProductionBatch(Guid companyId, Guid payrollPeriodId, string name, DateOnly productionDate, Guid? createdByUserId)
    {
        CompanyId = companyId;
        PayrollPeriodId = payrollPeriodId;
        Name = name.Trim();
        ProductionDate = productionDate;
        CreatedByUserId = createdByUserId;
        Status = "Draft";
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid PayrollPeriodId { get; private set; }
    public PayrollPeriod? PayrollPeriod { get; private set; }
    public string Name { get; private set; }
    public DateOnly ProductionDate { get; private set; }
    public Guid? ProductionOrderId { get; private set; }
    public ProductionOrder? ProductionOrder { get; private set; }
    public Guid? ProductionProductId { get; private set; }
    public ProductionProduct? ProductionProduct { get; private set; }
    public Guid? ProductionOperationId { get; private set; }
    public ProductionOperation? ProductionOperation { get; private set; }
    public Guid? ProductionCellId { get; private set; }
    public ProductionCell? ProductionCell { get; private set; }
    public decimal? DefaultQuantity { get; private set; }
    public decimal? DefaultUnitValue { get; private set; }
    public string? DefaultNotes { get; private set; }
    public string Status { get; private set; }
    public int TotalEmployees { get; private set; }
    public decimal TotalQuantity { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public SystemUser? CreatedByUser { get; private set; }
    public ICollection<EmployeeProductionBatchItem> Items { get; private set; } = [];
}
