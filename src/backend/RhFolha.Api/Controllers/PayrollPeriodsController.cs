using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Application.Payroll;
using RhFolha.Api.Security;
using RhFolha.Domain.Payroll;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/payroll-periods")]
public sealed class PayrollPeriodsController(RhFolhaDbContext dbContext, AuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var periods = await dbContext.PayrollPeriods
            .AsNoTracking()
            .OrderByDescending(period => period.Year)
            .ThenByDescending(period => period.Month)
            .Select(period => new PayrollPeriodResponse(
                period.Id,
                period.CompanyId,
                period.Year,
                period.Month,
                period.Code,
                period.StartsOn,
                period.EndsOn,
                period.Status,
                period.OpenedAt,
                period.ClosedAt))
            .ToListAsync(cancellationToken);

        return Ok(periods);
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost]
    public async Task<IActionResult> Post(CreatePayrollPeriodRequest request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.PayrollPeriods.AnyAsync(
            period => period.CompanyId == request.CompanyId && period.Year == request.Year && period.Month == request.Month,
            cancellationToken);

        if (exists)
        {
            return Conflict(new { message = "Competencia ja cadastrada para esta empresa." });
        }

        var period = new PayrollPeriod(request.CompanyId, request.Year, request.Month);

        dbContext.PayrollPeriods.Add(period);
        auditService.Add("payroll_period.create", "PayrollPeriod", period.Id, $"Competencia {period.Code} aberta.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = period.Id }, new PayrollPeriodResponse(
            period.Id,
            period.CompanyId,
            period.Year,
            period.Month,
            period.Code,
            period.StartsOn,
            period.EndsOn,
            period.Status,
            period.OpenedAt,
            period.ClosedAt));
    }

    [Authorize(Roles = RoleGroups.AdminOnly)]
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var period = await dbContext.PayrollPeriods.FirstOrDefaultAsync(period => period.Id == id, cancellationToken);

        if (period is null)
        {
            return NotFound();
        }

        try
        {
            period.Close();
            auditService.Add("payroll_period.close", "PayrollPeriod", period.Id, $"Competencia {period.Code} fechada.");
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.PayrollApproval)]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var period = await dbContext.PayrollPeriods.FirstOrDefaultAsync(period => period.Id == id, cancellationToken);

        if (period is null)
        {
            return NotFound();
        }

        var hasCurrentCalculation = await dbContext.PayrollCalculations.AnyAsync(
            calculation => calculation.PayrollPeriodId == id && calculation.IsCurrent,
            cancellationToken);

        if (!hasCurrentCalculation)
        {
            return BadRequest(new { message = "Competencia precisa ter uma folha calculada antes da aprovacao." });
        }

        try
        {
            period.Approve();
            auditService.Add("payroll_period.approve", "PayrollPeriod", period.Id, $"Folha da competencia {period.Code} aprovada.");
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.PayrollWork)]
    [HttpPost("{id:guid}/calculate")]
    public async Task<IActionResult> Calculate(Guid id, CancellationToken cancellationToken)
    {
        var period = await dbContext.PayrollPeriods.FirstOrDefaultAsync(period => period.Id == id, cancellationToken);

        if (period is null)
        {
            return NotFound();
        }

        if (!period.CanCalculate)
        {
            return BadRequest(new { message = "Competencia nao pode ser calculada no status atual." });
        }

        var salaryRubric = await dbContext.Rubrics
            .AsNoTracking()
            .FirstOrDefaultAsync(
                rubric => rubric.CompanyId == period.CompanyId && rubric.Code == "001" && rubric.IsActive,
                cancellationToken);

        if (salaryRubric is null)
        {
            return BadRequest(new { message = "Rubrica de salario mensal codigo 001 nao encontrada ou inativa." });
        }

        var productionRubric = await dbContext.Rubrics
            .AsNoTracking()
            .FirstOrDefaultAsync(
                rubric => rubric.CompanyId == period.CompanyId && rubric.Code == "002" && rubric.IsActive,
                cancellationToken);

        if (productionRubric is null)
        {
            return BadRequest(new { message = "Rubrica de producao codigo 002 nao encontrada ou inativa." });
        }

        var fgtsRubric = await dbContext.Rubrics
            .AsNoTracking()
            .FirstOrDefaultAsync(
                rubric => rubric.CompanyId == period.CompanyId && rubric.Code == "903" && rubric.IsActive,
                cancellationToken);

        if (fgtsRubric is null)
        {
            return BadRequest(new { message = "Rubrica informativa de FGTS codigo 903 nao encontrada ou inativa." });
        }

        var inssRubric = await dbContext.Rubrics
            .AsNoTracking()
            .FirstOrDefaultAsync(
                rubric => rubric.CompanyId == period.CompanyId && rubric.Code == "901" && rubric.IsActive,
                cancellationToken);

        if (inssRubric is null)
        {
            return BadRequest(new { message = "Rubrica de INSS codigo 901 nao encontrada ou inativa." });
        }

        var irrfRubric = await dbContext.Rubrics
            .AsNoTracking()
            .FirstOrDefaultAsync(
                rubric => rubric.CompanyId == period.CompanyId && rubric.Code == "902" && rubric.IsActive,
                cancellationToken);

        if (irrfRubric is null)
        {
            return BadRequest(new { message = "Rubrica de IRRF codigo 902 nao encontrada ou inativa." });
        }

        var fgtsTable = await dbContext.StatutoryTables
            .AsNoTracking()
            .Include(table => table.Ranges)
            .Where(table =>
                table.CompanyId == period.CompanyId &&
                table.Type == "fgts" &&
                table.IsActive &&
                table.StartsOn <= period.EndsOn &&
                (!table.EndsOn.HasValue || table.EndsOn.Value >= period.StartsOn))
            .OrderByDescending(table => table.StartsOn)
            .FirstOrDefaultAsync(cancellationToken);

        var fgtsRate = fgtsTable?.Ranges.OrderBy(range => range.LowerLimit).FirstOrDefault()?.RatePercent;

        if (fgtsRate is null)
        {
            return BadRequest(new { message = "Tabela legal de FGTS vigente nao encontrada." });
        }

        var inssTable = await dbContext.StatutoryTables
            .AsNoTracking()
            .Include(table => table.Ranges)
            .Where(table =>
                table.CompanyId == period.CompanyId &&
                table.Type == "inss" &&
                table.IsActive &&
                table.StartsOn <= period.EndsOn &&
                (!table.EndsOn.HasValue || table.EndsOn.Value >= period.StartsOn))
            .OrderByDescending(table => table.StartsOn)
            .FirstOrDefaultAsync(cancellationToken);

        if (inssTable is null || inssTable.Ranges.Count == 0)
        {
            return BadRequest(new { message = "Tabela legal de INSS vigente nao encontrada." });
        }

        var irrfTable = await dbContext.StatutoryTables
            .AsNoTracking()
            .Include(table => table.Ranges)
            .Where(table =>
                table.CompanyId == period.CompanyId &&
                table.Type == "irrf" &&
                table.IsActive &&
                table.StartsOn <= period.EndsOn &&
                (!table.EndsOn.HasValue || table.EndsOn.Value >= period.StartsOn))
            .OrderByDescending(table => table.StartsOn)
            .FirstOrDefaultAsync(cancellationToken);

        if (irrfTable is null || irrfTable.Ranges.Count == 0)
        {
            return BadRequest(new { message = "Tabela legal de IRRF vigente nao encontrada." });
        }

        var irrfReductionTable = await dbContext.StatutoryTables
            .AsNoTracking()
            .Include(table => table.Ranges)
            .Where(table =>
                table.CompanyId == period.CompanyId &&
                table.Type == "irrf_reducao" &&
                table.IsActive &&
                table.StartsOn <= period.EndsOn &&
                (!table.EndsOn.HasValue || table.EndsOn.Value >= period.StartsOn))
            .OrderByDescending(table => table.StartsOn)
            .FirstOrDefaultAsync(cancellationToken);

        var fgtsIncidenceRubricIds = await dbContext.RubricValidities
            .AsNoTracking()
            .Where(validity =>
                validity.IsActive &&
                validity.IncidenceFgts &&
                validity.Rubric != null &&
                validity.Rubric.CompanyId == period.CompanyId &&
                validity.StartsOn <= period.EndsOn &&
                (!validity.EndsOn.HasValue || validity.EndsOn.Value >= period.StartsOn))
            .Select(validity => validity.RubricId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var irrfIncidenceRubricIds = await dbContext.RubricValidities
            .AsNoTracking()
            .Where(validity =>
                validity.IsActive &&
                validity.IncidenceIrrf &&
                validity.Rubric != null &&
                validity.Rubric.CompanyId == period.CompanyId &&
                validity.StartsOn <= period.EndsOn &&
                (!validity.EndsOn.HasValue || validity.EndsOn.Value >= period.StartsOn))
            .Select(validity => validity.RubricId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var inssIncidenceRubricIds = await dbContext.RubricValidities
            .AsNoTracking()
            .Where(validity =>
                validity.IsActive &&
                validity.IncidenceInss &&
                validity.Rubric != null &&
                validity.Rubric.CompanyId == period.CompanyId &&
                validity.StartsOn <= period.EndsOn &&
                (!validity.EndsOn.HasValue || validity.EndsOn.Value >= period.StartsOn))
            .Select(validity => validity.RubricId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var employees = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.CompanyId == period.CompanyId && employee.Status == Domain.Employees.EmployeeStatus.Active)
            .OrderBy(employee => employee.Name)
            .ToListAsync(cancellationToken);

        var payrollEntries = await dbContext.PayrollEntries
            .AsNoTracking()
            .Include(entry => entry.Rubric)
            .Where(entry => entry.CompanyId == period.CompanyId && entry.PayrollPeriodId == period.Id && entry.Status == "aprovado")
            .ToListAsync(cancellationToken);

        var fixedEntries = await dbContext.FixedPayrollEntries
            .AsNoTracking()
            .Include(entry => entry.Rubric)
            .Where(entry =>
                entry.CompanyId == period.CompanyId &&
                entry.IsActive &&
                entry.StartsOn <= period.EndsOn &&
                (!entry.EndsOn.HasValue || entry.EndsOn.Value >= period.StartsOn))
            .ToListAsync(cancellationToken);

        var hasDraftProductionEntries = await dbContext.EmployeeProductionEntries.AnyAsync(
            entry =>
                entry.CompanyId == period.CompanyId &&
                entry.PayrollPeriodId == period.Id &&
                entry.DeletedAt == null &&
                entry.Status == "Draft",
            cancellationToken);

        if (hasDraftProductionEntries)
        {
            return BadRequest(new { message = "Existem apontamentos de producao em rascunho nesta competencia. Aprove ou cancele antes de calcular a folha." });
        }

        var previousCurrentCalculations = await dbContext.PayrollCalculations
            .Where(calculation =>
                calculation.CompanyId == period.CompanyId &&
                calculation.PayrollPeriodId == period.Id &&
                calculation.IsCurrent)
            .ToListAsync(cancellationToken);
        var previousCurrentCalculationIds = previousCurrentCalculations.Select(calculation => calculation.Id).ToHashSet();

        var productionEntries = await dbContext.EmployeeProductionEntries
            .Where(entry =>
                entry.CompanyId == period.CompanyId &&
                entry.PayrollPeriodId == period.Id &&
                entry.DeletedAt == null &&
                (entry.Status == "Approved" ||
                    (entry.Status == "IntegratedIntoPayroll" &&
                        entry.IntegratedPayrollCalculationId.HasValue &&
                        previousCurrentCalculationIds.Contains(entry.IntegratedPayrollCalculationId.Value))))
            .ToListAsync(cancellationToken);

        foreach (var productionEntry in productionEntries.Where(entry => entry.Status == "IntegratedIntoPayroll"))
        {
            productionEntry.ReopenIntegration();
        }

        foreach (var calculation in previousCurrentCalculations)
        {
            calculation.MarkAsHistorical();
        }

        var newCalculation = new PayrollCalculation(period.CompanyId, period.Id, period.Code);
        var items = new List<PayrollCalculationItem>();
        var employeesById = employees.ToDictionary(employee => employee.Id);

        foreach (var employee in employees.Where(employee => employee.CompensationModel == "mensalista" && employee.BaseSalary > 0))
        {
            items.Add(new PayrollCalculationItem(
                newCalculation.Id,
                employee.Id,
                employee.Registration,
                employee.Name,
                salaryRubric.Id,
                salaryRubric.Code,
                salaryRubric.Name,
                salaryRubric.Type,
                "salario",
                employee.BaseSalary,
                null));
        }

        foreach (var entry in payrollEntries)
        {
            if (entry.Rubric is null || !employeesById.TryGetValue(entry.EmployeeId, out var employee))
            {
                continue;
            }

            items.Add(new PayrollCalculationItem(
                newCalculation.Id,
                employee.Id,
                employee.Registration,
                employee.Name,
                entry.RubricId,
                entry.Rubric.Code,
                entry.Rubric.Name,
                entry.Rubric.Type,
                entry.Origin,
                entry.Amount,
                entry.Quantity));
        }

        foreach (var productionGroup in productionEntries.GroupBy(entry => entry.EmployeeId))
        {
            if (!employeesById.TryGetValue(productionGroup.Key, out var employee))
            {
                continue;
            }

            var quantity = productionGroup.Sum(entry => entry.Quantity);
            var amount = productionGroup.Sum(entry => entry.TotalAmount);
            if (amount <= 0)
            {
                continue;
            }

            var productionItem = new PayrollCalculationItem(
                newCalculation.Id,
                employee.Id,
                employee.Registration,
                employee.Name,
                productionRubric.Id,
                productionRubric.Code,
                productionRubric.Name,
                productionRubric.Type,
                "producao",
                amount,
                quantity);

            items.Add(productionItem);

            foreach (var productionEntry in productionGroup)
            {
                productionEntry.MarkIntegrated(newCalculation.Id, productionItem.Id);
            }
        }

        foreach (var fixedEntry in fixedEntries)
        {
            if (fixedEntry.Rubric is null || !employeesById.TryGetValue(fixedEntry.EmployeeId, out var employee))
            {
                continue;
            }

            items.Add(new PayrollCalculationItem(
                newCalculation.Id,
                employee.Id,
                employee.Registration,
                employee.Name,
                fixedEntry.RubricId,
                fixedEntry.Rubric.Code,
                fixedEntry.Rubric.Name,
                fixedEntry.Rubric.Type,
                "fixo",
                fixedEntry.Amount,
                fixedEntry.Quantity));
        }

        var fgtsIncidenceRubricSet = fgtsIncidenceRubricIds.ToHashSet();
        var fgtsBasesByEmployee = items
            .Where(item => fgtsIncidenceRubricSet.Contains(item.RubricId))
            .GroupBy(item => item.EmployeeId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Amount));

        foreach (var employee in employees)
        {
            if (!fgtsBasesByEmployee.TryGetValue(employee.Id, out var fgtsBase) || fgtsBase <= 0)
            {
                continue;
            }

            var fgtsAmount = LegalPayrollCalculator.CalculatePercentage(fgtsBase, fgtsRate.Value);

            items.Add(new PayrollCalculationItem(
                newCalculation.Id,
                employee.Id,
                employee.Registration,
                employee.Name,
                fgtsRubric.Id,
                fgtsRubric.Code,
                fgtsRubric.Name,
                fgtsRubric.Type,
                "sistema_fgts",
                fgtsAmount,
                null,
                fgtsBase,
                fgtsRate.Value));
        }

        var inssIncidenceRubricSet = inssIncidenceRubricIds.ToHashSet();
        var inssBasesByEmployee = items
            .Where(item => inssIncidenceRubricSet.Contains(item.RubricId))
            .GroupBy(item => item.EmployeeId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Amount));

        foreach (var employee in employees)
        {
            if (!inssBasesByEmployee.TryGetValue(employee.Id, out var inssBase) || inssBase <= 0)
            {
                continue;
            }

            var inssAmount = LegalPayrollCalculator.CalculateProgressiveTableAmount(inssBase, ToLegalRanges(inssTable.Ranges));

            if (inssAmount <= 0)
            {
                continue;
            }

            items.Add(new PayrollCalculationItem(
                newCalculation.Id,
                employee.Id,
                employee.Registration,
                employee.Name,
                inssRubric.Id,
                inssRubric.Code,
                inssRubric.Name,
                inssRubric.Type,
                "sistema_inss",
                inssAmount,
                null,
                inssBase,
                null));
        }

        var irrfIncidenceRubricSet = irrfIncidenceRubricIds.ToHashSet();
        var irrfGrossBasesByEmployee = items
            .Where(item => irrfIncidenceRubricSet.Contains(item.RubricId))
            .GroupBy(item => item.EmployeeId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Amount));
        var inssAmountsByEmployee = items
            .Where(item => item.RubricId == inssRubric.Id)
            .GroupBy(item => item.EmployeeId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Amount));

        foreach (var employee in employees)
        {
            if (!irrfGrossBasesByEmployee.TryGetValue(employee.Id, out var irrfGrossBase) || irrfGrossBase <= 0)
            {
                continue;
            }

            var inssAmount = inssAmountsByEmployee.GetValueOrDefault(employee.Id);
            var irrfBase = Math.Max(0m, irrfGrossBase - inssAmount);
            var progressiveAmount = LegalPayrollCalculator.CalculateProgressiveTableAmount(irrfBase, ToLegalRanges(irrfTable.Ranges));
            var reductionAmount = irrfReductionTable is null
                ? 0m
                : LegalPayrollCalculator.CalculateIrrfReductionAmount(irrfBase, progressiveAmount, ToLegalRanges(irrfReductionTable.Ranges));
            var irrfAmount = Math.Max(0m, Math.Round(progressiveAmount - reductionAmount, 2, MidpointRounding.AwayFromZero));

            if (irrfAmount <= 0)
            {
                continue;
            }

            items.Add(new PayrollCalculationItem(
                newCalculation.Id,
                employee.Id,
                employee.Registration,
                employee.Name,
                irrfRubric.Id,
                irrfRubric.Code,
                irrfRubric.Name,
                irrfRubric.Type,
                "sistema_irrf",
                irrfAmount,
                null,
                irrfBase,
                null));
        }

        var totalProventos = items.Where(item => item.RubricType == "provento").Sum(item => item.Amount);
        var totalDescontos = items.Where(item => item.RubricType == "desconto").Sum(item => item.Amount);
        var employeeCount = items.Select(item => item.EmployeeId).Distinct().Count();

        newCalculation.SetTotals(totalProventos, totalDescontos, employeeCount);
        period.MarkCalculated();

        dbContext.PayrollCalculations.Add(newCalculation);
        dbContext.PayrollCalculationItems.AddRange(items);
        auditService.Add("payroll_period.calculate", "PayrollPeriod", period.Id, $"Folha da competencia {period.Code} calculada.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new PayrollCalculationSummaryResponse(
            newCalculation.Id,
            newCalculation.PayrollPeriodId,
            newCalculation.PeriodCode,
            newCalculation.CalculatedAt,
            newCalculation.TotalProventos,
            newCalculation.TotalDescontos,
            newCalculation.TotalLiquido,
            newCalculation.EmployeeCount,
            newCalculation.Status,
            newCalculation.IsCurrent));
    }

    [Authorize(Roles = RoleGroups.AdminOnly)]
    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken cancellationToken)
    {
        var period = await dbContext.PayrollPeriods.FirstOrDefaultAsync(period => period.Id == id, cancellationToken);

        if (period is null)
        {
            return NotFound();
        }

        try
        {
            period.Reopen();
            auditService.Add("payroll_period.reopen", "PayrollPeriod", period.Id, $"Competencia {period.Code} reaberta.");
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        return NoContent();
    }

    private static IEnumerable<LegalTableRange> ToLegalRanges(IEnumerable<StatutoryTableRange> ranges)
    {
        return ranges.Select(range => new LegalTableRange(
            range.LowerLimit,
            range.UpperLimit,
            range.RatePercent,
            range.DeductionAmount));
    }
}

public sealed record CreatePayrollPeriodRequest(Guid CompanyId, int Year, int Month);

public sealed record PayrollPeriodResponse(
    Guid Id,
    Guid CompanyId,
    int Year,
    int Month,
    string Code,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string Status,
    DateTime OpenedAt,
    DateTime? ClosedAt);

public sealed record PayrollCalculationSummaryResponse(
    Guid Id,
    Guid PayrollPeriodId,
    string PeriodCode,
    DateTime CalculatedAt,
    decimal TotalProventos,
    decimal TotalDescontos,
    decimal TotalLiquido,
    int EmployeeCount,
    string Status,
    bool IsCurrent);
