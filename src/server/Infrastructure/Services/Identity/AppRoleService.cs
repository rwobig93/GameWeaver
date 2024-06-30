using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Mappers.Identity;
using Application.Models.Identity.Permission;
using Application.Models.Identity.Role;
using Application.Models.Identity.User;
using Application.Repositories.Identity;
using Application.Services.Identity;
using Application.Services.Lifecycle;
using Application.Services.System;
using Domain.Contracts;
using Domain.DatabaseEntities.Identity;

namespace Infrastructure.Services.Identity;

public class AppRoleService : IAppRoleService
{
    private readonly IAppRoleRepository _roleRepository;
    private readonly IAppPermissionRepository _permissionRepository;
    private readonly IRunningServerState _serverState;
    private readonly IDateTimeService _dateTimeService;

    public AppRoleService(IAppRoleRepository roleRepository, IAppPermissionRepository permissionRepository, IRunningServerState serverState,
        IDateTimeService dateTimeService)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _serverState = serverState;
        _dateTimeService = dateTimeService;
    }

    private async Task<IResult<AppRoleFull?>> ConvertToFullAsync(AppRoleDb roleDb)
    {
        var fullRole = roleDb.ToFull();

        var foundUsers = await _roleRepository.GetUsersForRole(roleDb.Id);
        if (!foundUsers.Succeeded)
            return await Result<AppRoleFull?>.FailAsync(foundUsers.ErrorMessage);
            
        fullRole.Users = (foundUsers.Result?.ToSlims() ?? new List<AppUserSlim>())
            .OrderBy(x => x.Username)
            .ToList();

        var foundPermissions = await _permissionRepository.GetAllForRoleAsync(roleDb.Id);
        if (!foundPermissions.Succeeded)
            return await Result<AppRoleFull?>.FailAsync(foundPermissions.ErrorMessage);
            
        fullRole.Permissions = (foundPermissions.Result?.ToSlims() ?? new List<AppPermissionSlim>())
            .OrderBy(x => x.Group)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Access)
            .ToList();

        return await Result<AppRoleFull?>.SuccessAsync(fullRole);
    }

    private async Task<bool> IsUserAdmin(Guid userId)
    {
        var roleCheck = await _roleRepository.IsUserInRoleAsync(userId, RoleConstants.DefaultRoles.AdminName);
        return roleCheck.Result;
    }

    private async Task<bool> CanUserDoThisAction(Guid modifyingUserId, Guid roleId)
    {
        // If the user is the system user they have full reign, so we let them past the permission validation
        if (modifyingUserId == _serverState.SystemUserId)
        {
            return true;
        }
        
        // If the user is an admin we let them do whatever they want
        var modifyingUserIsAdmin = await IsUserAdmin(modifyingUserId);
        if (modifyingUserIsAdmin)
        {
            return true;
        }

        var modifyingRole = await GetByIdAsync(roleId);
        if (!modifyingRole.Succeeded || modifyingRole.Data is null)
        {
            return false;
        }

        // If user isn't admin then they can't modify administrative roles
        if (modifyingRole.Data.Name is RoleConstants.DefaultRoles.AdminName or RoleConstants.DefaultRoles.ModeratorName)
        {
            return false;
        }

        // We are assuming permission to edit roles is already verified, so we let all other actions be allowed
        return true;
    }

    public async Task<IResult<IEnumerable<AppRoleSlim>>> GetAllAsync()
    {
        try
        {
            var roles = await _roleRepository.GetAllAsync();
            if (!roles.Succeeded)
                return await Result<IEnumerable<AppRoleSlim>>.FailAsync(roles.ErrorMessage);

            return await Result<IEnumerable<AppRoleSlim>>.SuccessAsync(roles.Result?.ToSlims() ?? new List<AppRoleSlim>());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppRoleSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppRoleSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            var roles = await _roleRepository.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!roles.Succeeded)
                return await Result<IEnumerable<AppRoleSlim>>.FailAsync(roles.ErrorMessage);

            return await Result<IEnumerable<AppRoleSlim>>.SuccessAsync(roles.Result?.ToSlims() ?? new List<AppRoleSlim>());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppRoleSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        try
        {
            var roleCount = await _roleRepository.GetCountAsync();
            if (!roleCount.Succeeded)
                return await Result<int>.FailAsync(roleCount.ErrorMessage);

            return await Result<int>.SuccessAsync(roleCount.Result);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppRoleSlim?>> GetByIdAsync(Guid roleId)
    {
        try
        {
            var foundRole = await _roleRepository.GetByIdAsync(roleId);
            if (!foundRole.Succeeded)
                return await Result<AppRoleSlim?>.FailAsync(foundRole.ErrorMessage);

            return await Result<AppRoleSlim?>.SuccessAsync(foundRole.Result?.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<AppRoleSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppRoleFull?>> GetByIdFullAsync(Guid roleId)
    {
        try
        {
            var foundRole = await _roleRepository.GetByIdAsync(roleId);
            if (!foundRole.Succeeded)
                return await Result<AppRoleFull?>.FailAsync(foundRole.ErrorMessage);

            if (foundRole.Result is null)
                return await Result<AppRoleFull?>.FailAsync(foundRole.Result?.ToFull());

            return await ConvertToFullAsync(foundRole.Result);
        }
        catch (Exception ex)
        {
            return await Result<AppRoleFull?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppRoleSlim?>> GetByNameAsync(string roleName)
    {
        try
        {
            var foundRole = await _roleRepository.GetByNameAsync(roleName);
            if (!foundRole.Succeeded)
                return await Result<AppRoleSlim?>.FailAsync(foundRole.ErrorMessage);

            if (foundRole.Result is null)
                return await Result<AppRoleSlim?>.FailAsync(foundRole.Result?.ToSlim());

            return await Result<AppRoleSlim?>.SuccessAsync(foundRole.Result.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<AppRoleSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppRoleFull?>> GetByNameFullAsync(string roleName)
    {
        try
        {
            var foundRole = await _roleRepository.GetByNameAsync(roleName);
            if (!foundRole.Succeeded)
                return await Result<AppRoleFull?>.FailAsync(foundRole.ErrorMessage);
            
            if (foundRole.Result is null)
                return await Result<AppRoleFull?>.FailAsync(foundRole.Result?.ToFull());

            return await ConvertToFullAsync(foundRole.Result);
        }
        catch (Exception ex)
        {
            return await Result<AppRoleFull?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppRoleSlim>>> SearchAsync(string searchText)
    {
        try
        {
            var searchResult = await _roleRepository.SearchAsync(searchText);
            if (!searchResult.Succeeded)
                return await Result<IEnumerable<AppRoleSlim>>.FailAsync(searchResult.ErrorMessage);

            var results = (searchResult.Result?.ToSlims() ?? new List<AppRoleSlim>())
                .OrderBy(x => x.Name);

            return await Result<IEnumerable<AppRoleSlim>>.SuccessAsync(results);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppRoleSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppRoleSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        try
        {
            var searchResult = await _roleRepository.SearchPaginatedAsync(searchText, pageNumber, pageSize);
            if (!searchResult.Succeeded)
                return await Result<IEnumerable<AppRoleSlim>>.FailAsync(searchResult.ErrorMessage);

            var results = (searchResult.Result?.ToSlims() ?? new List<AppRoleSlim>())
                .OrderBy(x => x.Name);

            return await Result<IEnumerable<AppRoleSlim>>.SuccessAsync(results);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppRoleSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<Guid>> CreateAsync(AppRoleCreate createObject, Guid modifyingUserId)
    {
        try
        {
            if (createObject.Name.Length < RoleConstants.RoleNameMinimumLength)
                return await Result<Guid>.FailAsync($"Role name must be longer than {RoleConstants.RoleNameMinimumLength} characters");
            
            if (string.IsNullOrWhiteSpace(createObject.Description))
                return await Result<Guid>.FailAsync("Role description must have something in it - please use a descriptive description");

            var existingRoleWithName = await _roleRepository.GetByNameAsync(createObject.Name);
            if (!existingRoleWithName.Succeeded)
                return await Result<Guid>.FailAsync(existingRoleWithName.ErrorMessage);
            
            if (existingRoleWithName.Result is not null)
                return await Result<Guid>.FailAsync("A role with that name already exists, please use a different name");

            createObject.CreatedBy = modifyingUserId;
            createObject.CreatedOn = _dateTimeService.NowDatabaseTime;
            
            var createRequest = await _roleRepository.CreateAsync(createObject);
            if (!createRequest.Succeeded)
                return await Result<Guid>.FailAsync(createRequest.ErrorMessage);

            return await Result<Guid>.SuccessAsync(createRequest.Result);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> UpdateAsync(AppRoleUpdate updateObject, Guid modifyingUserId)
    {
        try
        {
            var roleToChange = await GetByIdAsync(updateObject.Id);
            if (roleToChange.Data is null)
            {
                return await Result<Guid>.FailAsync(ErrorMessageConstants.Roles.NotFound);
            }

            var canUserDoThisAction = await CanUserDoThisAction(modifyingUserId, updateObject.Id);
            if (!canUserDoThisAction)
            {
                return await Result<Guid>.FailAsync(ErrorMessageConstants.Roles.CannotAdministrateAdminRole);
            }
            
            if (updateObject.Name is not null && updateObject.Name.Length < RoleConstants.RoleNameMinimumLength)
            {
                return await Result<Guid>.FailAsync($"Role name must be longer than {RoleConstants.RoleNameMinimumLength} characters");
            }
            
            if (string.IsNullOrWhiteSpace(updateObject.Description))
            {
                return await Result<Guid>.FailAsync("Role description must have something in it - please use a descriptive description");
            }

            // If we are changing the name we need to verify there isn't already a role with the same name as the update
            if (roleToChange.Data!.Name != updateObject.Name)
            {
                // We don't allow default role names to change to keep our sanity and enforce Admin, Moderator & Default role intent
                if (RoleConstants.GetRequiredRoleNames().Contains(roleToChange.Data.Name))
                {
                    return await Result.FailAsync("The role you are attempting to modify cannot have it's name changed");
                }
                
                var existingRoleWithName = await _roleRepository.GetByNameAsync(updateObject.Name!);
                if (!existingRoleWithName.Succeeded)
                {
                    return await Result<Guid>.FailAsync(existingRoleWithName.ErrorMessage);
                }
                
                if (existingRoleWithName.Result is not null)
                {
                    return await Result<Guid>.FailAsync("A role with that name already exists, please use a different name");
                }
            }

            updateObject.LastModifiedBy = modifyingUserId;
            updateObject.LastModifiedOn = _dateTimeService.NowDatabaseTime;

            var updateRequest = await _roleRepository.UpdateAsync(updateObject);
            if (!updateRequest.Succeeded)
            {
                return await Result.FailAsync(updateRequest.ErrorMessage);
            }

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        try
        {
            var foundRole = await GetByIdAsync(id);
            if (!foundRole.Succeeded || foundRole.Data is null)
                return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

            var roleUserCount = await GetUsersForRole(id);
            if (!roleUserCount.Succeeded)
                return await Result.FailAsync(roleUserCount.Messages);

            if (roleUserCount.Data.Any())
                return await Result.FailAsync("Roles that contain users cannot be deleted, please remove all users first");

            if (RoleConstants.GetRequiredRoleNames().Contains(foundRole.Data.Name))
                return await Result.FailAsync("The role you are attempting to delete is a built-in role and cannot be deleted");
            
            var deleteRequest = await _roleRepository.DeleteAsync(id, modifyingUserId);
            if (!deleteRequest.Succeeded)
                return await Result.FailAsync(deleteRequest.ErrorMessage);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> IsUserInRoleAsync(Guid userId, Guid roleId)
    {
        try
        {
            var roleCheck = await _roleRepository.IsUserInRoleAsync(userId, roleId);
            if (!roleCheck.Succeeded)
                return await Result<bool>.FailAsync(roleCheck.ErrorMessage);

            return await Result<bool>.SuccessAsync(roleCheck.Result);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> IsUserInRoleAsync(Guid userId, string roleName)
    {
        try
        {
            var roleCheck = await _roleRepository.IsUserInRoleAsync(userId, roleName);
            if (!roleCheck.Succeeded)
                return await Result<bool>.FailAsync(roleCheck.ErrorMessage);

            return await Result<bool>.SuccessAsync(roleCheck.Result);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> IsUserAdminAsync(Guid userId)
    {
        try
        {
            var roleCheck = await _roleRepository.IsUserInRoleAsync(userId, RoleConstants.DefaultRoles.AdminName);
            if (!roleCheck.Succeeded)
                return await Result<bool>.FailAsync(roleCheck.ErrorMessage);

            return await Result<bool>.SuccessAsync(roleCheck.Result);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> IsUserModeratorAsync(Guid userId)
    {
        try
        {
            var roleCheck = await _roleRepository.IsUserInRoleAsync(userId, RoleConstants.DefaultRoles.ModeratorName);
            if (!roleCheck.Succeeded)
                return await Result<bool>.FailAsync(roleCheck.ErrorMessage);

            return await Result<bool>.SuccessAsync(roleCheck.Result);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> IsUserAdminOrModeratorAsync(Guid userId)
    {
        try
        {
            var adminRoleCheck = await _roleRepository.IsUserInRoleAsync(userId, RoleConstants.DefaultRoles.AdminName);
            if (!adminRoleCheck.Succeeded)
                return await Result<bool>.FailAsync(adminRoleCheck.ErrorMessage);

            if (adminRoleCheck.Result)
                return await Result<bool>.SuccessAsync(adminRoleCheck.Result);
            
            var modRoleCheck = await _roleRepository.IsUserInRoleAsync(userId, RoleConstants.DefaultRoles.ModeratorName);
            if (!modRoleCheck.Succeeded)
                return await Result<bool>.FailAsync(modRoleCheck.ErrorMessage);
            
            return await Result<bool>.SuccessAsync(modRoleCheck.Result);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> AddUserToRoleAsync(Guid userId, Guid roleId, Guid modifyingUserId)
    {
        try
        {
            var userAdd = await _roleRepository.AddUserToRoleAsync(userId, roleId, modifyingUserId);
            if (!userAdd.Succeeded)
                return await Result.FailAsync(userAdd.ErrorMessage);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> RemoveUserFromRoleAsync(Guid userId, Guid roleId, Guid modifyingUserId)
    {
        try
        {
            var userRemove = await _roleRepository.RemoveUserFromRoleAsync(userId, roleId, modifyingUserId);
            if (!userRemove.Succeeded)
                return await Result.FailAsync(userRemove.ErrorMessage);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppRoleSlim>>> GetRolesForUser(Guid userId)
    {
        try
        {
            var rolesRequest = await _roleRepository.GetRolesForUser(userId);
            if (!rolesRequest.Succeeded)
                return await Result<IEnumerable<AppRoleSlim>>.FailAsync(rolesRequest.ErrorMessage);

            var roles = (rolesRequest.Result?.ToSlims() ?? new List<AppRoleSlim>())
                .OrderBy(x => x.Name);

            return await Result<IEnumerable<AppRoleSlim>>.SuccessAsync(roles);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppRoleSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppUserSlim>>> GetUsersForRole(Guid roleId)
    {
        try
        {
            var rolesRequest = await _roleRepository.GetUsersForRole(roleId);
            if (!rolesRequest.Succeeded)
                return await Result<IEnumerable<AppUserSlim>>.FailAsync(rolesRequest.ErrorMessage);

            var users = (rolesRequest.Result?.ToSlims() ?? new List<AppUserSlim>())
                .OrderBy(x => x.Username);

            return await Result<IEnumerable<AppUserSlim>>.SuccessAsync(users);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserSlim>>.FailAsync(ex.Message);
        }
    }
}