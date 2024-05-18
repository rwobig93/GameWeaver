namespace Application.Constants.Identity;

public static class RoleConstants
{
    public const int RoleNameMinimumLength = 3;
    
    public static class DefaultRoles
    {
        public const string AdminName = "Admin";
        public const string AdminDescription = "Global administrator role with all permissions";
    
        public const string ModeratorName = "Moderator";
        public const string ModeratorDescription = "Moderator role with most administration permissions";
    
        public const string ServiceAccountName = "ServiceAccount";
        public const string ServiceAccountDescription = "Service Account role with permissions for service accounts";
    
        public const string DefaultName = "Default";
        public const string DefaultDescription = "Default role with base permissions, granted to every account by default";
    }

    public static List<string> GetRequiredRoleNames()
    {
        return
        [
            DefaultRoles.AdminName,
            DefaultRoles.ModeratorName,
            DefaultRoles.ServiceAccountName,
            DefaultRoles.DefaultName
        ];
    }

    public static List<string> GetAdminRoleNames()
    {
        return [DefaultRoles.AdminName];
    }

    public static List<string> GetModeratorRoleNames()
    {
        return
        [
            DefaultRoles.ModeratorName,
            DefaultRoles.DefaultName
        ];
    }

    public static List<string> GetServiceAccountRoleNames()
    {
        return [DefaultRoles.ServiceAccountName];
    }

    public static List<string> GetDefaultRoleNames()
    {
        return [DefaultRoles.DefaultName];
    }
}