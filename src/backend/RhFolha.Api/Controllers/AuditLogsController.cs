using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleGroups.AdminOnly)]
[Route("api/audit-logs")]
public sealed class AuditLogsController(RhFolhaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var logs = await dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .Take(200)
            .Select(log => new AuditLogResponse(
                log.Id,
                log.CreatedAt,
                log.UserName,
                log.UserEmail,
                log.UserRole,
                log.Action,
                log.EntityName,
                log.EntityId,
                log.Description,
                log.IpAddress))
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }
}

public sealed record AuditLogResponse(
    Guid Id,
    DateTime CreatedAt,
    string UserName,
    string UserEmail,
    string UserRole,
    string Action,
    string EntityName,
    Guid? EntityId,
    string Description,
    string? IpAddress);
