using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Employees;
using RhFolha.Domain.Payroll;
using RhFolha.Domain.Security;

namespace RhFolha.Domain.Production;

public sealed class EmployeeProductionEntry : Entity
{
    private EmployeeProductionEntry()
    {
        RateSource = string.Empty;
        Origin = string.Empty;
        Status = string.Empty;
        EmployeeRegistrationSnapshot = string.Empty;
        EmployeeNameSnapshot = string.Empty;
        ProductReferenceSnapshot = string.Empty;
        ProductDescriptionSnapshot = string.Empty;
        OperationNameSnapshot = string.Empty;
    }

    public EmployeeProductionEntry(
        Guid companyId,
        Guid payrollPeriodId,
        Guid employeeId,
        DateOnly productionDate,
        Guid productionProductId,
        Guid productionOperationId,
        decimal quantity,
        decimal unitValue,
        string employeeRegistrationSnapshot,
        string employeeNameSnapshot,
        string productReferenceSnapshot,
        string productDescriptionSnapshot,
        string operationNameSnapshot)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade de producao deve ser maior que zero.");
        }

        if (unitValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitValue), "O valor unitario da producao nao pode ser negativo.");
        }

        CompanyId = companyId;
        PayrollPeriodId = payrollPeriodId;
        EmployeeId = employeeId;
        ProductionDate = productionDate;
        ProductionProductId = productionProductId;
        ProductionOperationId = productionOperationId;
        Quantity = quantity;
        UnitValue = unitValue;
        TotalAmount = quantity * unitValue;
        RateSource = "Manual";
        Origin = "Manual";
        Status = "Draft";
        EmployeeRegistrationSnapshot = employeeRegistrationSnapshot.Trim();
        EmployeeNameSnapshot = employeeNameSnapshot.Trim();
        ProductReferenceSnapshot = productReferenceSnapshot.Trim();
        ProductDescriptionSnapshot = productDescriptionSnapshot.Trim();
        OperationNameSnapshot = operationNameSnapshot.Trim();
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid PayrollPeriodId { get; private set; }
    public PayrollPeriod? PayrollPeriod { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public DateOnly ProductionDate { get; private set; }
    public Guid? ProductionOrderId { get; private set; }
    public ProductionOrder? ProductionOrder { get; private set; }
    public Guid? ProductionOrderProductId { get; private set; }
    public ProductionOrderProduct? ProductionOrderProduct { get; private set; }
    public Guid ProductionProductId { get; private set; }
    public ProductionProduct? ProductionProduct { get; private set; }
    public Guid ProductionOperationId { get; private set; }
    public ProductionOperation? ProductionOperation { get; private set; }
    public Guid? ProductionCellId { get; private set; }
    public ProductionCell? ProductionCell { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitValue { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string RateSource { get; private set; }
    public Guid? ProductionRateId { get; private set; }
    public ProductionRate? ProductionRate { get; private set; }
    public string Origin { get; private set; }
    public string Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public SystemUser? ApprovedByUser { get; private set; }
    public Guid? IntegratedPayrollCalculationId { get; private set; }
    public PayrollCalculation? IntegratedPayrollCalculation { get; private set; }
    public Guid? IntegratedPayrollCalculationItemId { get; private set; }
    public PayrollCalculationItem? IntegratedPayrollCalculationItem { get; private set; }
    public string EmployeeRegistrationSnapshot { get; private set; }
    public string EmployeeNameSnapshot { get; private set; }
    public string? OrderNumberSnapshot { get; private set; }
    public string ProductReferenceSnapshot { get; private set; }
    public string ProductDescriptionSnapshot { get; private set; }
    public string OperationNameSnapshot { get; private set; }
    public string? CellNameSnapshot { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void Approve(Guid? approvedByUserId)
    {
        if (Status is "IntegratedIntoPayroll" or "Canceled")
        {
            throw new InvalidOperationException("A producao nao pode ser aprovada no status atual.");
        }

        Status = "Approved";
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyRate(Guid productionRateId, decimal unitValue)
    {
        if (Status != "Draft")
        {
            throw new InvalidOperationException("Somente producao em rascunho pode ter valor recalculado.");
        }

        if (unitValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitValue), "O valor unitario da producao nao pode ser negativo.");
        }

        ProductionRateId = productionRateId;
        UnitValue = unitValue;
        TotalAmount = Quantity * unitValue;
        RateSource = "RateTable";
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDraft(
        Guid payrollPeriodId,
        Guid employeeId,
        DateOnly productionDate,
        Guid productionProductId,
        Guid productionOperationId,
        decimal quantity,
        string employeeRegistrationSnapshot,
        string employeeNameSnapshot,
        string productReferenceSnapshot,
        string productDescriptionSnapshot,
        string operationNameSnapshot)
    {
        if (Status != "Draft")
        {
            throw new InvalidOperationException("Somente producao em rascunho pode ser editada.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade de producao deve ser maior que zero.");
        }

        PayrollPeriodId = payrollPeriodId;
        EmployeeId = employeeId;
        ProductionDate = productionDate;
        ProductionProductId = productionProductId;
        ProductionOperationId = productionOperationId;
        Quantity = quantity;
        TotalAmount = quantity * UnitValue;
        EmployeeRegistrationSnapshot = employeeRegistrationSnapshot.Trim();
        EmployeeNameSnapshot = employeeNameSnapshot.Trim();
        ProductReferenceSnapshot = productReferenceSnapshot.Trim();
        ProductDescriptionSnapshot = productDescriptionSnapshot.Trim();
        OperationNameSnapshot = operationNameSnapshot.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetContext(
        Guid? productionOrderId,
        string? orderNumberSnapshot,
        Guid? productionOrderProductId,
        Guid? productionCellId,
        string? cellNameSnapshot,
        string? notes)
    {
        if (Status != "Draft")
        {
            throw new InvalidOperationException("Somente producao em rascunho pode ser alterada.");
        }

        ProductionOrderId = productionOrderId;
        OrderNumberSnapshot = string.IsNullOrWhiteSpace(orderNumberSnapshot) ? null : orderNumberSnapshot.Trim();
        ProductionOrderProductId = productionOrderProductId;
        ProductionCellId = productionCellId;
        CellNameSnapshot = string.IsNullOrWhiteSpace(cellNameSnapshot) ? null : cellNameSnapshot.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == "IntegratedIntoPayroll")
        {
            throw new InvalidOperationException("Producao integrada na folha nao pode ser cancelada.");
        }

        Status = "Canceled";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkIntegrated(Guid payrollCalculationId, Guid payrollCalculationItemId)
    {
        if (Status != "Approved")
        {
            throw new InvalidOperationException("Somente producao aprovada pode ser integrada na folha.");
        }

        IntegratedPayrollCalculationId = payrollCalculationId;
        IntegratedPayrollCalculationItemId = payrollCalculationItemId;
        Status = "IntegratedIntoPayroll";
        UpdatedAt = DateTime.UtcNow;
    }
}
