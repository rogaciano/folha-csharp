using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Departments;

namespace RhFolha.Domain.Production;

public sealed class ProductionCell : Entity
{
    private ProductionCell()
    {
        ExternalId = string.Empty;
        Name = string.Empty;
        Status = string.Empty;
    }

    public ProductionCell(Guid companyId, string externalId, string name)
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
    public Guid? DepartmentId { get; private set; }
    public Department? Department { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void UpdateFromSync(string name, string? description, string status)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = string.IsNullOrWhiteSpace(status) ? "Unknown" : status.Trim();
        LastSyncedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
