using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Payroll;

public sealed class Rubric : Entity
{
    private Rubric()
    {
        Code = string.Empty;
        Name = string.Empty;
        Type = string.Empty;
    }

    public Rubric(
        Guid companyId,
        string code,
        string name,
        string type,
        string? description,
        string? esocialNature,
        bool allowsManualEntry,
        bool allowsMassEntry,
        bool allowsFixedEntry)
    {
        CompanyId = companyId;
        Code = code.Trim();
        Name = name.Trim();
        Type = type.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ESocialNature = string.IsNullOrWhiteSpace(esocialNature) ? null : esocialNature.Trim();
        AllowsManualEntry = allowsManualEntry;
        AllowsMassEntry = allowsMassEntry;
        AllowsFixedEntry = allowsFixedEntry;
        IsActive = true;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Type { get; private set; }
    public string? Description { get; private set; }
    public string? ESocialNature { get; private set; }
    public bool AllowsManualEntry { get; private set; }
    public bool AllowsMassEntry { get; private set; }
    public bool AllowsFixedEntry { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<RubricValidity> Validities { get; private set; } = [];

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
