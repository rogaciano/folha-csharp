using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.Payroll;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/rubrics")]
public sealed class RubricsController(RhFolhaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var rubrics = await dbContext.Rubrics
            .AsNoTracking()
            .Include(rubric => rubric.Validities)
            .OrderBy(rubric => rubric.Code)
            .Select(rubric => new RubricResponse(
                rubric.Id,
                rubric.CompanyId,
                rubric.Code,
                rubric.Name,
                rubric.Type,
                rubric.ESocialNature,
                rubric.AllowsManualEntry,
                rubric.AllowsMassEntry,
                rubric.AllowsFixedEntry,
                rubric.IsActive,
                rubric.Validities
                    .OrderByDescending(validity => validity.StartsOn)
                    .Select(validity => new RubricValidityResponse(
                        validity.Id,
                        validity.StartsOn,
                        validity.EndsOn,
                        validity.IncidenceInss,
                        validity.IncidenceFgts,
                        validity.IncidenceIrrf,
                        validity.IncidenceDsr,
                        validity.CalculationMethod,
                        validity.CalculationBase,
                        validity.IsActive))
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(rubrics);
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost]
    public async Task<IActionResult> Post(CreateRubricRequest request, CancellationToken cancellationToken)
    {
        var rubric = new Rubric(
            request.CompanyId,
            request.Code,
            request.Name,
            request.Type,
            request.Description,
            request.ESocialNature,
            request.AllowsManualEntry,
            request.AllowsMassEntry,
            request.AllowsFixedEntry);

        var validity = new RubricValidity(
            rubric.Id,
            request.StartsOn,
            null,
            request.IncidenceInss,
            request.IncidenceFgts,
            request.IncidenceIrrf,
            request.IncidenceDsr,
            request.CalculationMethod,
            request.CalculationBase);

        dbContext.Rubrics.Add(rubric);
        dbContext.RubricValidities.Add(validity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = rubric.Id }, null);
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var rubric = await dbContext.Rubrics.FirstOrDefaultAsync(rubric => rubric.Id == id, cancellationToken);

        if (rubric is null)
        {
            return NotFound();
        }

        rubric.Activate();
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var hasEntries = await dbContext.PayrollEntries.AnyAsync(entry => entry.RubricId == id, cancellationToken);
        var hasFixedEntries = await dbContext.FixedPayrollEntries.AnyAsync(entry => entry.RubricId == id && entry.IsActive, cancellationToken);

        if (hasFixedEntries)
        {
            return BadRequest(new { message = "Rubrica possui lancamentos fixos ativos e nao pode ser inativada." });
        }

        var rubric = await dbContext.Rubrics.FirstOrDefaultAsync(rubric => rubric.Id == id, cancellationToken);

        if (rubric is null)
        {
            return NotFound();
        }

        rubric.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken);

        return hasEntries ? Ok(new { message = "Rubrica inativada. Lancamentos historicos foram preservados." }) : NoContent();
    }
}

public sealed record CreateRubricRequest(
    Guid CompanyId,
    string Code,
    string Name,
    string Type,
    string? Description,
    string? ESocialNature,
    bool AllowsManualEntry,
    bool AllowsMassEntry,
    bool AllowsFixedEntry,
    DateOnly StartsOn,
    bool IncidenceInss,
    bool IncidenceFgts,
    bool IncidenceIrrf,
    bool IncidenceDsr,
    string CalculationMethod,
    string CalculationBase);

public sealed record RubricResponse(
    Guid Id,
    Guid CompanyId,
    string Code,
    string Name,
    string Type,
    string? ESocialNature,
    bool AllowsManualEntry,
    bool AllowsMassEntry,
    bool AllowsFixedEntry,
    bool IsActive,
    RubricValidityResponse? CurrentValidity);

public sealed record RubricValidityResponse(
    Guid Id,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    bool IncidenceInss,
    bool IncidenceFgts,
    bool IncidenceIrrf,
    bool IncidenceDsr,
    string CalculationMethod,
    string CalculationBase,
    bool IsActive);
