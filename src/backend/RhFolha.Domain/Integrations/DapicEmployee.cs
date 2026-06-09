using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Integrations;

public sealed class DapicEmployee : Entity
{
    private DapicEmployee()
    {
        ExternalId = string.Empty;
        Name = string.Empty;
        Status = string.Empty;
    }

    public DapicEmployee(Guid companyId, string externalId, string name, string? fantasyName, string? displayName)
    {
        CompanyId = companyId;
        ExternalId = externalId.Trim();
        Name = name.Trim();
        FantasyName = string.IsNullOrWhiteSpace(fantasyName) ? null : fantasyName.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        Status = "Active";
        LastSyncedAt = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string ExternalId { get; private set; }
    public string Name { get; private set; }
    public string? FantasyName { get; private set; }
    public string? DisplayName { get; private set; }
    public string Status { get; private set; }
    public DateTime? RawUpdatedAt { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    public bool IsIgnored { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void UpdateFromSync(string name, string? fantasyName, string? displayName)
    {
        Name = name.Trim();
        FantasyName = string.IsNullOrWhiteSpace(fantasyName) ? null : fantasyName.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        Status = "Active";
        LastSyncedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
