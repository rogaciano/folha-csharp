using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.JobPositions;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/job-positions")]
public sealed class JobPositionsController(RhFolhaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var jobPositions = await dbContext.JobPositions
            .AsNoTracking()
            .Include(jobPosition => jobPosition.Company)
            .OrderBy(jobPosition => jobPosition.Name)
            .Select(jobPosition => new JobPositionResponse(
                jobPosition.Id,
                jobPosition.CompanyId,
                jobPosition.Company!.LegalName,
                jobPosition.Name,
                jobPosition.InternalCode,
                jobPosition.Cbo,
                jobPosition.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(jobPositions);
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost]
    public async Task<IActionResult> Post(CreateJobPositionRequest request, CancellationToken cancellationToken)
    {
        var jobPosition = new JobPosition(request.CompanyId, request.Name, request.InternalCode, request.Cbo);
        dbContext.JobPositions.Add(jobPosition);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = jobPosition.Id }, new JobPositionResponse(
            jobPosition.Id,
            jobPosition.CompanyId,
            string.Empty,
            jobPosition.Name,
            jobPosition.InternalCode,
            jobPosition.Cbo,
            jobPosition.IsActive));
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var jobPosition = await dbContext.JobPositions.FirstOrDefaultAsync(jobPosition => jobPosition.Id == id, cancellationToken);

        if (jobPosition is null)
        {
            return NotFound();
        }

        jobPosition.Activate();
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var hasActiveEmployees = await dbContext.Employees.AnyAsync(
            employee => employee.JobPositionId == id && employee.Status == Domain.Employees.EmployeeStatus.Active,
            cancellationToken);

        if (hasActiveEmployees)
        {
            return BadRequest(new { message = "Cargo possui colaboradores ativos e nao pode ser inativado." });
        }

        var jobPosition = await dbContext.JobPositions.FirstOrDefaultAsync(jobPosition => jobPosition.Id == id, cancellationToken);

        if (jobPosition is null)
        {
            return NotFound();
        }

        jobPosition.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed record CreateJobPositionRequest(Guid CompanyId, string Name, string InternalCode, string? Cbo);
public sealed record JobPositionResponse(Guid Id, Guid CompanyId, string CompanyName, string Name, string InternalCode, string? Cbo, bool IsActive);
