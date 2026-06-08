using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.Departments;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/departments")]
public sealed class DepartmentsController(RhFolhaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var departments = await dbContext.Departments
            .AsNoTracking()
            .Include(department => department.Company)
            .OrderBy(department => department.Name)
            .Select(department => new DepartmentResponse(
                department.Id,
                department.CompanyId,
                department.Company!.LegalName,
                department.Name,
                department.InternalCode,
                department.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(departments);
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost]
    public async Task<IActionResult> Post(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var department = new Department(request.CompanyId, request.Name, request.InternalCode);
        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = department.Id }, new DepartmentResponse(
            department.Id,
            department.CompanyId,
            string.Empty,
            department.Name,
            department.InternalCode,
            department.IsActive));
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var department = await dbContext.Departments.FirstOrDefaultAsync(department => department.Id == id, cancellationToken);

        if (department is null)
        {
            return NotFound();
        }

        department.Activate();
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var hasActiveEmployees = await dbContext.Employees.AnyAsync(
            employee => employee.DepartmentId == id && employee.Status == Domain.Employees.EmployeeStatus.Active,
            cancellationToken);

        if (hasActiveEmployees)
        {
            return BadRequest(new { message = "Setor possui colaboradores ativos e nao pode ser inativado." });
        }

        var department = await dbContext.Departments.FirstOrDefaultAsync(department => department.Id == id, cancellationToken);

        if (department is null)
        {
            return NotFound();
        }

        department.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed record CreateDepartmentRequest(Guid CompanyId, string Name, string InternalCode);
public sealed record DepartmentResponse(Guid Id, Guid CompanyId, string CompanyName, string Name, string InternalCode, bool IsActive);
