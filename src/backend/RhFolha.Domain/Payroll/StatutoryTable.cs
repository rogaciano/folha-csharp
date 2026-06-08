using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Payroll;

public sealed class StatutoryTable : Entity
{
    private StatutoryTable()
    {
        Type = string.Empty;
        Name = string.Empty;
    }

    public StatutoryTable(
        Guid companyId,
        string type,
        string name,
        DateOnly startsOn,
        DateOnly? endsOn,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Tipo da tabela legal deve ser informado.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Nome da tabela legal deve ser informado.", nameof(name));
        }

        if (endsOn.HasValue && endsOn.Value < startsOn)
        {
            throw new ArgumentException("Fim da vigencia nao pode ser anterior ao inicio.");
        }

        CompanyId = companyId;
        Type = type.Trim().ToLowerInvariant();
        Name = name.Trim();
        StartsOn = startsOn;
        EndsOn = endsOn;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsActive = true;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public string Type { get; private set; }
    public string Name { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<StatutoryTableRange> Ranges { get; private set; } = [];

    public void AddRange(decimal lowerLimit, decimal? upperLimit, decimal ratePercent, decimal deductionAmount)
    {
        Ranges.Add(new StatutoryTableRange(Id, lowerLimit, upperLimit, ratePercent, deductionAmount));
        UpdatedAt = DateTime.UtcNow;
    }

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
