using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.JobPositions;

public sealed class JobPosition : Entity
{
    private JobPosition()
    {
        Name = string.Empty;
        InternalCode = string.Empty;
    }

    public JobPosition(Guid companyId, string name, string internalCode, string? cbo)
    {
        CompanyId = companyId;
        Name = name.Trim();
        InternalCode = internalCode.Trim();
        Cbo = string.IsNullOrWhiteSpace(cbo) ? null : cbo.Trim();
        IsActive = true;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string Name { get; private set; }
    public string InternalCode { get; private set; }
    public string? Cbo { get; private set; }
    public bool IsActive { get; private set; }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
