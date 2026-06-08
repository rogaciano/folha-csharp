using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.Payroll;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/payroll-entries")]
public sealed class PayrollEntriesController(RhFolhaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var entries = await dbContext.PayrollEntries
            .AsNoTracking()
            .Include(entry => entry.PayrollPeriod)
            .Include(entry => entry.Employee)
            .Include(entry => entry.Rubric)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenBy(entry => entry.Employee!.Name)
            .Select(entry => new PayrollEntryResponse(
                entry.Id,
                entry.CompanyId,
                entry.PayrollPeriodId,
                entry.PayrollPeriod!.Code,
                entry.EmployeeId,
                entry.Employee!.Registration,
                entry.Employee.Name,
                entry.RubricId,
                entry.Rubric!.Code,
                entry.Rubric.Name,
                entry.Rubric.Type,
                entry.EntryDate,
                entry.Amount,
                entry.Quantity,
                entry.Reference,
                entry.Notes,
                entry.Origin,
                entry.Status))
            .ToListAsync(cancellationToken);

        return Ok(entries);
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost]
    public async Task<IActionResult> Post(CreatePayrollEntryRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateEntryHeader(
            request.CompanyId,
            request.PayrollPeriodId,
            request.RubricId,
            request.EntryDate,
            requireMassEntry: false,
            cancellationToken);

        if (validation.Result is not null)
        {
            return validation.Result;
        }

        var employeeExists = await EmployeeExists(request.CompanyId, request.EmployeeId, cancellationToken);

        if (!employeeExists)
        {
            return BadRequest(new { message = "Colaborador nao encontrado para a empresa informada." });
        }

        try
        {
            var entry = new PayrollEntry(
                request.CompanyId,
                request.PayrollPeriodId,
                request.EmployeeId,
                request.RubricId,
                request.EntryDate,
                request.Amount,
                request.Quantity,
                request.Reference,
                request.Notes);

            dbContext.PayrollEntries.Add(entry);
            await dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { id = entry.Id }, null);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("mass")]
    public async Task<IActionResult> PostMass(CreateMassPayrollEntriesRequest request, CancellationToken cancellationToken)
    {
        if (request.Entries.Count == 0)
        {
            return BadRequest(new { message = "Informe pelo menos um lancamento." });
        }

        var validation = await ValidateEntryHeader(
            request.CompanyId,
            request.PayrollPeriodId,
            request.RubricId,
            request.EntryDate,
            requireMassEntry: true,
            cancellationToken);

        if (validation.Result is not null)
        {
            return validation.Result;
        }

        var employeeIds = request.Entries.Select(entry => entry.EmployeeId).Distinct().ToList();
        var validEmployeeIds = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.CompanyId == request.CompanyId && employeeIds.Contains(employee.Id))
            .Select(employee => employee.Id)
            .ToListAsync(cancellationToken);

        if (validEmployeeIds.Count != employeeIds.Count)
        {
            return BadRequest(new { message = "Um ou mais colaboradores nao pertencem a empresa informada." });
        }

        try
        {
            var entries = request.Entries.Select(item => new PayrollEntry(
                request.CompanyId,
                request.PayrollPeriodId,
                item.EmployeeId,
                request.RubricId,
                request.EntryDate,
                item.Amount,
                item.Quantity,
                request.Reference,
                item.Notes,
                "massa"));

            dbContext.PayrollEntries.AddRange(entries);
            await dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), null);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private async Task<(IActionResult? Result, PayrollPeriod? Period, Rubric? Rubric)> ValidateEntryHeader(
        Guid companyId,
        Guid payrollPeriodId,
        Guid rubricId,
        DateOnly entryDate,
        bool requireMassEntry,
        CancellationToken cancellationToken)
    {
        var period = await dbContext.PayrollPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(period => period.Id == payrollPeriodId, cancellationToken);

        if (period is null)
        {
            return (BadRequest(new { message = "Competencia nao encontrada." }), null, null);
        }

        if (period.Status is not ("aberta" or "reaberta"))
        {
            return (BadRequest(new { message = "Lancamentos so podem ser feitos em competencia aberta ou reaberta." }), null, null);
        }

        if (period.CompanyId != companyId)
        {
            return (BadRequest(new { message = "Competencia nao pertence a empresa informada." }), null, null);
        }

        if (entryDate < period.StartsOn || entryDate > period.EndsOn)
        {
            return (BadRequest(new { message = "Data do lancamento deve estar dentro da competencia." }), null, null);
        }

        var rubric = await dbContext.Rubrics
            .AsNoTracking()
            .FirstOrDefaultAsync(
                rubric => rubric.Id == rubricId && rubric.CompanyId == companyId,
                cancellationToken);

        if (rubric is null)
        {
            return (BadRequest(new { message = "Rubrica nao encontrada para a empresa informada." }), null, null);
        }

        if (!rubric.AllowsManualEntry)
        {
            return (BadRequest(new { message = "Rubrica nao permite lancamento manual." }), null, null);
        }

        if (requireMassEntry && !rubric.AllowsMassEntry)
        {
            return (BadRequest(new { message = "Rubrica nao permite lancamento em massa." }), null, null);
        }

        return (null, period, rubric);
    }

    private Task<bool> EmployeeExists(Guid companyId, Guid employeeId, CancellationToken cancellationToken)
    {
        return dbContext.Employees.AnyAsync(
            employee => employee.Id == employeeId && employee.CompanyId == companyId,
            cancellationToken);
    }
}

public sealed record CreatePayrollEntryRequest(
    Guid CompanyId,
    Guid PayrollPeriodId,
    Guid EmployeeId,
    Guid RubricId,
    DateOnly EntryDate,
    decimal Amount,
    decimal? Quantity,
    string? Reference,
    string? Notes);

public sealed record CreateMassPayrollEntriesRequest(
    Guid CompanyId,
    Guid PayrollPeriodId,
    Guid RubricId,
    DateOnly EntryDate,
    string? Reference,
    IReadOnlyCollection<CreateMassPayrollEntryItemRequest> Entries);

public sealed record CreateMassPayrollEntryItemRequest(
    Guid EmployeeId,
    decimal Amount,
    decimal? Quantity,
    string? Notes);

public sealed record PayrollEntryResponse(
    Guid Id,
    Guid CompanyId,
    Guid PayrollPeriodId,
    string PayrollPeriodCode,
    Guid EmployeeId,
    string EmployeeRegistration,
    string EmployeeName,
    Guid RubricId,
    string RubricCode,
    string RubricName,
    string RubricType,
    DateOnly EntryDate,
    decimal Amount,
    decimal? Quantity,
    string? Reference,
    string? Notes,
    string Origin,
    string Status);
