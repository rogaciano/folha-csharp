using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Production;

public sealed class ProductionOperation : Entity
{
    private ProductionOperation()
    {
        ExternalId = string.Empty;
        Name = string.Empty;
        Status = string.Empty;
    }

    public ProductionOperation(Guid companyId, string externalId, string name)
    {
        CompanyId = companyId;
        ExternalId = externalId.Trim();
        Name = name.Trim();
        Status = "Active";
        LastSyncedAt = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string ExternalId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Status { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}
