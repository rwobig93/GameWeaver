using System.Reflection;
using System.Security.Claims;
using Application.Constants.Identity;
using Domain.DatabaseEntities.Identity;
using Domain.Enums.Identity;

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

    public static string GetClaimValueFromServiceAccount(Guid accountId, DynamicPermissionGroup permissionGroup, DynamicPermissionLevel permissionLevel)
    {
        return $"Dynamic.{permissionGroup.ToString()}.{accountId.ToString()}.{permissionLevel.ToString()}";
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
        return (from prop in typeof(PermissionConstants).GetNestedTypes().SelectMany(
                c => c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)) 
            select prop.GetValue(null) into propertyValue where propertyValue is not null select propertyValue.ToString()!).ToList();
    }

    public static List<string> GetModeratorRolePermissions()
    {
        return new List<string>()
        {
            PermissionConstants.Preferences.ChangeTheme,
            PermissionConstants.Jobs.View,
            PermissionConstants.Permissions.View,
            PermissionConstants.Permissions.Add,
            PermissionConstants.Permissions.Remove,
            PermissionConstants.Roles.View,
            PermissionConstants.Roles.Edit,
            PermissionConstants.Roles.Create,
            PermissionConstants.Roles.Delete,
            PermissionConstants.Roles.Add,
            PermissionConstants.Roles.Remove,
            PermissionConstants.Users.View,
            PermissionConstants.Users.Edit,
            PermissionConstants.Users.Create,
            PermissionConstants.Users.Delete,
            PermissionConstants.Users.Disable,
            PermissionConstants.Users.Enable,
            PermissionConstants.Users.ResetPassword,
            PermissionConstants.Users.ChangeEmail,
            PermissionConstants.Audit.View,
            PermissionConstants.Audit.Search,
            PermissionConstants.Audit.Export,
            PermissionConstants.ServiceAccounts.View
        };
    }

    public static List<string> GetServiceAccountRolePermissions()
    {
        return new List<string>()
        {
            PermissionConstants.Api.View
        };
    }

    public static List<string> GetDefaultRolePermissions()
    {
        return new List<string>()
        {
            PermissionConstants.Preferences.ChangeTheme,
            PermissionConstants.Users.ChangeEmail
        };
    }
}