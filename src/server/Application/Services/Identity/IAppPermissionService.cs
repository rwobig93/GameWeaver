using Application.Models.Identity.Permission;
using Application.Models.Identity.Role;
using Application.Models.Identity.User;
using Domain.Contracts;
using Domain.Enums.Identity;

namespace Application.Services.Identity;

public interface IAppPermissionService
{
    Task<IResult<IEnumerable<AppPermissionCreate>>> GetAllAvailablePermissionsAsync();
    Task<IResult<IEnumerable<AppPermissionCreate>>> GetAllAvailableDynamicServiceAccountPermissionsAsync(Guid id);
    Task<IResult<IEnumerable<AppPermissionCreate>>> GetAllAvailableDynamicGameServerPermissionsAsync(Guid id);
    Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllAssignedAsync();
    Task<PaginatedResult<IEnumerable<AppPermissionSlim>>> GetAllAssignedPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<IEnumerable<AppPermissionSlim>>> SearchAsync(string searchTerm);
    Task<PaginatedResult<IEnumerable<AppPermissionSlim>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize);
    Task<IResult<int>> GetCountAsync();
    Task<IResult<IEnumerable<AppUserSlim>>> GetAllUsersByClaimValueAsync(string claimValue);
    Task<IResult<IEnumerable<AppRoleSlim>>> GetAllRolesByClaimValueAsync(string claimValue);
    Task<IResult<AppPermissionSlim?>> GetByIdAsync(Guid permissionId);
    Task<IResult<AppPermissionSlim?>> GetByUserIdAndValueAsync(Guid userId, string claimValue);
    Task<IResult<AppPermissionSlim?>> GetByRoleIdAndValueAsync(Guid roleId, string claimValue);
    Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllByRoleNameAsync(string roleName);
    Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllByGroupAsync(string groupName);
    Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllByAccessAsync(string accessName);
    Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllByClaimValueAsync(string claimValue);
    Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllForRoleAsync(Guid roleId);
    Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllDirectForUserAsync(Guid userId);
    Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllIncludingRolesForUserAsync(Guid userId);
    Task<IResult<Guid>> CreateAsync(AppPermissionCreate createObject, Guid modifyingUserId);
    Task<IResult> UpdateAsync(AppPermissionUpdate updateObject, Guid modifyingUserId);
    Task<IResult> DeleteAsync(Guid permissionId, Guid modifyingUserId);
    Task<IResult<bool>> UserHasDirectPermission(Guid userId, string permissionValue);
    Task<IResult<bool>> UserIncludingRolesHasPermission(Guid userId, string permissionValue);
    Task<IResult<bool>> RoleHasPermission(Guid roleId, string permissionValue);
    Task<IResult<IEnumerable<AppPermissionSlim>>> GetDynamicByTypeAndNameAsync(DynamicPermissionGroup type, Guid name);
}