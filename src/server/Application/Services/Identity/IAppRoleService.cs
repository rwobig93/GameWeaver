using Application.Models.Identity.Role;
using Application.Models.Identity.User;
using Domain.Contracts;

namespace Application.Services.Identity;

public interface IAppRoleService
{
    Task<IResult<IEnumerable<AppRoleSlim>>> GetAllAsync();
    Task<IResult<IEnumerable<AppRoleSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetCountAsync();
    Task<IResult<AppRoleSlim?>> GetByIdAsync(Guid roleId);
    Task<IResult<AppRoleFull?>> GetByIdFullAsync(Guid roleId);
    Task<IResult<AppRoleSlim?>> GetByNameAsync(string roleName);
    Task<IResult<AppRoleFull?>> GetByNameFullAsync(string roleName);
    Task<IResult<IEnumerable<AppRoleSlim>>> SearchAsync(string searchText);
    Task<IResult<IEnumerable<AppRoleSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<IResult<Guid>> CreateAsync(AppRoleCreate createObject, Guid modifyingUserId);
    Task<IResult> UpdateAsync(AppRoleUpdate updateObject, Guid modifyingUserId);
    Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId);
    Task<IResult<bool>> IsUserInRoleAsync(Guid userId, Guid roleId);
    Task<IResult<bool>> IsUserInRoleAsync(Guid userId, string roleName);
    Task<IResult<bool>> IsUserAdminAsync(Guid userId);
    Task<IResult<bool>> IsUserModeratorAsync(Guid userId);
    Task<IResult<bool>> IsUserAdminOrModeratorAsync(Guid userId);
    Task<IResult> AddUserToRoleAsync(Guid userId, Guid roleId, Guid modifyingUserId);
    Task<IResult> RemoveUserFromRoleAsync(Guid userId, Guid roleId, Guid modifyingUserId);
    Task<IResult<IEnumerable<AppRoleSlim>>> GetRolesForUser(Guid userId);
    Task<IResult<IEnumerable<AppUserSlim>>> GetUsersForRole(Guid roleId);
}