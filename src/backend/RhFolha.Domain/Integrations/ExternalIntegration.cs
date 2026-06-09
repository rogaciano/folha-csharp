using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Integrations;

public sealed class ExternalIntegration : Entity
{
    private ExternalIntegration()
    {
        Provider = string.Empty;
        Name = string.Empty;
        BaseUrl = string.Empty;
        ExternalCompanyIdentifier = string.Empty;
        IntegrationTokenSecret = string.Empty;
        Status = string.Empty;
    }

    public ExternalIntegration(
        Guid companyId,
        string provider,
        string name,
        string baseUrl,
        string externalCompanyIdentifier,
        string integrationTokenSecret)
    {
        CompanyId = companyId;
        Provider = provider.Trim();
        Name = name.Trim();
        BaseUrl = baseUrl.Trim();
        ExternalCompanyIdentifier = externalCompanyIdentifier.Trim();
        IntegrationTokenSecret = integrationTokenSecret.Trim();
        Status = "Active";
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string Provider { get; private set; }
    public string Name { get; private set; }
    public string BaseUrl { get; private set; }
    public string ExternalCompanyIdentifier { get; private set; }
    public string IntegrationTokenSecret { get; private set; }
    public string? AccessToken { get; private set; }
    public DateTime? AccessTokenExpiresAt { get; private set; }
    public DateTime? LastSyncAt { get; private set; }
    public string Status { get; private set; }
    public string? LastError { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void SetAccessToken(string accessToken, DateTime expiresAt)
    {
        AccessToken = accessToken.Trim();
        AccessTokenExpiresAt = expiresAt;
        LastError = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSynced()
    {
        LastSyncAt = DateTime.UtcNow;
        LastError = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkError(string error)
    {
        Status = "Error";
        LastError = error.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
