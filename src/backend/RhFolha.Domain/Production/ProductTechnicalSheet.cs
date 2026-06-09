using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Production;

public sealed class ProductTechnicalSheet : Entity
{
    private ProductTechnicalSheet()
    {
        ExternalProductId = string.Empty;
        Status = string.Empty;
    }

    public ProductTechnicalSheet(Guid companyId, Guid productionProductId, string externalProductId)
    {
        CompanyId = companyId;
        ProductionProductId = productionProductId;
        ExternalProductId = externalProductId.Trim();
        Status = "Active";
        LastSyncedAt = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid ProductionProductId { get; private set; }
    public ProductionProduct? ProductionProduct { get; private set; }
    public string ExternalProductId { get; private set; }
    public string? VersionLabel { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    public string? RawHash { get; private set; }
    public string Status { get; private set; }
    public ICollection<ProductTechnicalSheetOperation> Operations { get; private set; } = [];
}
