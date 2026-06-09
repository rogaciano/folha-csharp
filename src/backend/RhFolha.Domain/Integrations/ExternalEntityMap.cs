using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Security;

namespace RhFolha.Domain.Integrations;

public sealed class ExternalEntityMap : Entity
{
    private ExternalEntityMap()
    {
        Provider = string.Empty;
        ExternalEntityType = string.Empty;
        ExternalId = string.Empty;
        LocalEntityType = string.Empty;
        ExternalDisplayName = string.Empty;
        Status = string.Empty;
    }

    public ExternalEntityMap(
        Guid companyId,
        string provider,
        string externalEntityType,
        string externalId,
        string localEntityType,
        Guid localEntityId,
        string externalDisplayName,
        Guid? linkedByUserId)
    {
        CompanyId = companyId;
        Provider = provider.Trim();
        ExternalEntityType = externalEntityType.Trim();
        ExternalId = externalId.Trim();
        LocalEntityType = localEntityType.Trim();
        LocalEntityId = localEntityId;
        ExternalDisplayName = externalDisplayName.Trim();
        LinkedByUserId = linkedByUserId;
        LinkedAt = DateTime.UtcNow;
        Status = "Linked";
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string Provider { get; private set; }
    public string ExternalEntityType { get; private set; }
    public string ExternalId { get; private set; }
    public string LocalEntityType { get; private set; }
    public Guid LocalEntityId { get; private set; }
    public string ExternalDisplayName { get; private set; }
    public string Status { get; private set; }
    public DateTime? LinkedAt { get; private set; }
    public Guid? LinkedByUserId { get; private set; }
    public SystemUser? LinkedByUser { get; private set; }
    public string? Notes { get; private set; }
}
