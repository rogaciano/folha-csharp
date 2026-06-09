using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Security;

namespace RhFolha.Domain.Integrations;

public sealed class ExternalSyncLog : Entity
{
    private ExternalSyncLog()
    {
        Provider = string.Empty;
        Resource = string.Empty;
        Status = string.Empty;
    }

    public ExternalSyncLog(Guid companyId, Guid externalIntegrationId, string provider, string resource, Guid? createdByUserId)
    {
        CompanyId = companyId;
        ExternalIntegrationId = externalIntegrationId;
        Provider = provider.Trim();
        Resource = resource.Trim();
        CreatedByUserId = createdByUserId;
        StartedAt = DateTime.UtcNow;
        Status = "Running";
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid ExternalIntegrationId { get; private set; }
    public ExternalIntegration? ExternalIntegration { get; private set; }
    public string Provider { get; private set; }
    public string Resource { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public string Status { get; private set; }
    public DateOnly? RequestedFrom { get; private set; }
    public DateOnly? RequestedTo { get; private set; }
    public int PageCount { get; private set; }
    public int RecordsRead { get; private set; }
    public int RecordsCreated { get; private set; }
    public int RecordsUpdated { get; private set; }
    public int RecordsIgnored { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public SystemUser? CreatedByUser { get; private set; }

    public void Finish(
        string status,
        int pageCount,
        int recordsRead,
        int recordsCreated,
        int recordsUpdated,
        int recordsIgnored,
        string? errorMessage = null)
    {
        Status = status.Trim();
        PageCount = pageCount;
        RecordsRead = recordsRead;
        RecordsCreated = recordsCreated;
        RecordsUpdated = recordsUpdated;
        RecordsIgnored = recordsIgnored;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim();
        FinishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
