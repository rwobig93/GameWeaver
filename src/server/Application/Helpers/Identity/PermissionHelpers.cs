using System.Security.Claims;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Domain.DatabaseEntities.Identity;

namespace Application.Helpers.Identity;

public static class PermissionHelpers
{
    // Built-in Permissions are the following format: Permissions.Group.Name.Access => Permissions.Identity.Users.View
    // Dynamic Permissions are the following format: Dynamic.Group.Id.AccessLevel => Dynamic.ServiceAccounts.<Guid>.Admin
    public static string? GetClaimValueFromPermission(string? permissionGroup, string? permissionName, string? permissionAccess)
    {
        if (permissionGroup is null || permissionName is null || permissionAccess is null)
            return null;
        
        return $"Permissions.{permissionGroup}.{permissionName}.{permissionAccess}";
    }

    public static string GetGroupFromValue(string permissionValue) => permissionValue.Split('.')[1];
    public static string GetNameFromValue(string permissionValue) => permissionValue.Split('.')[2];
    public static string GetAccessFromValue(string permissionValue) => permissionValue.Split('.')[3];

    public static Claim ToClaim(this AppPermissionDb appPermission)
    {
        return new Claim(appPermission.ClaimType!, appPermission.ClaimValue!);
    }

    public static IEnumerable<Claim> ToClaims(this IEnumerable<AppPermissionDb> appPermissions)
    {
        return appPermissions.Select(x => new Claim(x.ClaimType!, x.ClaimValue!));
    }

    public static IEnumerable<Claim> ToClaims(this IEnumerable<AppRoleDb> appRoles)
    {
        return appRoles.Select(x => new Claim(ClaimTypes.Role, x.Name));
    }
    
    /// <summary>
    /// Returns a list of all native permissions values
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllBuiltInPermissions()
    {
        return ReflectionHelpers.GetConstantsRecursively(typeof(PermissionConstants));
    }

    public static List<string> GetModeratorRolePermissions()
    {
        return
        [
            // Preferences
            PermissionConstants.Identity.Preferences.ChangeTheme,
            
            // System
            PermissionConstants.System.Jobs.View,
            PermissionConstants.System.Audit.View,
            PermissionConstants.System.Audit.Search,
            PermissionConstants.System.Audit.Export,
            
            // Permissions & Roles
            PermissionConstants.Identity.Permissions.View,
            PermissionConstants.Identity.Permissions.Add,
            PermissionConstants.Identity.Permissions.Remove,
            PermissionConstants.Identity.Roles.View,
            PermissionConstants.Identity.Roles.Edit,
            PermissionConstants.Identity.Roles.Create,
            PermissionConstants.Identity.Roles.Delete,
            PermissionConstants.Identity.Roles.Add,
            PermissionConstants.Identity.Roles.Remove,
            
            // Users & Accounts
            PermissionConstants.Identity.Users.View,
            PermissionConstants.Identity.Users.Edit,
            PermissionConstants.Identity.Users.Create,
            PermissionConstants.Identity.Users.Delete,
            PermissionConstants.Identity.Users.Disable,
            PermissionConstants.Identity.Users.Enable,
            PermissionConstants.Identity.Users.ResetPassword,
            PermissionConstants.Identity.Users.ChangeEmail,
            PermissionConstants.Identity.ServiceAccounts.View
        ];
    }

    public static List<string> GetServiceAccountRolePermissions()
    {
        return [PermissionConstants.System.Api.View];
    }

    public static List<string> GetDefaultRolePermissions()
    {
        return
        [
            PermissionConstants.Identity.Preferences.ChangeTheme,
            PermissionConstants.Identity.Users.ChangeEmail
        ];
    }
}