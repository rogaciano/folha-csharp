using RhFolha.Domain.Common;
using RhFolha.Domain.Companies;

namespace RhFolha.Domain.Production;

public sealed class ProductTechnicalSheetOperation : Entity
{
    private ProductTechnicalSheetOperation()
    {
        OperationNameSnapshot = string.Empty;
    }

    public ProductTechnicalSheetOperation(
        Guid companyId,
        Guid productTechnicalSheetId,
        Guid productionOperationId,
        string operationNameSnapshot,
        int sequence)
    {
        CompanyId = companyId;
        ProductTechnicalSheetId = productTechnicalSheetId;
        ProductionOperationId = productionOperationId;
        OperationNameSnapshot = operationNameSnapshot.Trim();
        Sequence = sequence;
    }

    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid ProductTechnicalSheetId { get; private set; }
    public ProductTechnicalSheet? ProductTechnicalSheet { get; private set; }
    public Guid ProductionOperationId { get; private set; }
    public ProductionOperation? ProductionOperation { get; private set; }
    public string OperationNameSnapshot { get; private set; }
    public int Sequence { get; private set; }
    public decimal? StandardQuantity { get; private set; }
    public decimal? StandardTime { get; private set; }
    public string? Notes { get; private set; }
}
