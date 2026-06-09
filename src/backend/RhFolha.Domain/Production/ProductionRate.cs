using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Departments;
using RhFolha.Domain.JobPositions;

namespace RhFolha.Domain.Production;

public sealed class ProductionRate : Entity
{
    private ProductionRate()
    {
        CalculationType = string.Empty;
        Status = string.Empty;
    }

    public ProductionRate(Guid companyId, Guid productionRateTableId, decimal unitValue)
    {
        CompanyId = companyId;
        ProductionRateTableId = productionRateTableId;
        UnitValue = unitValue;
        CalculationType = "QuantityTimesUnitValue";
        Status = "Active";
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid ProductionRateTableId { get; private set; }
    public ProductionRateTable? ProductionRateTable { get; private set; }
    public Guid? ProductionProductId { get; private set; }
    public ProductionProduct? ProductionProduct { get; private set; }
    public Guid? ProductionOperationId { get; private set; }
    public ProductionOperation? ProductionOperation { get; private set; }
    public Guid? JobPositionId { get; private set; }
    public JobPosition? JobPosition { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Department? Department { get; private set; }
    public Guid? ProductionCellId { get; private set; }
    public ProductionCell? ProductionCell { get; private set; }
    public decimal UnitValue { get; private set; }
    public string CalculationType { get; private set; }
    public decimal? MinimumQuantity { get; private set; }
    public decimal? MaximumQuantity { get; private set; }
    public string Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}
