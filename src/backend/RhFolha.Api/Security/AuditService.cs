using System.Security.Claims;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Security;

public sealed class AuditService(RhFolhaDbContext dbContext, IHttpContextAccessor httpContextAccessor)
{
    public void Add(string action, string entityName, Guid? entityId, string description)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdValue = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : (Guid?)null;
        var userName = user?.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
        var userEmail = user?.FindFirstValue(ClaimTypes.Email) ?? "sistema";
        var userRole = user?.FindFirstValue(ClaimTypes.Role) ?? "sistema";
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        dbContext.AuditLogs.Add(new AuditLog(
            userId,
            userName,
            userEmail,
            userRole,
            action,
            entityName,
            entityId,
            description,
            ipAddress));
    }
}
