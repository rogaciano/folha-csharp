using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Api.Security;
using RhFolha.Domain.Production;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/production-rate-tables")]
public sealed class ProductionRateTablesController(
    RhFolhaDbContext dbContext,
    AuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var tables = await dbContext.ProductionRateTables
            .AsNoTracking()
            .Include(table => table.Rates.Where(rate => rate.DeletedAt == null))
                .ThenInclude(rate => rate.ProductionProduct)
            .Include(table => table.Rates.Where(rate => rate.DeletedAt == null))
                .ThenInclude(rate => rate.ProductionOperation)
            .Include(table => table.Rates.Where(rate => rate.DeletedAt == null))
                .ThenInclude(rate => rate.ProductionCell)
            .Include(table => table.Rates.Where(rate => rate.DeletedAt == null))
                .ThenInclude(rate => rate.Department)
            .Include(table => table.Rates.Where(rate => rate.DeletedAt == null))
                .ThenInclude(rate => rate.JobPosition)
            .Where(table => table.DeletedAt == null)
            .OrderByDescending(table => table.EffectiveFrom)
            .ThenBy(table => table.Name)
            .ToListAsync(cancellationToken);

        return Ok(tables.Select(table => new ProductionRateTableResponse(
            table.Id,
            table.CompanyId,
            table.Name,
            table.EffectiveFrom,
            table.EffectiveTo,
            table.Status,
            table.Notes,
            table.Rates
                .OrderBy(rate => rate.ProductionProduct?.Reference)
                .ThenBy(rate => rate.ProductionOperation?.Name)
                .Select(rate => new ProductionRateResponse(
                    rate.Id,
                    rate.ProductionProductId,
                    rate.ProductionProduct?.Reference,
                    rate.ProductionProduct?.FactoryDescription,
                    rate.ProductionOperationId,
                    rate.ProductionOperation?.Name,
                    rate.ProductionCellId,
                    rate.ProductionCell?.Name,
                    rate.DepartmentId,
                    rate.Department?.Name,
                    rate.JobPositionId,
                    rate.JobPosition?.Name,
                    rate.UnitValue,
                    rate.MinimumQuantity,
                    rate.MaximumQuantity,
                    rate.Status,
                    rate.Notes))
                .ToList())));
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost]
    public async Task<IActionResult> Post(CreateProductionRateTableRequest request, CancellationToken cancellationToken)
    {
        if (!await dbContext.Companies.AnyAsync(company => company.Id == request.CompanyId, cancellationToken))
        {
            return BadRequest(new { message = "Empresa informada nao encontrada." });
        }

        if (request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom)
        {
            return BadRequest(new { message = "Fim de vigencia nao pode ser menor que o inicio." });
        }

        var table = new ProductionRateTable(request.CompanyId, request.Name, request.EffectiveFrom, request.EffectiveTo);
        table.Update(request.Name, request.EffectiveFrom, request.EffectiveTo, request.Notes);

        foreach (var rateRequest in request.Rates.Where(rate => rate.UnitValue > 0))
        {
            var validation = await ValidateRateReferences(request.CompanyId, rateRequest, cancellationToken);
            if (validation is not null)
            {
                return BadRequest(new { message = validation });
            }

            var rate = new ProductionRate(request.CompanyId, table.Id, rateRequest.UnitValue);
            rate.ConfigureCriteria(
                rateRequest.ProductionProductId,
                rateRequest.ProductionOperationId,
                rateRequest.ProductionCellId,
                rateRequest.DepartmentId,
                rateRequest.JobPositionId,
                rateRequest.MinimumQuantity,
                rateRequest.MaximumQuantity,
                rateRequest.Notes);
            table.Rates.Add(rate);
        }

        if (table.Rates.Count == 0)
        {
            return BadRequest(new { message = "Informe pelo menos uma linha de valor." });
        }

        dbContext.ProductionRateTables.Add(table);
        auditService.Add("production_rate_table.create", "ProductionRateTable", table.Id, $"Tabela de producao {table.Name} criada.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = table.Id }, new { table.Id });
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var table = await dbContext.ProductionRateTables.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (table is null)
        {
            return NotFound();
        }

        table.Activate();
        auditService.Add("production_rate_table.activate", "ProductionRateTable", table.Id, $"Tabela de producao {table.Name} ativada.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var table = await dbContext.ProductionRateTables.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (table is null)
        {
            return NotFound();
        }

        table.Deactivate();
        auditService.Add("production_rate_table.deactivate", "ProductionRateTable", table.Id, $"Tabela de producao {table.Name} inativada.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<string?> ValidateRateReferences(Guid companyId, CreateProductionRateRequest request, CancellationToken cancellationToken)
    {
        if (request.MaximumQuantity.HasValue && request.MinimumQuantity.HasValue && request.MaximumQuantity < request.MinimumQuantity)
        {
            return "Quantidade maxima nao pode ser menor que a minima.";
        }

        if (request.ProductionProductId.HasValue && !await dbContext.ProductionProducts.AnyAsync(item => item.Id == request.ProductionProductId && item.CompanyId == companyId, cancellationToken))
        {
            return "Produto informado nao encontrado.";
        }

        if (request.ProductionOperationId.HasValue && !await dbContext.ProductionOperations.AnyAsync(item => item.Id == request.ProductionOperationId && item.CompanyId == companyId, cancellationToken))
        {
            return "Operacao informada nao encontrada.";
        }

        if (request.ProductionCellId.HasValue && !await dbContext.ProductionCells.AnyAsync(item => item.Id == request.ProductionCellId && item.CompanyId == companyId, cancellationToken))
        {
            return "Celula informada nao encontrada.";
        }

        if (request.DepartmentId.HasValue && !await dbContext.Departments.AnyAsync(item => item.Id == request.DepartmentId && item.CompanyId == companyId, cancellationToken))
        {
            return "Setor informado nao encontrado.";
        }

        if (request.JobPositionId.HasValue && !await dbContext.JobPositions.AnyAsync(item => item.Id == request.JobPositionId && item.CompanyId == companyId, cancellationToken))
        {
            return "Cargo informado nao encontrado.";
        }

        return null;
    }
}

public sealed record CreateProductionRateTableRequest(
    Guid CompanyId,
    string Name,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string? Notes,
    IReadOnlyList<CreateProductionRateRequest> Rates);

public sealed record CreateProductionRateRequest(
    Guid? ProductionProductId,
    Guid? ProductionOperationId,
    Guid? ProductionCellId,
    Guid? DepartmentId,
    Guid? JobPositionId,
    decimal UnitValue,
    decimal? MinimumQuantity,
    decimal? MaximumQuantity,
    string? Notes);

public sealed record ProductionRateTableResponse(
    Guid Id,
    Guid CompanyId,
    string Name,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Status,
    string? Notes,
    IReadOnlyList<ProductionRateResponse> Rates);

public sealed record ProductionRateResponse(
    Guid Id,
    Guid? ProductionProductId,
    string? ProductReference,
    string? ProductDescription,
    Guid? ProductionOperationId,
    string? OperationName,
    Guid? ProductionCellId,
    string? CellName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? JobPositionId,
    string? JobPositionName,
    decimal UnitValue,
    decimal? MinimumQuantity,
    decimal? MaximumQuantity,
    string Status,
    string? Notes);
