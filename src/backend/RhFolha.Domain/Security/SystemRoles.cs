namespace RhFolha.Domain.Security;

public static class SystemRoles
{
    public const string Administrator = "administrador";
    public const string HumanResources = "rh_operacional";
    public const string Reviewer = "conferente";
    public const string ReadOnly = "leitura";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Administrator,
        HumanResources,
        Reviewer,
        ReadOnly
    };
}

public static class RoleGroups
{
    public const string AdminOnly = SystemRoles.Administrator;
    public const string HrOperations = $"{SystemRoles.Administrator},{SystemRoles.HumanResources}";
    public const string PayrollWork = $"{SystemRoles.Administrator},{SystemRoles.HumanResources},{SystemRoles.Reviewer}";
    public const string PayrollApproval = $"{SystemRoles.Administrator},{SystemRoles.Reviewer}";
}
