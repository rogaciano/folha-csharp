using RhFolha.Domain.Common;

namespace RhFolha.Domain.Security;

public sealed class SystemUser : Entity
{
    private SystemUser()
    {
        FullName = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        Role = string.Empty;
    }

    public SystemUser(Guid? companyId, string fullName, string email, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Nome do usuario e obrigatorio.", nameof(fullName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("E-mail do usuario e obrigatorio.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Hash da senha e obrigatorio.", nameof(passwordHash));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Perfil do usuario e obrigatorio.", nameof(role));
        }

        CompanyId = companyId;
        FullName = fullName.Trim();
        Email = NormalizeEmail(email);
        PasswordHash = passwordHash;
        Role = NormalizeRole(role);
        IsActive = true;
    }

    public Guid? CompanyId { get; private set; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public void UpdateProfile(Guid? companyId, string fullName, string role)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Nome do usuario e obrigatorio.", nameof(fullName));
        }

        CompanyId = companyId;
        FullName = fullName.Trim();
        Role = NormalizeRole(role);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Hash da senha e obrigatorio.", nameof(passwordHash));
        }

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegisterLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public static string NormalizeRole(string role)
    {
        var normalizedRole = role.Trim().ToLowerInvariant();
        if (!SystemRoles.All.Contains(normalizedRole))
        {
            throw new ArgumentException("Perfil de usuario invalido.", nameof(role));
        }

        return normalizedRole;
    }
}
