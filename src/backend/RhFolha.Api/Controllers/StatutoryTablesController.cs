using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Api.Security;
using RhFolha.Domain.Payroll;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/statutory-tables")]
public sealed class StatutoryTablesController(RhFolhaDbContext dbContext, AuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var tables = await dbContext.StatutoryTables
            .AsNoTracking()
            .Include(table => table.Ranges)
            .OrderBy(table => table.Type)
            .ThenByDescending(table => table.StartsOn)
            .Select(table => new StatutoryTableResponse(
                table.Id,
                table.CompanyId,
                table.Type,
                table.Name,
                table.StartsOn,
                table.EndsOn,
                table.Notes,
                table.IsActive,
                table.Ranges
                    .OrderBy(range => range.LowerLimit)
                    .Select(range => new StatutoryTableRangeResponse(
                        range.Id,
                        range.LowerLimit,
                        range.UpperLimit,
                        range.RatePercent,
                        range.DeductionAmount))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return Ok(tables);
    }

    [Authorize(Roles = RoleGroups.AdminOnly)]
    [HttpPost]
    public async Task<IActionResult> Post(CreateStatutoryTableRequest request, CancellationToken cancellationToken)
    {
        if (request.Ranges.Count == 0)
        {
            return BadRequest(new { message = "Informe pelo menos uma faixa para a tabela legal." });
        }

        var table = new StatutoryTable(
            request.CompanyId,
            request.Type,
            request.Name,
            request.StartsOn,
            request.EndsOn,
            request.Notes);

        foreach (var range in request.Ranges.OrderBy(range => range.LowerLimit))
        {
            table.AddRange(range.LowerLimit, range.UpperLimit, range.RatePercent, range.DeductionAmount);
        }

        dbContext.StatutoryTables.Add(table);
        auditService.Add("statutory_table.create", "StatutoryTable", table.Id, $"Tabela legal {table.Name} criada.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = table.Id }, null);
    }

    [Authorize(Roles = RoleGroups.AdminOnly)]
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var table = await dbContext.StatutoryTables.FirstOrDefaultAsync(table => table.Id == id, cancellationToken);

        if (table is null)
        {
            return NotFound();
        }

        table.Activate();
        auditService.Add("statutory_table.activate", "StatutoryTable", table.Id, $"Tabela legal {table.Name} reativada.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.AdminOnly)]
    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> Duplicate(Guid id, DuplicateStatutoryTableRequest request, CancellationToken cancellationToken)
    {
        var source = await dbContext.StatutoryTables
            .Include(table => table.Ranges)
            .FirstOrDefaultAsync(table => table.Id == id, cancellationToken);

        if (source is null)
        {
            return NotFound();
        }

        if (request.StartsOn <= source.StartsOn)
        {
            return BadRequest(new { message = "A nova vigencia deve ser posterior ao inicio da tabela original." });
        }

        var alreadyExists = await dbContext.StatutoryTables.AnyAsync(
            table => table.CompanyId == source.CompanyId && table.Type == source.Type && table.StartsOn == request.StartsOn,
            cancellationToken);

        if (alreadyExists)
        {
            return BadRequest(new { message = "Ja existe uma tabela desse tipo com a vigencia informada." });
        }

        var duplicate = new StatutoryTable(
            source.CompanyId,
            source.Type,
            string.IsNullOrWhiteSpace(request.Name) ? $"{source.Name} - nova vigencia" : request.Name,
            request.StartsOn,
            request.EndsOn,
            string.IsNullOrWhiteSpace(request.Notes) ? source.Notes : request.Notes);

        foreach (var range in source.Ranges.OrderBy(range => range.LowerLimit))
        {
            duplicate.AddRange(range.LowerLimit, range.UpperLimit, range.RatePercent, range.DeductionAmount);
        }

        dbContext.StatutoryTables.Add(duplicate);
        auditService.Add("statutory_table.duplicate", "StatutoryTable", duplicate.Id, $"Nova vigencia criada a partir da tabela {source.Name}.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = duplicate.Id }, null);
    }

    [Authorize(Roles = RoleGroups.AdminOnly)]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var table = await dbContext.StatutoryTables.FirstOrDefaultAsync(table => table.Id == id, cancellationToken);

        if (table is null)
        {
            return NotFound();
        }

        table.Deactivate();
        auditService.Add("statutory_table.deactivate", "StatutoryTable", table.Id, $"Tabela legal {table.Name} inativada.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Tabela legal inativada. Historico e faixas foram preservados." });
    }
}

public sealed record CreateStatutoryTableRequest(
    Guid CompanyId,
    string Type,
    string Name,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    string? Notes,
    IReadOnlyCollection<CreateStatutoryTableRangeRequest> Ranges);

public sealed record CreateStatutoryTableRangeRequest(
    decimal LowerLimit,
    decimal? UpperLimit,
    decimal RatePercent,
    decimal DeductionAmount);

public sealed record DuplicateStatutoryTableRequest(
    string? Name,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    string? Notes);

public sealed record StatutoryTableResponse(
    Guid Id,
    Guid CompanyId,
    string Type,
    string Name,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    string? Notes,
    bool IsActive,
    IReadOnlyCollection<StatutoryTableRangeResponse> Ranges);

public sealed record StatutoryTableRangeResponse(
    Guid Id,
    decimal LowerLimit,
    decimal? UpperLimit,
    decimal RatePercent,
    decimal DeductionAmount);
