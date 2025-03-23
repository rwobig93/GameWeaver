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
    
        public const string DefaultName = "Everyone";
        public const string DefaultDescription = "Role with permissions everyone has";
    }
    
    public static readonly string[] NoTouchRoles = [DefaultRoles.AdminName, DefaultRoles.ModeratorName, DefaultRoles.ServiceAccountName, DefaultRoles.DefaultName];
    public static readonly string[] StaticUserRoleNames = [DefaultRoles.ServiceAccountName, DefaultRoles.DefaultName];
    public static readonly string[] AdminRoleNames = [DefaultRoles.AdminName, DefaultRoles.DefaultName];
    public static readonly string[] ModeratorRoleNames = [DefaultRoles.ModeratorName, DefaultRoles.DefaultName];
    public static readonly string[] ServiceAccountRoleNames = [DefaultRoles.ServiceAccountName, DefaultRoles.DefaultName];
    public static readonly string[] DefaultRoleNames = [DefaultRoles.DefaultName];
}