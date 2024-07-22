using Application.Models.Identity.Permission;
using Domain.Contracts;
using Domain.DatabaseEntities.Identity;
using Domain.Models.Database;
using Domain.Models.Identity;

namespace Application.Repositories.Identity;

public interface IAppPermissionRepository
{
    Task<DatabaseActionResult<IEnumerable<AppPermissionDb>>> GetAllAsync();
    Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppPermissionDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<AppPermissionDb>>> SearchAsync(string searchTerm);
    Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppPermissionDb>>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<IEnumerable<AppUserSecurityDb>>> GetAllUsersByClaimValueAsync(string claimValue);
    Task<DatabaseActionResult<IEnumerable<AppRoleDb>>> GetAllRolesByClaimValueAsync(string claimValue);
    Task<DatabaseActionResult<AppPermissionDb>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<AppPermissionDb>> GetByUserIdAndValueAsync(Guid userId, string claimValue);
    Task<DatabaseActionResult<AppPermissionDb>> GetByRoleIdAndValueAsync(Guid roleId, string claimValue);
    Task<DatabaseActionResult<IEnumerable<AppPermissionDb>>> GetAllByNameAsync(string roleName);
    Task<DatabaseActionResult<IEnumerable<AppPermissionDb>>> GetAllByGroupAsync(string groupName);
    Task<DatabaseActionResult<IEnumerable<AppPermissionDb>>> GetAllByAccessAsync(string accessName);
    Task<DatabaseActionResult<IEnumerable<AppPermissionDb>>> GetAllByClaimValueAsync(string claimValue);
    Task<DatabaseActionResult<IEnumerable<AppPermissionDb>>> GetAllForRoleAsync(Guid roleId);
    Task<DatabaseActionResult<IEnumerable<AppPermissionDb>>> GetAllDirectForUserAsync(Guid userId);
    Task<DatabaseActionResult<IEnumerable<AppPermissionDb>>> GetAllIncludingRolesForUserAsync(Guid userId);
    Task<DatabaseActionResult<Guid>> CreateAsync(AppPermissionCreate createObject);
    Task<DatabaseActionResult> UpdateAsync(AppPermissionUpdate updateObject);
    Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId);
    Task<DatabaseActionResult<bool>> UserHasDirectPermission(Guid userId, string permissionValue);
    Task<DatabaseActionResult<bool>> UserIncludingRolesHasPermission(Guid userId, string permissionValue);
    Task<DatabaseActionResult<bool>> RoleHasPermission(Guid roleId, string permissionValue);
}