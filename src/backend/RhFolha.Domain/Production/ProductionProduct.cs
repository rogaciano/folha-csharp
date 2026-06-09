using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Production;

public sealed class ProductionProduct : Entity
{
    private ProductionProduct()
    {
        ExternalId = string.Empty;
        Reference = string.Empty;
        FactoryDescription = string.Empty;
        Status = string.Empty;
    }

    public ProductionProduct(Guid companyId, string externalId, string reference, string factoryDescription)
    {
        CompanyId = companyId;
        ExternalId = externalId.Trim();
        Reference = reference.Trim();
        FactoryDescription = factoryDescription.Trim();
        Status = "Active";
        LastSyncedAt = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string ExternalId { get; private set; }
    public string Reference { get; private set; }
    public string FactoryDescription { get; private set; }
    public string Status { get; private set; }
    public DateTime? ExternalCreatedAt { get; private set; }
    public DateTime? ExternalUpdatedAt { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}
