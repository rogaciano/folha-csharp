using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Api.Security;
using RhFolha.Domain.Employees;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController(
    RhFolhaDbContext dbContext,
    IWebHostEnvironment environment,
    AuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var employees = await dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.Department)
            .Include(employee => employee.JobPosition)
            .Include(employee => employee.ResponsibleEmployee)
            .OrderBy(employee => employee.Name)
            .Select(employee => new EmployeeResponse(
                employee.Id,
                employee.CompanyId,
                employee.DepartmentId,
                employee.Department!.Name,
                employee.JobPositionId,
                employee.JobPosition!.Name,
                employee.ResponsibleEmployeeId,
                employee.ResponsibleEmployee != null ? employee.ResponsibleEmployee.Name : null,
                employee.Registration,
                employee.Name,
                employee.DocumentNumber,
                employee.AdmissionDate,
                employee.CompensationModel,
                employee.BaseSalary,
                employee.ProductionUnitValue,
                employee.PhotoUrl,
                employee.Status.ToString()))
            .ToListAsync(cancellationToken);

        return Ok(employees);
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost]
    public async Task<IActionResult> Post(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        if (request.ResponsibleEmployeeId.HasValue)
        {
            var responsibleExists = await dbContext.Employees.AnyAsync(
                employee => employee.Id == request.ResponsibleEmployeeId.Value && employee.CompanyId == request.CompanyId,
                cancellationToken);

            if (!responsibleExists)
            {
                return BadRequest("Responsavel informado nao existe na mesma empresa.");
            }
        }

        var employee = new Employee(
            request.CompanyId,
            request.DepartmentId,
            request.JobPositionId,
            request.Registration,
            request.Name,
            request.DocumentNumber,
            request.AdmissionDate,
            request.CompensationModel,
            request.BaseSalary,
            request.ProductionUnitValue,
            request.ResponsibleEmployeeId);
        employee.UpdatePhoto(request.PhotoUrl);

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = employee.Id }, new EmployeeResponse(
            employee.Id,
            employee.CompanyId,
            employee.DepartmentId,
            string.Empty,
            employee.JobPositionId,
            string.Empty,
            employee.ResponsibleEmployeeId,
            null,
            employee.Registration,
            employee.Name,
            employee.DocumentNumber,
            employee.AdmissionDate,
            employee.CompensationModel,
            employee.BaseSalary,
            employee.ProductionUnitValue,
            employee.PhotoUrl,
            employee.Status.ToString()));
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/photo")]
    public async Task<IActionResult> UpdatePhoto(Guid id, UpdateEmployeePhotoRequest request, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (employee is null)
        {
            return NotFound();
        }

        employee.UpdatePhoto(request.PhotoUrl);
        auditService.Add("employee.photo_update", "Employee", employee.Id, $"Foto do colaborador {employee.Name} atualizada por URL.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/responsible")]
    public async Task<IActionResult> UpdateResponsible(Guid id, UpdateEmployeeResponsibleRequest request, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (employee is null)
        {
            return NotFound();
        }

        if (request.ResponsibleEmployeeId.HasValue)
        {
            var responsibleExists = await dbContext.Employees.AnyAsync(
                item => item.Id == request.ResponsibleEmployeeId.Value && item.CompanyId == employee.CompanyId && item.Id != employee.Id,
                cancellationToken);

            if (!responsibleExists)
            {
                return BadRequest("Responsavel informado nao existe na mesma empresa.");
            }
        }

        employee.UpdateResponsible(request.ResponsibleEmployeeId);
        auditService.Add("employee.responsible_update", "Employee", employee.Id, $"Responsavel direto do colaborador {employee.Name} atualizado.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/photo-upload")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (employee is null)
        {
            return NotFound();
        }

        if (file.Length == 0)
        {
            return BadRequest("Arquivo vazio.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Formato de foto nao suportado.");
        }

        var uploadDirectory = Path.Combine(environment.ContentRootPath, "data", "uploads", "employees");
        Directory.CreateDirectory(uploadDirectory);

        foreach (var oldFile in Directory.GetFiles(uploadDirectory, $"{employee.Id}.*"))
        {
            System.IO.File.Delete(oldFile);
        }

        var fileName = $"{employee.Id}{extension}";
        var physicalPath = Path.Combine(uploadDirectory, fileName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        employee.UpdatePhoto($"/uploads/employees/{fileName}");
        auditService.Add("employee.photo_upload", "Employee", employee.Id, $"Foto do colaborador {employee.Name} enviada por upload.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { employee.PhotoUrl });
    }

    [HttpGet("{id:guid}/events")]
    public async Task<IActionResult> GetEvents(Guid id, CancellationToken cancellationToken)
    {
        var employeeExists = await dbContext.Employees.AnyAsync(employee => employee.Id == id, cancellationToken);

        if (!employeeExists)
        {
            return NotFound();
        }

        var events = await dbContext.EmployeeEvents
            .AsNoTracking()
            .Where(employeeEvent => employeeEvent.EmployeeId == id)
            .OrderByDescending(employeeEvent => employeeEvent.EventDate)
            .ThenByDescending(employeeEvent => employeeEvent.CreatedAt)
            .Select(employeeEvent => new EmployeeEventResponse(
                employeeEvent.Id,
                employeeEvent.CompanyId,
                employeeEvent.EmployeeId,
                employeeEvent.EventDate,
                employeeEvent.Type,
                employeeEvent.Title,
                employeeEvent.Description,
                employeeEvent.Responsible,
                employeeEvent.Status,
                employeeEvent.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(events);
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("{id:guid}/events")]
    public async Task<IActionResult> CreateEvent(Guid id, CreateEmployeeEventRequest request, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (employee is null)
        {
            return NotFound();
        }

        var employeeEvent = new EmployeeEvent(
            employee.CompanyId,
            employee.Id,
            request.EventDate,
            request.Type,
            request.Title,
            request.Description,
            request.Responsible);

        dbContext.EmployeeEvents.Add(employeeEvent);
        auditService.Add("employee_event.create", "EmployeeEvent", employeeEvent.Id, $"Evento {employeeEvent.Title} registrado para {employee.Name}.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetEvents), new { id = employee.Id }, new EmployeeEventResponse(
            employeeEvent.Id,
            employeeEvent.CompanyId,
            employeeEvent.EmployeeId,
            employeeEvent.EventDate,
            employeeEvent.Type,
            employeeEvent.Title,
            employeeEvent.Description,
            employeeEvent.Responsible,
            employeeEvent.Status,
            employeeEvent.CreatedAt));
    }

    [Authorize(Roles = RoleGroups.HrOperations)]
    [HttpPost("events/{eventId:guid}/cancel")]
    public async Task<IActionResult> CancelEvent(Guid eventId, CancellationToken cancellationToken)
    {
        var employeeEvent = await dbContext.EmployeeEvents.FirstOrDefaultAsync(item => item.Id == eventId, cancellationToken);

        if (employeeEvent is null)
        {
            return NotFound();
        }

        employeeEvent.Cancel();
        auditService.Add("employee_event.cancel", "EmployeeEvent", employeeEvent.Id, $"Evento {employeeEvent.Title} cancelado.");
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed record CreateEmployeeRequest(
    Guid CompanyId,
    Guid DepartmentId,
    Guid JobPositionId,
    string Registration,
    string Name,
    string DocumentNumber,
    DateOnly AdmissionDate,
    string CompensationModel,
    decimal BaseSalary,
    decimal ProductionUnitValue,
    Guid? ResponsibleEmployeeId,
    string? PhotoUrl);

public sealed record UpdateEmployeePhotoRequest(string? PhotoUrl);

public sealed record UpdateEmployeeResponsibleRequest(Guid? ResponsibleEmployeeId);

public sealed record CreateEmployeeEventRequest(
    DateOnly EventDate,
    string Type,
    string Title,
    string Description,
    string Responsible);

public sealed record EmployeeResponse(
    Guid Id,
    Guid CompanyId,
    Guid DepartmentId,
    string DepartmentName,
    Guid JobPositionId,
    string JobPositionName,
    Guid? ResponsibleEmployeeId,
    string? ResponsibleEmployeeName,
    string Registration,
    string Name,
    string DocumentNumber,
    DateOnly AdmissionDate,
    string CompensationModel,
    decimal BaseSalary,
    decimal ProductionUnitValue,
    string? PhotoUrl,
    string Status);

public sealed record EmployeeEventResponse(
    Guid Id,
    Guid CompanyId,
    Guid EmployeeId,
    DateOnly EventDate,
    string Type,
    string Title,
    string Description,
    string Responsible,
    string Status,
    DateTime CreatedAt);
