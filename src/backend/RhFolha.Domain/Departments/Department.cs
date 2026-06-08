using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Departments;

public sealed class Department : Entity
{
    private Department()
    {
        Name = string.Empty;
        InternalCode = string.Empty;
    }

    public Department(Guid companyId, string name, string internalCode)
    {
        CompanyId = companyId;
        Name = name.Trim();
        InternalCode = internalCode.Trim();
        IsActive = true;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string Name { get; private set; }
    public string InternalCode { get; private set; }
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
