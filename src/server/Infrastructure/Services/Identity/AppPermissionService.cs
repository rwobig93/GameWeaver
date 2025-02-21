using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Identity;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.Permission;
using Application.Models.Identity.Role;
using Application.Models.Identity.User;
using Application.Repositories.Identity;
using Application.Repositories.Lifecycle;
using Application.Services.Identity;
using Application.Services.Lifecycle;
using Application.Services.System;
using Domain.Contracts;
using Domain.Enums.Identity;
using Domain.Enums.Lifecycle;
using Hangfire;

namespace Infrastructure.Services.Identity;

public class AppPermissionService : IAppPermissionService
{
    private readonly IAppPermissionRepository _permissionRepository;
    private readonly IAppUserRepository _userRepository;
    private readonly IAppRoleRepository _roleRepository;
    private readonly IRunningServerState _serverState;
    private readonly IDateTimeService _dateTime;
    private readonly ILogger _logger;
    private readonly ITroubleshootingRecordsRepository _tshootRepository;

    public AppPermissionService(IAppPermissionRepository permissionRepository, IAppUserRepository userRepository, IAppRoleRepository roleRepository,
        IRunningServerState serverState, IDateTimeService dateTime, ILogger logger, ITroubleshootingRecordsRepository tshootRepository)
    {
        _permissionRepository = permissionRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _serverState = serverState;
        _dateTime = dateTime;
        _logger = logger;
        _tshootRepository = tshootRepository;
    }

    private async Task<bool> IsUserAdmin(Guid userId)
    {
        var roleCheck = await _roleRepository.IsUserInRoleAsync(userId, RoleConstants.DefaultRoles.AdminName);
        return roleCheck.Result;
    }

    private async Task<bool> IsUserModerator(Guid userId)
    {
        var roleCheck = await _roleRepository.IsUserInRoleAsync(userId, RoleConstants.DefaultRoles.ModeratorName);
        return roleCheck.Result;
    }

    private async Task<bool> CanUserDoThisAction(Guid modifyingUserId, string claimValue)
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

        // If the permission is a dynamic permission and the user is a moderator they can administrate the permission
        if (claimValue.StartsWith("Dynamic."))
        {
            var modifyingUserIsModerator = await IsUserModerator(modifyingUserId);
            if (modifyingUserIsModerator)
            {
                return true;
            }
        }
        
        // If a user has the permission, and they've been given access to add/remove permissions then they are good to go,
        //    otherwise they can't add/remove a permission they themselves don't have
        var invokingUserHasRequestingPermission = await UserIncludingRolesHasPermission(modifyingUserId, claimValue);
        return invokingUserHasRequestingPermission.Data;
    }

    public async Task<IResult<IEnumerable<AppPermissionCreate>>> GetAllAvailablePermissionsAsync()
    {
        try
        {
            var allPermissions = new List<AppPermissionCreate>();
            
            // Get all native/built-in permissions
            var allBuiltInPermissions = PermissionHelpers.GetAllBuiltInPermissions();
            allPermissions.AddRange(allBuiltInPermissions.ToAppPermissionCreates());

            return await Result<IEnumerable<AppPermissionCreate>>.SuccessAsync(allPermissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionCreate>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionCreate>>> GetAllAvailableDynamicServiceAccountPermissionsAsync(Guid id)
    {
        try
        {
            var allPermissions = new List<AppPermissionCreate>();
            
            var allServiceAccountPermissions = PermissionHelpers.GetAllServiceAccountDynamicPermissions(id);
            allPermissions.AddRange(allServiceAccountPermissions.ToDynamicPermissionCreates());

            return await Result<IEnumerable<AppPermissionCreate>>.SuccessAsync(allPermissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionCreate>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionCreate>>> GetAllAvailableDynamicGameServerPermissionsAsync(Guid id)
    {
        try
        {
            var allPermissions = new List<AppPermissionCreate>();
            
            var allGameServerPermissions = PermissionHelpers.GetAllGameServerDynamicPermissions(id);
            allPermissions.AddRange(allGameServerPermissions.ToDynamicPermissionCreates());

            return await Result<IEnumerable<AppPermissionCreate>>.SuccessAsync(allPermissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionCreate>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllAssignedAsync()
    {
        try
        {
            var permissionsRequest = await _permissionRepository.GetAllAsync();
            if (!permissionsRequest.Succeeded)
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(permissionsRequest.ErrorMessage);

            var permissions = (permissionsRequest.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<PaginatedResult<IEnumerable<AppPermissionSlim>>> GetAllAssignedPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var response = await _permissionRepository.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!response.Succeeded)
            {
                return await PaginatedResult<IEnumerable<AppPermissionSlim>>.FailAsync(response.ErrorMessage);
            }
        
            if (response.Result?.Data is null)
            {
                return await PaginatedResult<IEnumerable<AppPermissionSlim>>.SuccessAsync([]);
            }

            var permissions = (response.Result?.Data.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await PaginatedResult<IEnumerable<AppPermissionSlim>>.SuccessAsync(
                permissions,
                response.Result!.StartPage,
                response.Result.CurrentPage,
                response.Result.EndPage,
                response.Result.TotalCount,
                response.Result.PageSize);
        }
        catch (Exception ex)
        {
            return await PaginatedResult<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> SearchAsync(string searchTerm)
    {
        try
        {
            var searchResult = await _permissionRepository.SearchAsync(searchTerm);
            if (!searchResult.Succeeded)
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(searchResult.ErrorMessage);

            var permissions = (searchResult.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<PaginatedResult<IEnumerable<AppPermissionSlim>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize)
    {
        try
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var response = await _permissionRepository.SearchPaginatedAsync(searchTerm, pageNumber, pageSize);
            if (!response.Succeeded)
            {
                return await PaginatedResult<IEnumerable<AppPermissionSlim>>.FailAsync(response.ErrorMessage);
            }
        
            if (response.Result?.Data is null)
            {
                return await PaginatedResult<IEnumerable<AppPermissionSlim>>.SuccessAsync([]);
            }

            var permissions = (response.Result?.Data.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await PaginatedResult<IEnumerable<AppPermissionSlim>>.SuccessAsync(
                permissions,
                response.Result!.StartPage,
                response.Result.CurrentPage,
                response.Result.EndPage,
                response.Result.TotalCount,
                response.Result.PageSize);
        }
        catch (Exception ex)
        {
            return await PaginatedResult<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        try
        {
            var countRequest = await _permissionRepository.GetCountAsync();
            if (!countRequest.Succeeded)
                return await Result<int>.FailAsync(countRequest.ErrorMessage);

            return await Result<int>.SuccessAsync(countRequest.Result);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppUserSlim>>> GetAllUsersByClaimValueAsync(string claimValue)
    {
        try
        {
            var foundUsers = await _permissionRepository.GetAllUsersByClaimValueAsync(claimValue);
            if (!foundUsers.Succeeded)
                return await Result<IEnumerable<AppUserSlim>>.FailAsync(foundUsers.ErrorMessage);

            return await Result<IEnumerable<AppUserSlim>>.SuccessAsync(foundUsers.Result?.ToSlims() ?? new List<AppUserSlim>());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppRoleSlim>>> GetAllRolesByClaimValueAsync(string claimValue)
    {
        try
        {
            var foundRoles = await _permissionRepository.GetAllRolesByClaimValueAsync(claimValue);
            if (!foundRoles.Succeeded)
                return await Result<IEnumerable<AppRoleSlim>>.FailAsync(foundRoles.ErrorMessage);

            return await Result<IEnumerable<AppRoleSlim>>.SuccessAsync(foundRoles.Result?.ToSlims() ?? new List<AppRoleSlim>());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppRoleSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppPermissionSlim?>> GetByIdAsync(Guid permissionId)
    {
        try
        {
            var foundPermission = await _permissionRepository.GetByIdAsync(permissionId);
            if (!foundPermission.Succeeded)
                return await Result<AppPermissionSlim?>.FailAsync(foundPermission.ErrorMessage);

            return await Result<AppPermissionSlim?>.SuccessAsync(foundPermission.Result?.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<AppPermissionSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppPermissionSlim?>> GetByUserIdAndValueAsync(Guid userId, string claimValue)
    {
        try
        {
            var foundPermission = await _permissionRepository.GetByUserIdAndValueAsync(userId, claimValue);
            if (!foundPermission.Succeeded)
                return await Result<AppPermissionSlim?>.FailAsync(foundPermission.ErrorMessage);

            return await Result<AppPermissionSlim?>.SuccessAsync(foundPermission.Result?.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<AppPermissionSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppPermissionSlim?>> GetByRoleIdAndValueAsync(Guid roleId, string claimValue)
    {
        try
        {
            var foundPermission = await _permissionRepository.GetByRoleIdAndValueAsync(roleId, claimValue);
            if (!foundPermission.Succeeded)
                return await Result<AppPermissionSlim?>.FailAsync(foundPermission.ErrorMessage);

            return await Result<AppPermissionSlim?>.SuccessAsync(foundPermission.Result?.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<AppPermissionSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllByRoleNameAsync(string roleName)
    {
        try
        {
            var foundPermissions = await _permissionRepository.GetAllByNameAsync(roleName);
            if (!foundPermissions.Succeeded)
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(foundPermissions.ErrorMessage);
            
            var permissions = (foundPermissions.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllByGroupAsync(string groupName)
    {
        try
        {
            var foundPermissions = await _permissionRepository.GetAllByGroupAsync(groupName);
            if (!foundPermissions.Succeeded)
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(foundPermissions.ErrorMessage);
            
            var permissions = (foundPermissions.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllByAccessAsync(string accessName)
    {
        try
        {
            var foundPermissions = await _permissionRepository.GetAllByAccessAsync(accessName);
            if (!foundPermissions.Succeeded)
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(foundPermissions.ErrorMessage);
            
            var permissions = (foundPermissions.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllByClaimValueAsync(string claimValue)
    {
        try
        {
            var foundPermissions = await _permissionRepository.GetAllByClaimValueAsync(claimValue);
            if (!foundPermissions.Succeeded)
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(foundPermissions.ErrorMessage);
            
            var permissions = (foundPermissions.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllForRoleAsync(Guid roleId)
    {
        try
        {
            var foundPermissions = await _permissionRepository.GetAllForRoleAsync(roleId);
            if (!foundPermissions.Succeeded)
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(foundPermissions.ErrorMessage);
            
            var permissions = (foundPermissions.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllDirectForUserAsync(Guid userId)
    {
        try
        {
            var foundPermissions = await _permissionRepository.GetAllDirectForUserAsync(userId);
            if (!foundPermissions.Succeeded)
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(foundPermissions.ErrorMessage);
            
            var permissions = (foundPermissions.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> GetAllIncludingRolesForUserAsync(Guid userId)
    {
        try
        {
            var foundPermissions = await _permissionRepository.GetAllIncludingRolesForUserAsync(userId);
            if (!foundPermissions.Succeeded)
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(foundPermissions.ErrorMessage);
            
            var permissions = (foundPermissions.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }

    private async Task UpdateUserForPermissionChange(Guid userId)
    {
        _logger.Debug("Updating clientId's for {UserId} to re-auth for updated permissions", userId);

        // Update user userClientId's with a re-auth description which will be checked to re-auth w/ a refresh token for updated permissions
        var userClientIds = await _userRepository.GetUserExtendedAttributesByTypeAsync(userId, ExtendedAttributeType.UserClientId);
        if (!userClientIds.Succeeded || userClientIds.Result is null) return;

        foreach (var clientId in userClientIds.Result)
            await _userRepository.UpdateExtendedAttributeAsync(clientId.Id, clientId.Value, UserClientIdState.ReAuthNeeded.ToString());
            
        _logger.Debug("Updated clientId's for User to force refresh token re-auth: {UserId}", userId);
    }

    private async Task UpdateRoleUsersForPermissionChange(Guid roleId)
    {
        var roleUsers = await _roleRepository.GetUsersForRole(roleId);
        if (!roleUsers.Succeeded)
        {
            _logger.Error("Failure occurred attempting to get users for role for permission update: {Error}", roleUsers.ErrorMessage);
            return;
        }

        var usersToUpdate = roleUsers.Result?.ToSlims().ToList() ?? [];
        
        _logger.Debug("Found {UserCount} users for role {RoleId} that need to re-auth for updated permissions", usersToUpdate.Count, roleId);

        // Update each user's userClientId with a re-auth description which will be checked to re-auth w/ a refresh token for updated permissions
        foreach (var user in usersToUpdate)
        {
            var userClientIds = await _userRepository.GetUserExtendedAttributesByTypeAsync(user.Id, ExtendedAttributeType.UserClientId);
            if (!userClientIds.Succeeded || userClientIds.Result is null) continue;

            foreach (var clientId in userClientIds.Result)
                await _userRepository.UpdateExtendedAttributeAsync(clientId.Id, clientId.Value, UserClientIdState.ReAuthNeeded.ToString());
            
            _logger.Debug("Updated clientId's for User to force refresh token re-auth: [{UserId}]{Username}", user.Id, user.Username);
        }
    }

    public async Task<IResult<Guid>> CreateAsync(AppPermissionCreate createObject, Guid modifyingUserId)
    {
        try
        {
            if (createObject.UserId == Guid.Empty && createObject.RoleId == Guid.Empty)
                return await Result<Guid>.FailAsync("UserId & RoleId cannot be empty, please provide a valid Id");
            if (createObject.UserId == GuidHelpers.GetMax() && createObject.RoleId == GuidHelpers.GetMax())
                return await Result<Guid>.FailAsync("UserId & RoleId cannot be empty, please provide a valid Id");
            if (createObject.UserId != GuidHelpers.GetMax() && createObject.RoleId != GuidHelpers.GetMax())
                return await Result<Guid>.FailAsync("Each permission assignment request can only be made for a User or Role, not both at the same time");

            if (createObject.UserId != GuidHelpers.GetMax())
            {
                var foundUser = await _userRepository.GetByIdAsync(createObject.UserId);
                if (foundUser.Result is null)
                    return await Result<Guid>.FailAsync("UserId provided is invalid");
            }
            
            if (createObject.RoleId != GuidHelpers.GetMax())
            {
                var foundRole = await _roleRepository.GetByIdAsync(createObject.RoleId);
                if (foundRole.Result is null)
                    return await Result<Guid>.FailAsync("RoleId provided is invalid");
            }

            if (!await CanUserDoThisAction(modifyingUserId, createObject.ClaimValue))
                return await Result<Guid>.FailAsync(ErrorMessageConstants.Permissions.CannotAdministrateMissingPermission);

            createObject.CreatedBy = modifyingUserId;
            createObject.CreatedOn = _dateTime.NowDatabaseTime;

            var createRequest = await _permissionRepository.CreateAsync(createObject);
            if (!createRequest.Succeeded)
                return await Result<Guid>.FailAsync(createRequest.ErrorMessage);
            
            // Queue background job to update all users w/ permission or role permission to re-auth and have the latest permissions
            _logger.Debug("Kicking off job to update {PermissionClaimValue} claim userClientId's with re-auth string to force token refresh for updated permissions",
                createObject.ClaimValue);

            var response = createObject.UserId == GuidHelpers.GetMax() ?
                BackgroundJob.Enqueue(() => UpdateRoleUsersForPermissionChange(createObject.RoleId)) :
                BackgroundJob.Enqueue(() => UpdateUserForPermissionChange(createObject.UserId));
            
            if (response is null)
                await _tshootRepository.CreateTroubleshootRecord(_serverState, _dateTime, TroubleshootEntityType.Permissions, createRequest.Result,
                    "Failed to queue permission job to update users w/ new permissions to validate against clientId", new Dictionary<string, string>()
                {
                    {"Action", "Permission Change - Update Users - Create Permission"},
                    {"PermissionId", createRequest.Result.ToString()}
                });
            
            return await Result<Guid>.SuccessAsync(createRequest.Result);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> UpdateAsync(AppPermissionUpdate updateObject, Guid modifyingUserId)
    {
        try
        {
            var foundPermission = await _permissionRepository.GetByIdAsync(updateObject.Id);
            if (!foundPermission.Succeeded || foundPermission.Result?.ClaimValue is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);
            }

            var userCanDoAction = await CanUserDoThisAction(modifyingUserId, foundPermission.Result.ClaimValue);
            if (!userCanDoAction)
            {
                return await Result<Guid>.FailAsync(ErrorMessageConstants.Permissions.CannotAdministrateMissingPermission);
            }

            updateObject.LastModifiedBy = modifyingUserId;
            updateObject.LastModifiedOn = _dateTime.NowDatabaseTime;

            var updateRequest = await _permissionRepository.UpdateAsync(updateObject);
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

    public async Task<IResult> DeleteAsync(Guid permissionId, Guid modifyingUserId)
    {
        try
        {
            var foundPermission = await _permissionRepository.GetByIdAsync(permissionId);
            if (!foundPermission.Succeeded || foundPermission.Result?.ClaimValue is null)
                return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

            var userCanDoAction = await CanUserDoThisAction(modifyingUserId, foundPermission.Result.ClaimValue);
            if (!userCanDoAction)
                return await Result<Guid>.FailAsync(ErrorMessageConstants.Permissions.CannotAdministrateMissingPermission);
            
            var deleteRequest = await _permissionRepository.DeleteAsync(foundPermission.Result.Id, modifyingUserId);
            if (!deleteRequest.Succeeded)
                return await Result.FailAsync(deleteRequest.ErrorMessage);
            
            // Queue background job to update all users w/ permission or role permission to re-auth and have the latest permissions
            _logger.Debug("Kicking off job to update {PermissionClaimValue} claim userClientId's with re-auth string to force token refresh for updated permissions",
                foundPermission.Result.ClaimValue);

            var response = foundPermission.Result.UserId == GuidHelpers.GetMax() ?
                BackgroundJob.Enqueue(() => UpdateRoleUsersForPermissionChange(foundPermission.Result.RoleId)) :
                BackgroundJob.Enqueue(() => UpdateUserForPermissionChange(foundPermission.Result.UserId));
            
            if (response is null)
                await _tshootRepository.CreateTroubleshootRecord(_serverState, _dateTime, TroubleshootEntityType.Permissions, permissionId,
                    "Failed to queue permission job to update users w/ new permissions to validate against clientId", new Dictionary<string, string>()
                    {
                        {"Action", "Permission Change - Update Users - Delete Permission"},
                        {"PermissionId", permissionId.ToString()}
                    });

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> UserHasDirectPermission(Guid userId, string permissionValue)
    {
        try
        {
            var checkRequest = await _permissionRepository.UserHasDirectPermission(userId, permissionValue);
            if (!checkRequest.Succeeded)
                return await Result<bool>.FailAsync(checkRequest.ErrorMessage);

            return await Result<bool>.SuccessAsync(checkRequest.Result);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> UserIncludingRolesHasPermission(Guid userId, string permissionValue)
    {
        try
        {
            var checkRequest = await _permissionRepository.UserIncludingRolesHasPermission(userId, permissionValue);
            if (!checkRequest.Succeeded)
                return await Result<bool>.FailAsync(checkRequest.ErrorMessage);

            return await Result<bool>.SuccessAsync(checkRequest.Result);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<bool>> RoleHasPermission(Guid roleId, string permissionValue)
    {
        try
        {
            var checkRequest = await _permissionRepository.RoleHasPermission(roleId, permissionValue);
            if (!checkRequest.Succeeded)
                return await Result<bool>.FailAsync(checkRequest.ErrorMessage);

            return await Result<bool>.SuccessAsync(checkRequest.Result);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppPermissionSlim>>> GetDynamicByTypeAndNameAsync(DynamicPermissionGroup type, Guid name)
    {
        try
        {
            var foundPermissions = await _permissionRepository.GetDynamicByTypeAndNameAsync(type, name);
            if (!foundPermissions.Succeeded)
            {
                return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(foundPermissions.ErrorMessage);
            }
            
            var permissions = (foundPermissions.Result?.ToSlims() ?? new List<AppPermissionSlim>())
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Access);

            return await Result<IEnumerable<AppPermissionSlim>>.SuccessAsync(permissions);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppPermissionSlim>>.FailAsync(ex.Message);
        }
    }
}