using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Production;

public sealed class ProductionRateTable : Entity
{
    private ProductionRateTable()
    {
        Name = string.Empty;
        Status = string.Empty;
    }

    public ProductionRateTable(Guid companyId, string name, DateOnly effectiveFrom, DateOnly? effectiveTo = null)
    {
        CompanyId = companyId;
        Name = name.Trim();
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Status = "Draft";
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string Name { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public ICollection<ProductionRate> Rates { get; private set; } = [];
}
