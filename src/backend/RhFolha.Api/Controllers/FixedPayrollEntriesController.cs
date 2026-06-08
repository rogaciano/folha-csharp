using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Api.Security;
using RhFolha.Domain.Payroll;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/fixed-payroll-entries")]
public sealed class FixedPayrollEntriesController(RhFolhaDbContext dbContext, AuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var entries = await dbContext.FixedPayrollEntries
            .AsNoTracking()
            .Include(entry => entry.Employee)
            .Include(entry => entry.Rubric)
            .OrderBy(entry => entry.Employee!.Name)
            .ThenBy(entry => entry.Rubric!.Code)
            .Select(entry => new FixedPayrollEntryResponse(
                entry.Id,
                entry.CompanyId,
                entry.EmployeeId,
                entry.Employee!.Registration,
                entry.Employee.Name,
                entry.RubricId,
                entry.Rubric!.Code,
                entry.Rubric.Name,
                entry.Rubric.Type,
                entry.StartsOn,
                entry.EndsOn,
                entry.Amount,
                entry.Quantity,
                entry.Notes,
                entry.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(entries);
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost]
    public async Task<IActionResult> Post(CreateFixedPayrollEntryRequest request, CancellationToken cancellationToken)
    {
        var employeeExists = await dbContext.Employees.AnyAsync(
            employee => employee.Id == request.EmployeeId && employee.CompanyId == request.CompanyId,
            cancellationToken);

        if (!employeeExists)
        {
            return BadRequest(new { message = "Colaborador nao encontrado para a empresa informada." });
        }

        var rubric = await dbContext.Rubrics
            .AsNoTracking()
            .FirstOrDefaultAsync(
                rubric => rubric.Id == request.RubricId && rubric.CompanyId == request.CompanyId,
                cancellationToken);

        if (rubric is null)
        {
            return BadRequest(new { message = "Rubrica nao encontrada para a empresa informada." });
        }

        if (!rubric.AllowsFixedEntry)
        {
            return BadRequest(new { message = "Rubrica nao permite lancamento fixo." });
        }

        try
        {
            var entry = new FixedPayrollEntry(
                request.CompanyId,
                request.EmployeeId,
                request.RubricId,
                request.StartsOn,
                request.EndsOn,
                request.Amount,
                request.Quantity,
                request.Notes);

            dbContext.FixedPayrollEntries.Add(entry);
            auditService.Add("fixed_entry.create", "FixedPayrollEntry", entry.Id, "Lancamento fixo criado.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { id = entry.Id }, null);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/update")]
    public async Task<IActionResult> Update(Guid id, UpdateFixedPayrollEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await dbContext.FixedPayrollEntries.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        var rubric = await dbContext.Rubrics
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == request.RubricId && item.CompanyId == entry.CompanyId,
                cancellationToken);

        if (rubric is null)
        {
            return BadRequest(new { message = "Rubrica nao encontrada para a empresa informada." });
        }

        if (!rubric.AllowsFixedEntry)
        {
            return BadRequest(new { message = "Rubrica nao permite lancamento fixo." });
        }

        try
        {
            entry.Update(request.RubricId, request.StartsOn, request.EndsOn, request.Amount, request.Quantity, request.Notes);
            auditService.Add("fixed_entry.update", "FixedPayrollEntry", entry.Id, "Lancamento fixo atualizado.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CloseFixedPayrollEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await dbContext.FixedPayrollEntries.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        try
        {
            entry.Close(request.EndsOn);
            auditService.Add("fixed_entry.close", "FixedPayrollEntry", entry.Id, $"Lancamento fixo encerrado em {request.EndsOn:yyyy-MM-dd}.");
            await dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var entry = await dbContext.FixedPayrollEntries.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        entry.Deactivate();
        auditService.Add("fixed_entry.deactivate", "FixedPayrollEntry", entry.Id, "Lancamento fixo inativado.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var entry = await dbContext.FixedPayrollEntries.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        entry.Activate();
        auditService.Add("fixed_entry.activate", "FixedPayrollEntry", entry.Id, "Lancamento fixo reativado.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed record CreateFixedPayrollEntryRequest(
    Guid CompanyId,
    Guid EmployeeId,
    Guid RubricId,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    decimal Amount,
    decimal? Quantity,
    string? Notes);

public sealed record UpdateFixedPayrollEntryRequest(
    Guid RubricId,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    decimal Amount,
    decimal? Quantity,
    string? Notes);

public sealed record CloseFixedPayrollEntryRequest(DateOnly EndsOn);

public sealed record FixedPayrollEntryResponse(
    Guid Id,
    Guid CompanyId,
    Guid EmployeeId,
    string EmployeeRegistration,
    string EmployeeName,
    Guid RubricId,
    string RubricCode,
    string RubricName,
    string RubricType,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    decimal Amount,
    decimal? Quantity,
    string? Notes,
    bool IsActive);
