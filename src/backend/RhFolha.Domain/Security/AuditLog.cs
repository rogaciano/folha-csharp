using RhFolha.Domain.Common;

namespace RhFolha.Domain.Security;

public sealed class AuditLog : Entity
{
    private AuditLog()
    {
        Action = string.Empty;
        EntityName = string.Empty;
        Description = string.Empty;
        UserName = string.Empty;
        UserEmail = string.Empty;
        UserRole = string.Empty;
    }

    public AuditLog(
        Guid? userId,
        string userName,
        string userEmail,
        string userRole,
        string action,
        string entityName,
        Guid? entityId,
        string description,
        string? ipAddress)
    {
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
        UserRole = userRole;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        Description = description;
        IpAddress = ipAddress;
    }

    public Guid? UserId { get; private set; }
    public string UserName { get; private set; }
    public string UserEmail { get; private set; }
    public string UserRole { get; private set; }
    public string Action { get; private set; }
    public string EntityName { get; private set; }
    public Guid? EntityId { get; private set; }
    public string Description { get; private set; }
    public string? IpAddress { get; private set; }
}
