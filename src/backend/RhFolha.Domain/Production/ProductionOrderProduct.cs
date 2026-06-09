using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Production;

public sealed class ProductionOrderProduct : Entity
{
    private ProductionOrderProduct()
    {
        ReferenceSnapshot = string.Empty;
        DescriptionSnapshot = string.Empty;
        Status = string.Empty;
    }

    public ProductionOrderProduct(
        Guid companyId,
        Guid productionOrderId,
        Guid productionProductId,
        string referenceSnapshot,
        string descriptionSnapshot,
        decimal? plannedQuantity)
    {
        CompanyId = companyId;
        ProductionOrderId = productionOrderId;
        ProductionProductId = productionProductId;
        ReferenceSnapshot = referenceSnapshot.Trim();
        DescriptionSnapshot = descriptionSnapshot.Trim();
        PlannedQuantity = plannedQuantity;
        Status = "Open";
        LastSyncedAt = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid ProductionOrderId { get; private set; }
    public ProductionOrder? ProductionOrder { get; private set; }
    public Guid ProductionProductId { get; private set; }
    public ProductionProduct? ProductionProduct { get; private set; }
    public string? ExternalId { get; private set; }
    public string ReferenceSnapshot { get; private set; }
    public string DescriptionSnapshot { get; private set; }
    public string? Color { get; private set; }
    public string? Size { get; private set; }
    public string? Grade { get; private set; }
    public decimal? PlannedQuantity { get; private set; }
    public decimal ProducedQuantity { get; private set; }
    public string Status { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}
