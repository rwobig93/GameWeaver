using Application.Constants.Communication;
using Application.Helpers.Identity;
using Application.Helpers.Lifecycle;
using Application.Mappers.Identity;
using Application.Models.Identity.User;
using Application.Models.Identity.UserExtensions;
using Application.Repositories.Identity;
using Application.Repositories.Lifecycle;
using Application.Services.Identity;
using Application.Services.Lifecycle;
using Application.Services.System;
using Domain.Contracts;
using Domain.Enums.Identity;
using Domain.Enums.Lifecycle;
using Domain.Models.Identity;
using Newtonsoft.Json;

namespace Infrastructure.Services.Identity;

public class AppUserService : IAppUserService
{
    private readonly IAppUserRepository _userRepository;
    private readonly IAppPermissionRepository _permissionRepository;
    private readonly IRunningServerState _serverState;
    private readonly IDateTimeService _dateTime;
    private readonly ITroubleshootingRecordsRepository _tshootRepository;

    public AppUserService(IAppUserRepository userRepository, IAppPermissionRepository permissionRepository, IRunningServerState serverState, IDateTimeService dateTime,
        ITroubleshootingRecordsRepository tshootRepository)
    {
        _userRepository = userRepository;
        _permissionRepository = permissionRepository;
        _serverState = serverState;
        _dateTime = dateTime;
        _tshootRepository = tshootRepository;
    }

    private static async Task<Result<AppUserFull?>> ConvertToFullAsync(AppUserFullDb? userFullDb)
    {
        if (userFullDb is null)
            return await Result<AppUserFull?>.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);
        
        var fullUser = userFullDb.ToFull();
        
        fullUser.Roles = userFullDb.Roles.ToSlims()
            .OrderBy(x => x.Name)
            .ToList();

        fullUser.ExtendedAttributes = userFullDb.ExtendedAttributes.ToSlims()
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Value)
            .ToList();
        
        fullUser.Permissions = userFullDb.Permissions.ToSlims()
            .OrderBy(x => x.Group)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Access)
            .ToList();

        return await Result<AppUserFull?>.SuccessAsync(fullUser);
    }

    public async Task<IResult<IEnumerable<AppUserSlim>>> GetAllAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            if (!users.Succeeded)
                return await Result<IEnumerable<AppUserSlim>>.FailAsync(users.ErrorMessage);

            return await Result<IEnumerable<AppUserSlim>>.SuccessAsync(users.Result!.ToSlims());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<PaginatedResult<IEnumerable<AppUserSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var response = await _userRepository.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!response.Succeeded)
            {
                return await PaginatedResult<IEnumerable<AppUserSlim>>.FailAsync(response.ErrorMessage);
            }
        
            if (response.Result?.Data is null)
            {
                return await PaginatedResult<IEnumerable<AppUserSlim>>.SuccessAsync([]);
            }

            return await PaginatedResult<IEnumerable<AppUserSlim>>.SuccessAsync(
                response.Result.Data.ToSlims(),
                response.Result.StartPage,
                response.Result.CurrentPage,
                response.Result.EndPage,
                response.Result.TotalCount,
                response.Result.PageSize);
        }
        catch (Exception ex)
        {
            return await PaginatedResult<IEnumerable<AppUserSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        try
        {
            var userCount = await _userRepository.GetCountAsync();
            if (!userCount.Succeeded)
                return await Result<int>.FailAsync(userCount.ErrorMessage);

            return await Result<int>.SuccessAsync(userCount.Result);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserSlim?>> GetByIdAsync(Guid userId)
    {
        try
        {
            var foundUser = await _userRepository.GetByIdAsync(userId);
            if (!foundUser.Succeeded)
                return await Result<AppUserSlim?>.FailAsync(foundUser.ErrorMessage);

            return await Result<AppUserSlim?>.SuccessAsync(foundUser.Result?.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<AppUserSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserFull?>> GetByIdFullAsync(Guid userId)
    {
        try
        {
            var foundUser = await _userRepository.GetByIdFullAsync(userId);
            if (!foundUser.Succeeded)
                return await Result<AppUserFull?>.FailAsync(foundUser.ErrorMessage);

            return await ConvertToFullAsync(foundUser.Result);
        }
        catch (Exception ex)
        {
            return await Result<AppUserFull?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserSecurityFull?>> GetByIdSecurityFullAsync(Guid userId)
    {
        try
        {
            var foundUser = await _userRepository.GetByIdSecurityAsync(userId);
            if (!foundUser.Succeeded || foundUser.Result is null)
                return await Result<AppUserSecurityFull?>.FailAsync(foundUser.ErrorMessage);

            return await Result<AppUserSecurityFull?>.SuccessAsync(foundUser.Result.ToUserSecurityFull());
        }
        catch (Exception ex)
        {
            return await Result<AppUserSecurityFull?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserSlim?>> GetByUsernameAsync(string username)
    {
        try
        {
            var foundUser = await _userRepository.GetByUsernameAsync(username);
            if (!foundUser.Succeeded)
                return await Result<AppUserSlim?>.FailAsync(foundUser.ErrorMessage);

            return await Result<AppUserSlim?>.SuccessAsync(foundUser.Result?.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<AppUserSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserFull?>> GetByUsernameFullAsync(string username)
    {
        try
        {
            var foundUser = await _userRepository.GetByUsernameFullAsync(username);
            if (!foundUser.Succeeded)
                return await Result<AppUserFull?>.FailAsync(foundUser.ErrorMessage);

            if (foundUser.Result is null)
                return await Result<AppUserFull?>.FailAsync(foundUser.Result?.ToFull());

            return await ConvertToFullAsync(foundUser.Result);
        }
        catch (Exception ex)
        {
            return await Result<AppUserFull?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserSecurityFull?>> GetByUsernameSecurityFullAsync(string username)
    {
        try
        {
            var foundUser = await _userRepository.GetByUsernameSecurityAsync(username);
            if (!foundUser.Succeeded || foundUser.Result is null)
                return await Result<AppUserSecurityFull?>.FailAsync(foundUser.ErrorMessage);

            return await Result<AppUserSecurityFull?>.SuccessAsync(foundUser.Result.ToUserSecurityFull());
        }
        catch (Exception ex)
        {
            return await Result<AppUserSecurityFull?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserSlim?>> GetByEmailAsync(string email)
    {
        try
        {
            var foundUser = await _userRepository.GetByEmailAsync(email);
            if (!foundUser.Succeeded)
                return await Result<AppUserSlim?>.FailAsync(foundUser.ErrorMessage);

            return await Result<AppUserSlim?>.SuccessAsync(foundUser.Result?.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<AppUserSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserFull?>> GetByEmailFullAsync(string email)
    {
        try
        {
            var foundUser = await _userRepository.GetByEmailFullAsync(email);
            if (!foundUser.Succeeded)
                return await Result<AppUserFull?>.FailAsync(foundUser.ErrorMessage);

            return await ConvertToFullAsync(foundUser.Result);
        }
        catch (Exception ex)
        {
            return await Result<AppUserFull?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> UpdateAsync(AppUserUpdate updateObject, Guid modifyingUserId)
    {
        try
        {
            var foundUser = await GetByIdAsync(updateObject.Id);
            if (!foundUser.Succeeded || foundUser.Data is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);
            }

            updateObject.LastModifiedBy = modifyingUserId;
            updateObject.LastModifiedOn = _dateTime.NowDatabaseTime;

            var userUpdate = await _userRepository.UpdateAsync(updateObject);
            if (!userUpdate.Succeeded)
            {
                return await Result.FailAsync(userUpdate.ErrorMessage);
            }

            if (foundUser.Data.AccountType != AccountType.Service) return await Result.SuccessAsync();
            if (updateObject.Username is not null && foundUser.Data.Username == updateObject.Username) return await Result.SuccessAsync();
            
            // Service Accounts have dynamic permissions, so we need to update assigned permissions if the account name changed
            var claimValue = PermissionHelpers.GetClaimValueFromServiceAccount(
                foundUser.Data.Id, DynamicPermissionGroup.ServiceAccounts, DynamicPermissionLevel.Admin);
            var serviceAccountPermissions = await _permissionRepository.GetAllByClaimValueAsync(claimValue);
            if (!serviceAccountPermissions.Succeeded || serviceAccountPermissions.Result is null)
            {
                await _tshootRepository.CreateTroubleshootRecord(_serverState, _dateTime, TroubleshootEntityType.Users,
                    foundUser.Data.Id, "Successfully updated service account but failed to update all dynamic permissions with the new name", new Dictionary<string, string>
                    {
                        {"Username Before", foundUser.Data.Username},
                        {"Username After", updateObject.Username ?? ""},
                        {"Error", serviceAccountPermissions.ErrorMessage}
                    });
                return await Result.FailAsync(ErrorMessageConstants.Generic.ContactAdmin);
            }

            List<string> errorMessages = [];
            
            foreach (var permission in serviceAccountPermissions.Result)
            {
                var permissionUpdate = permission.ToUpdate();
                permissionUpdate.Name = updateObject.Username;
                permissionUpdate.LastModifiedBy = _serverState.SystemUserId;
                permissionUpdate.LastModifiedOn = _dateTime.NowDatabaseTime;
                
                var updatePermissionRequest = await _permissionRepository.UpdateAsync(permissionUpdate);
                if (!updatePermissionRequest.Succeeded)
                    errorMessages.Add(updatePermissionRequest.ErrorMessage);
            }

            if (errorMessages.Count == 0)
                return await Result.SuccessAsync();

            // For any update requests that failed we'll create a troubleshooting audit trail to troubleshoot easier
            foreach (var message in errorMessages)
                await _tshootRepository.CreateTroubleshootRecord(_serverState, _dateTime, TroubleshootEntityType.Users,
                    foundUser.Data.Id, "Successfully updated service account but failed to update all dynamic permissions with the new name", new Dictionary<string, string>
                    {
                        {"Username Before", foundUser.Data.Username},
                        {"Username After", updateObject.Username ?? ""},
                        {"Error", message}
                    });
            
            return await Result.FailAsync(ErrorMessageConstants.Generic.ContactAdmin);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> DeleteAsync(Guid userId, Guid modifyingUserId)
    {
        try
        {
            var foundUser = await GetByIdAsync(userId);
            if (!foundUser.Succeeded || foundUser.Data is null)
                return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);
            
            var deleteUser = await _userRepository.DeleteAsync(userId, modifyingUserId);
            if (!deleteUser.Succeeded)
                return await Result.FailAsync(deleteUser.ErrorMessage);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppUserSlim>>> SearchAsync(string searchText)
    {
        try
        {
            var searchResult = await _userRepository.SearchAsync(searchText);
            if (!searchResult.Succeeded)
                return await Result<IEnumerable<AppUserSlim>>.FailAsync(searchResult.ErrorMessage);

            var results = (searchResult.Result?.ToSlims() ?? new List<AppUserSlim>())
                .OrderBy(x => x.Username);

            return await Result<IEnumerable<AppUserSlim>>.SuccessAsync(results);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<PaginatedResult<IEnumerable<AppUserSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        try
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var response = await _userRepository.SearchPaginatedAsync(searchText, pageNumber, pageSize);
            if (!response.Succeeded)
            {
                return await PaginatedResult<IEnumerable<AppUserSlim>>.FailAsync(response.ErrorMessage);
            }
        
            if (response.Result?.Data is null)
            {
                return await PaginatedResult<IEnumerable<AppUserSlim>>.SuccessAsync([]);
            }

            return await PaginatedResult<IEnumerable<AppUserSlim>>.SuccessAsync(
                response.Result.Data.ToSlims(),
                response.Result.StartPage,
                response.Result.CurrentPage,
                response.Result.EndPage,
                response.Result.TotalCount,
                response.Result.PageSize);
        }
        catch (Exception ex)
        {
            return await PaginatedResult<IEnumerable<AppUserSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<Guid>> CreateAsync(AppUserCreate createObject, Guid modifyingUserId)
    {
        try
        {
            var matchingEmail = (await _userRepository.GetByEmailAsync(createObject.Email)).Result;
            if (matchingEmail is not null)
                return await Result<Guid>.FailAsync(
                    $"The email address {createObject.Email} is already in use, are you sure you don't have an account already?");
        
            var matchingUserName = (await _userRepository.GetByUsernameAsync(createObject.Username)).Result;
            if (matchingUserName != null)
            {
                return await Result<Guid>.FailAsync(string.Format($"Username {createObject.Username} is already in use, please try again"));
            }

            createObject.CreatedBy = modifyingUserId;
            
            var createUser = await _userRepository.CreateAsync(createObject);
            if (!createUser.Succeeded)
                return await Result<Guid>.FailAsync(createUser.ErrorMessage);

            return await Result<Guid>.SuccessAsync(createUser.Result);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<Guid>> AddExtendedAttributeAsync(AppUserExtendedAttributeCreate addAttribute)
    {
        try
        {
            var addRequest = await _userRepository.AddExtendedAttributeAsync(addAttribute);
            if (!addRequest.Succeeded)
                return await Result<Guid>.FailAsync(addRequest.ErrorMessage);

            return await Result<Guid>.SuccessAsync(addRequest.Result);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> UpdateExtendedAttributeAsync(Guid attributeId, string? value, string? description)
    {
        try
        {
            var updateRequest = await _userRepository.UpdateExtendedAttributeAsync(attributeId, value, description);
            if (!updateRequest.Succeeded)
                return await Result.FailAsync(updateRequest.ErrorMessage);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> RemoveExtendedAttributeAsync(Guid attributeId)
    {
        try
        {
            var removeRequest = await _userRepository.RemoveExtendedAttributeAsync(attributeId);
            if (!removeRequest.Succeeded)
                return await Result.FailAsync(removeRequest.ErrorMessage);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> UpdatePreferences(Guid userId, AppUserPreferenceUpdate preferenceUpdate)
    {
        try
        {
            var updateRequest = await _userRepository.UpdatePreferences(userId, preferenceUpdate);
            if (!updateRequest.Succeeded)
                return await Result.FailAsync(updateRequest.ErrorMessage);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserPreferenceFull?>> GetPreferences(Guid userId)
    {
        try
        {
            var preferenceRequest = await _userRepository.GetPreferences(userId);
            if (!preferenceRequest.Succeeded)
                return await Result<AppUserPreferenceFull?>.FailAsync(preferenceRequest.ErrorMessage);

            var preferencesFull = preferenceRequest.Result?.ToFull();
            if (preferencesFull is null)
                return await Result<AppUserPreferenceFull?>.FailAsync(preferencesFull);
            
            preferencesFull.CustomThemeOne = JsonConvert.DeserializeObject<AppThemeCustom>(preferenceRequest.Result!.CustomThemeOne!)!;
            preferencesFull.CustomThemeTwo = JsonConvert.DeserializeObject<AppThemeCustom>(preferenceRequest.Result!.CustomThemeTwo!)!;
            preferencesFull.CustomThemeThree = JsonConvert.DeserializeObject<AppThemeCustom>(preferenceRequest.Result!.CustomThemeThree!)!;

            return await Result<AppUserPreferenceFull?>.SuccessAsync(preferencesFull);
        }
        catch (Exception ex)
        {
            return await Result<AppUserPreferenceFull?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserExtendedAttributeSlim?>> GetExtendedAttributeByIdAsync(Guid attributeId)
    {
        try
        {
            var getRequest = await _userRepository.GetExtendedAttributeByIdAsync(attributeId);
            if (!getRequest.Succeeded)
                return await Result<AppUserExtendedAttributeSlim?>.FailAsync(getRequest.ErrorMessage);

            return await Result<AppUserExtendedAttributeSlim?>.SuccessAsync(getRequest.Result?.ToSlim());
        }
        catch (Exception ex)
        {
            return await Result<AppUserExtendedAttributeSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetUserExtendedAttributesByTypeAsync(Guid userId, ExtendedAttributeType type)
    {
        try
        {
            var getRequest = await _userRepository.GetUserExtendedAttributesByTypeAsync(userId, type);
            if (!getRequest.Succeeded)
                return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(getRequest.ErrorMessage);

            var attributes = getRequest.Result?.ToSlims() ?? new List<AppUserExtendedAttributeSlim>();

            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.SuccessAsync(attributes);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetUserExtendedAttributesByNameAsync(Guid userId, string name)
    {
        try
        {
            var getRequest = await _userRepository.GetUserExtendedAttributesByNameAsync(userId, name);
            if (!getRequest.Succeeded)
                return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(getRequest.ErrorMessage);

            var attributes = getRequest.Result?.ToSlims() ?? new List<AppUserExtendedAttributeSlim>();

            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.SuccessAsync(attributes);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetAllUserExtendedAttributesAsync(Guid userId)
    {
        try
        {
            var getRequest = await _userRepository.GetAllUserExtendedAttributesAsync(userId);
            if (!getRequest.Succeeded)
                return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(getRequest.ErrorMessage);

            var attributes = getRequest.Result?.ToSlims() ?? new List<AppUserExtendedAttributeSlim>();

            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.SuccessAsync(attributes);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetAllExtendedAttributesByTypeAsync(ExtendedAttributeType type)
    {
        try
        {
            var getRequest = await _userRepository.GetAllExtendedAttributesByTypeAsync(type);
            if (!getRequest.Succeeded)
                return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(getRequest.ErrorMessage);

            var attributes = getRequest.Result?.ToSlims() ?? new List<AppUserExtendedAttributeSlim>();

            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.SuccessAsync(attributes);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetAllExtendedAttributesByNameAsync(string name)
    {
        try
        {
            var getRequest = await _userRepository.GetAllExtendedAttributesByNameAsync(name);
            if (!getRequest.Succeeded)
                return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(getRequest.ErrorMessage);

            var attributes = getRequest.Result?.ToSlims() ?? new List<AppUserExtendedAttributeSlim>();

            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.SuccessAsync(attributes);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetAllExtendedAttributesAsync()
    {
        try
        {
            var getRequest = await _userRepository.GetAllExtendedAttributesAsync();
            if (!getRequest.Succeeded)
                return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(getRequest.ErrorMessage);

            var attributes = getRequest.Result?.ToSlims() ?? new List<AppUserExtendedAttributeSlim>();

            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.SuccessAsync(attributes);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<AppUserExtendedAttributeSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<AppUserSecurityAttributeInfo?>> GetSecurityInfoAsync(Guid userId)
    {
        try
        {
            var foundSecurity = await _userRepository.GetSecurityAsync(userId);
            if (!foundSecurity.Succeeded)
                return await Result<AppUserSecurityAttributeInfo?>.FailAsync(foundSecurity.ErrorMessage);

            return await Result<AppUserSecurityAttributeInfo?>.SuccessAsync(foundSecurity.Result?.ToInfo());
        }
        catch (Exception ex)
        {
            return await Result<AppUserSecurityAttributeInfo?>.FailAsync(ex.Message);
        }
    }
}