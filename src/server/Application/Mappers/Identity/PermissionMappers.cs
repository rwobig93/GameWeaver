using Application.Helpers.Identity;
using Application.Helpers.Runtime;
using Application.Models.Identity.Permission;
using Application.Requests.Identity.Permission;
using Application.Responses.Identity;
using Domain.DatabaseEntities.Identity;
using Domain.Enums.Identity;
using Domain.Models.Identity;

namespace Application.Mappers.Identity;

public static class PermissionMappers
{
    public static PermissionResponse ToResponse(this AppPermissionDb permission)
    {
        return new PermissionResponse
        {
            Id = permission.Id,
            UserId = permission.UserId,
            RoleId = permission.RoleId,
            Name = permission.Name,
            Description = permission.Description,
            Group = permission.Group
        };
    }

    public static List<PermissionResponse> ToResponses(this IEnumerable<AppPermissionDb> permissions)
    {
        return permissions.Select(x => x.ToResponse()).ToList();
    }
    
    public static AppPermissionCreate ToCreate(this AppPermissionDb permissionDb)
    {
        return new AppPermissionCreate
        {
            RoleId = permissionDb.RoleId,
            UserId = permissionDb.UserId,
            ClaimType = permissionDb.ClaimType ?? "",
            ClaimValue = permissionDb.ClaimValue ?? "",
            Name = permissionDb.Name,
            Group = permissionDb.Group,
            Access = permissionDb.Access,
            Description = permissionDb.Description,
            CreatedBy = permissionDb.CreatedBy,
            CreatedOn = permissionDb.CreatedOn,
            LastModifiedBy = permissionDb.LastModifiedBy,
            LastModifiedOn = permissionDb.LastModifiedOn
        };
    }

    public static IEnumerable<AppPermissionCreate> ToCreates(this IEnumerable<AppPermissionDb> permissionDbs)
    {
        return permissionDbs.Select(x => x.ToCreate()).ToList();
    }
    
    public static AppPermissionCreate ToCreate(this AppPermissionSlim permissionSlim)
    {
        return new AppPermissionCreate
        {
            RoleId = permissionSlim.RoleId,
            UserId = permissionSlim.UserId,
            ClaimType = permissionSlim.ClaimType ?? "",
            ClaimValue = permissionSlim.ClaimValue,
            Name = permissionSlim.Name,
            Group = permissionSlim.Group,
            Access = permissionSlim.Access,
            Description = permissionSlim.Description,
            CreatedBy = permissionSlim.CreatedBy,
            CreatedOn = permissionSlim.CreatedOn,
            LastModifiedBy = permissionSlim.LastModifiedBy,
            LastModifiedOn = permissionSlim.LastModifiedOn
        };
    }

    public static IEnumerable<AppPermissionCreate> ToCreates(this IEnumerable<AppPermissionSlim> permissionSlims)
    {
        return permissionSlims.Select(x => x.ToCreate()).ToList();
    }

    public static AppPermissionDb ToDb(this AppPermissionCreate permissionCreate)
    {
        return new AppPermissionDb
        {
            RoleId = permissionCreate.RoleId,
            ClaimType = permissionCreate.ClaimType,
            ClaimValue = permissionCreate.ClaimValue,
            Id = Guid.Empty,
            UserId = permissionCreate.UserId,
            Name = permissionCreate.Name,
            Group = permissionCreate.Group,
            Access = permissionCreate.Access,
            Description = permissionCreate.Description,
            CreatedBy = permissionCreate.CreatedBy,
            CreatedOn = permissionCreate.CreatedOn,
            LastModifiedBy = permissionCreate.LastModifiedBy,
            LastModifiedOn = permissionCreate.LastModifiedOn
        };
    }
    
    public static AppPermissionCreate ToCreate(this PermissionCreateForRoleRequest permissionCreate)
    {
        return new AppPermissionCreate
        {
            RoleId = permissionCreate.RoleId,
            ClaimType = ApplicationClaimTypes.Permission,
            ClaimValue = PermissionHelpers.GetClaimValueFromPermission(permissionCreate.Group, permissionCreate.Name, permissionCreate.Access) ?? "",
            Name = permissionCreate.Name,
            Description = permissionCreate.Description,
            Group = permissionCreate.Group,
            Access = permissionCreate.Access
        };
    }
    
    public static AppPermissionCreate ToCreate(this PermissionCreateForUserRequest permissionCreate)
    {
        return new AppPermissionCreate
        {
            UserId = permissionCreate.UserId,
            ClaimType = ApplicationClaimTypes.Permission,
            ClaimValue = PermissionHelpers.GetClaimValueFromPermission(permissionCreate.Group, permissionCreate.Name, permissionCreate.Access) ?? "",
            Name = permissionCreate.Name,
            Description = permissionCreate.Description,
            Group = permissionCreate.Group,
            Access = permissionCreate.Access
        };
    }

    public static AppPermissionCreate ToAppPermissionCreate(this string permissionValue)
    {
        if (permissionValue.Split('.').Length != 4)
            throw new Exception("Permission value provided doesn't match the correct syntax of Permissions.Group.Name.Access");

        var permissionName = PermissionHelpers.GetNameFromValue(permissionValue);
        var permissionGroup = PermissionHelpers.GetGroupFromValue(permissionValue);
        var permissionAccess = PermissionHelpers.GetAccessFromValue(permissionValue);
        
        return new AppPermissionCreate
        {
            RoleId = GuidHelpers.GetMax(),
            UserId = GuidHelpers.GetMax(),
            ClaimType = ApplicationClaimTypes.Permission,
            ClaimValue = permissionValue,
            Name = permissionName,
            Group = permissionGroup,
            Access = permissionAccess,
            Description = $"{permissionAccess} access to {permissionName}",
            CreatedBy = Guid.Empty,
            CreatedOn = DateTime.Now,
            LastModifiedBy = null,
            LastModifiedOn = null
        };
    }

    public static AppPermissionCreate ToDynamicPermissionCreate(this AppUserServicePermissionDb serviceAccount, DynamicPermissionLevel permissionLevel)
    {
        return new AppPermissionCreate
        {
            RoleId = GuidHelpers.GetMax(),
            UserId = GuidHelpers.GetMax(),
            ClaimType = ApplicationClaimTypes.Permission,
            ClaimValue = PermissionHelpers.GetClaimValueFromServiceAccount(serviceAccount.Id, DynamicPermissionGroup.ServiceAccounts, permissionLevel),
            Group = DynamicPermissionGroup.ServiceAccounts.ToString(),
            Name = serviceAccount.Username,
            Access = permissionLevel.ToString(),
            Description = $"{permissionLevel.ToString()} access to {DynamicPermissionGroup.ServiceAccounts.ToString()} {serviceAccount.Id}",
            CreatedBy = Guid.Empty,
            CreatedOn = DateTime.Now,
            LastModifiedBy = null,
            LastModifiedOn = null
        };
    }

    public static IEnumerable<AppPermissionCreate> ToAppPermissionCreates(this IEnumerable<AppUserServicePermissionDb> serviceAccounts,
        DynamicPermissionLevel permissionLevel)
    {
        return serviceAccounts.Select(x => x.ToDynamicPermissionCreate(permissionLevel)).ToList();
    }

    public static IEnumerable<AppPermissionCreate> ToAppPermissionCreates(this IEnumerable<string> permissionValues)
    {
        return permissionValues.Select(x => x.ToAppPermissionCreate()).ToList();
    }
    
    public static AppPermissionSlim ToSlim(this AppPermissionDb appPermissionDb)
    {
        return new AppPermissionSlim
        {
            Id = appPermissionDb.Id,
            UserId = appPermissionDb.UserId,
            RoleId = appPermissionDb.RoleId,
            ClaimType = appPermissionDb.ClaimType,
            ClaimValue = appPermissionDb.ClaimValue ?? "",
            Name = appPermissionDb.Name,
            Group = appPermissionDb.Group,
            Access = appPermissionDb.Access,
            Description = appPermissionDb.Description,
            CreatedBy = appPermissionDb.CreatedBy,
            CreatedOn = appPermissionDb.CreatedOn,
            LastModifiedBy = appPermissionDb.LastModifiedBy,
            LastModifiedOn = appPermissionDb.LastModifiedOn
        };
    }

    public static IEnumerable<AppPermissionSlim> ToSlims(this IEnumerable<AppPermissionDb> appPermissionDbs)
    {
        return appPermissionDbs.Select(x => x.ToSlim());
    }

    public static AppPermissionSlim ToSlim(this AppPermissionCreate permissionCreate)
    {
        return new AppPermissionSlim
        {
            Id = Guid.Empty,
            UserId = permissionCreate.UserId,
            RoleId = permissionCreate.RoleId,
            ClaimType = permissionCreate.ClaimType,
            ClaimValue = permissionCreate.ClaimValue,
            Name = permissionCreate.Name,
            Group = permissionCreate.Group,
            Access = permissionCreate.Access,
            Description = permissionCreate.Description,
            CreatedBy = permissionCreate.CreatedBy,
            CreatedOn = permissionCreate.CreatedOn,
            LastModifiedBy = permissionCreate.LastModifiedBy,
            LastModifiedOn = permissionCreate.LastModifiedOn
        };
    }

    public static IEnumerable<AppPermissionSlim> ToSlims(this IEnumerable<AppPermissionCreate> appPermissionCreates)
    {
        return appPermissionCreates.Select(x => x.ToSlim());
    }
    
    public static PermissionResponse ToResponse(this AppPermissionSlim permission)
    {
        return new PermissionResponse
        {
            Id = permission.Id,
            UserId = permission.UserId,
            RoleId = permission.RoleId,
            Name = permission.Name,
            Description = permission.Description,
            Group = permission.Group
        };
    }

    public static List<PermissionResponse> ToResponses(this IEnumerable<AppPermissionSlim> permissions)
    {
        return permissions.Select(x => x.ToResponse()).ToList();
    }
    
    public static AppPermissionUpdate ToUpdate(this PermissionUpdateRequest permissionUpdate)
    {
        return new AppPermissionUpdate
        {
            Id = permissionUpdate.Id,
            ClaimType = ApplicationClaimTypes.Permission,
            ClaimValue = PermissionHelpers.GetClaimValueFromPermission(permissionUpdate.Group, permissionUpdate.Name, permissionUpdate.Access),
            Name = permissionUpdate.Name,
            Description = permissionUpdate.Description,
            Group = permissionUpdate.Group,
            Access = permissionUpdate.Access
        };
    }

    public static AppPermissionUpdate ToUpdate(this AppPermissionDb permissionDb)
    {
        return new AppPermissionUpdate
        {
            Id = permissionDb.Id,
            RoleId = permissionDb.RoleId,
            UserId = permissionDb.UserId,
            ClaimType = permissionDb.ClaimType,
            ClaimValue = permissionDb.ClaimValue,
            Name = permissionDb.Name,
            Group = permissionDb.Group,
            Access = permissionDb.Access,
            Description = permissionDb.Description,
            LastModifiedBy = permissionDb.LastModifiedBy,
            LastModifiedOn = permissionDb.LastModifiedOn
        };
    }
}