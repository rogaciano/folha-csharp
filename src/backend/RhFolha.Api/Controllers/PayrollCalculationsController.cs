using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/payroll-calculations")]
public sealed class PayrollCalculationsController(RhFolhaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var calculations = await dbContext.PayrollCalculations
            .AsNoTracking()
            .Where(calculation => calculation.IsCurrent)
            .OrderByDescending(calculation => calculation.CalculatedAt)
            .Select(calculation => new PayrollCalculationListResponse(
                calculation.Id,
                calculation.PayrollPeriodId,
                calculation.PeriodCode,
                calculation.CalculatedAt,
                calculation.TotalProventos,
                calculation.TotalDescontos,
                calculation.TotalLiquido,
                calculation.EmployeeCount,
                calculation.Status))
            .ToListAsync(cancellationToken);

        return Ok(calculations);
    }

    [HttpGet("period/{payrollPeriodId:guid}/current")]
    public async Task<IActionResult> GetCurrentByPeriod(Guid payrollPeriodId, CancellationToken cancellationToken)
    {
        var calculation = await dbContext.PayrollCalculations
            .AsNoTracking()
            .Include(calculation => calculation.Items)
            .Where(calculation => calculation.PayrollPeriodId == payrollPeriodId && calculation.IsCurrent)
            .OrderByDescending(calculation => calculation.CalculatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (calculation is null)
        {
            return NotFound();
        }

        return Ok(new PayrollCalculationDetailResponse(
            calculation.Id,
            calculation.PayrollPeriodId,
            calculation.PeriodCode,
            calculation.CalculatedAt,
            calculation.TotalProventos,
            calculation.TotalDescontos,
            calculation.TotalLiquido,
            calculation.EmployeeCount,
            calculation.Status,
            calculation.Items
                .OrderBy(item => item.EmployeeName)
                .ThenBy(item => item.RubricCode)
                .Select(item => new PayrollCalculationItemResponse(
                    item.Id,
                    item.EmployeeId,
                    item.EmployeeRegistration,
                    item.EmployeeName,
                    item.RubricId,
                    item.RubricCode,
                    item.RubricName,
                    item.RubricType,
                    item.Origin,
                    item.Amount,
                    item.Quantity,
                    item.BaseAmount,
                    item.CalculationRate))
                .ToList()));
    }
}

public sealed record PayrollCalculationListResponse(
    Guid Id,
    Guid PayrollPeriodId,
    string PeriodCode,
    DateTime CalculatedAt,
    decimal TotalProventos,
    decimal TotalDescontos,
    decimal TotalLiquido,
    int EmployeeCount,
    string Status);

public sealed record PayrollCalculationDetailResponse(
    Guid Id,
    Guid PayrollPeriodId,
    string PeriodCode,
    DateTime CalculatedAt,
    decimal TotalProventos,
    decimal TotalDescontos,
    decimal TotalLiquido,
    int EmployeeCount,
    string Status,
    IReadOnlyCollection<PayrollCalculationItemResponse> Items);

public sealed record PayrollCalculationItemResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeRegistration,
    string EmployeeName,
    Guid RubricId,
    string RubricCode,
    string RubricName,
    string RubricType,
    string Origin,
    decimal Amount,
    decimal? Quantity,
    decimal? BaseAmount,
    decimal? CalculationRate);
