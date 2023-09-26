using Application.Models.Identity.Permission;
using Application.Models.Identity.Role;
using Application.Models.Identity.User;
using Application.Requests.Identity.Role;
using Application.Responses.Identity;
using Domain.DatabaseEntities.Identity;

namespace Application.Mappers.Identity;

public static class RoleMappers
{
    public static AppRoleDb ToDb(this RoleResponse roleResponse)
    {
        return new AppRoleDb
        {
            Name = roleResponse.Name,
            Id = roleResponse.Id,
            Description = roleResponse.Description
        };
    }
    
    public static AppRoleCreate ToCreateObject(this AppRoleDb appRole)
    {
        return new AppRoleCreate
        {
            Name = appRole.Name,
            Description = appRole.Description,
            CreatedBy = appRole.CreatedBy,
            CreatedOn = appRole.CreatedOn
        };
    }

    public static AppRoleCreate ToCreateObject(this CreateRoleRequest createRole)
    {
        return new AppRoleCreate
        {
            Name = createRole.Name,
            Description = createRole.Description,
            CreatedBy = default,
            CreatedOn = default
        };
    }
    
    public static AppRoleFull ToFull(this AppRoleDb roleDb)
    {
        return new AppRoleFull
        {
            Id = roleDb.Id,
            Name = roleDb.Name,
            Description = roleDb.Description,
            CreatedBy = roleDb.CreatedBy,
            CreatedOn = roleDb.CreatedOn,
            LastModifiedBy = roleDb.LastModifiedBy,
            LastModifiedOn = roleDb.LastModifiedOn
        };
    }

    public static AppRoleFull ToFull(this AppRoleSlim appRoleSlim)
    {
        return new AppRoleFull
        {
            Id = appRoleSlim.Id,
            Name = appRoleSlim.Name,
            Description = appRoleSlim.Description,
            CreatedBy = appRoleSlim.CreatedBy,
            CreatedOn = appRoleSlim.CreatedOn,
            LastModifiedBy = appRoleSlim.LastModifiedBy,
            LastModifiedOn = appRoleSlim.LastModifiedOn,
            Users = new List<AppUserSlim>(),
            Permissions = new List<AppPermissionSlim>()
        };
    }
    
    public static AppRoleSlim ToSlim(this AppRoleDb appRoleDb)
    {
        return new AppRoleSlim
        {
            Id = appRoleDb.Id,
            Name = appRoleDb.Name,
            Description = appRoleDb.Description,
            CreatedBy = appRoleDb.CreatedBy,
            CreatedOn = appRoleDb.CreatedOn,
            LastModifiedBy = appRoleDb.LastModifiedBy,
            LastModifiedOn = appRoleDb.LastModifiedOn
        };
    }

    public static IEnumerable<AppRoleSlim> ToSlims(this IEnumerable<AppRoleDb> appRoleDbs)
    {
        return appRoleDbs.Select(x => x.ToSlim());
    }
    
    public static RoleResponse ToResponse(this AppRoleSlim appRole)
    {
        return new RoleResponse
        {
            Id = appRole.Id,
            Name = appRole.Name,
            Description = appRole.Description
        };
    }
    
    public static List<RoleResponse> ToResponses(this IEnumerable<AppRoleSlim> appRoles)
    {
        return appRoles.Select(x => x.ToResponse()).ToList();
    }
    
    public static RoleFullResponse ToFullResponse(this AppRoleSlim appRole)
    {
        return new RoleFullResponse
        {
            Id = appRole.Id,
            Name = appRole.Name,
            Description = appRole.Description,
            Permissions = new List<PermissionResponse>()
        };
    }
    
    public static List<RoleFullResponse> ToFullResponses(this IEnumerable<AppRoleSlim> appRoles)
    {
        return appRoles.Select(x => x.ToFullResponse()).ToList();
    }
    
    public static AppRoleUpdate ToObject(this AppRoleDb appRole)
    {
        return new AppRoleUpdate
        {
            Id = appRole.Id,
            Name = appRole.Name,
            Description = appRole.Description,
            LastModifiedBy = appRole.LastModifiedBy,
            LastModifiedOn = appRole.LastModifiedOn
        };
    }

    public static AppRoleUpdate ToUpdate(this UpdateRoleRequest roleRequest)
    {
        return new AppRoleUpdate
        {
            Id = roleRequest.Id,
            Name = roleRequest.Name,
            Description = roleRequest.Description,
            LastModifiedBy = null,
            LastModifiedOn = null
        };
    }

    public static AppRoleUpdate ToUpdate(this AppRoleFull roleFull)
    {
        return new AppRoleUpdate
        {
            Id = roleFull.Id,
            Name = roleFull.Name,
            Description = roleFull.Description,
            LastModifiedBy = roleFull.LastModifiedBy,
            LastModifiedOn = roleFull.LastModifiedOn
        };
    }

    public static AppRoleUpdate ToUpdate(this AppRoleDb roleDb)
    {
        return new AppRoleUpdate
        {
            Id = roleDb.Id,
            Name = roleDb.Name,
            Description = roleDb.Description,
            LastModifiedBy = roleDb.LastModifiedBy,
            LastModifiedOn = roleDb.LastModifiedOn
        };
    }
}