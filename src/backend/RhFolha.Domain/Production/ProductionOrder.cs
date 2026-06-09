using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Production;

public sealed class ProductionOrder : Entity
{
    private ProductionOrder()
    {
        ExternalId = string.Empty;
        Status = string.Empty;
    }

    public ProductionOrder(Guid companyId, string externalId, string? number, string? description)
    {
        CompanyId = companyId;
        ExternalId = externalId.Trim();
        Number = string.IsNullOrWhiteSpace(number) ? null : number.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = "Unknown";
        LastSyncedAt = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string ExternalId { get; private set; }
    public string? Number { get; private set; }
    public string? Description { get; private set; }
    public string Status { get; private set; }
    public DateOnly? IssueDate { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public DateTime? ExternalUpdatedAt { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    public string? RawStatus { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public ICollection<ProductionOrderProduct> Products { get; private set; } = [];
}
