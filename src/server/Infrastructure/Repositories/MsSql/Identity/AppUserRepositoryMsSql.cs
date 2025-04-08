using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.User;
using Application.Models.Identity.UserExtensions;
using Application.Repositories.Identity;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Domain.DatabaseEntities.Identity;
using Domain.Enums.Identity;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;
using Domain.Models.Identity;
using Infrastructure.Database.MsSql.Identity;
using Infrastructure.Database.MsSql.Shared;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories.MsSql.Identity;

public class AppUserRepositoryMsSql : IAppUserRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTime;
    private readonly IAuditTrailsRepository _auditRepository;
    private readonly IOptions<AppConfiguration> _generalConfig;

    public AppUserRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTime,
        IAuditTrailsRepository auditRepository, IOptions<AppConfiguration> generalConfig)
    {
        _database = database;
        _logger = logger;
        _dateTime = dateTime;
        _auditRepository = auditRepository;
        _generalConfig = generalConfig;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserSecurityDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<AppUserSecurityDb>> actionReturn = new();

        try
        {
            var allUsers = await _database.LoadData<AppUserSecurityDb, dynamic>(AppUsersTableMsSql.GetAll, new { });
            actionReturn.Succeed(allUsers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserServicePermissionDb>>> GetAllServiceAccountsForPermissionsAsync()
    {
        DatabaseActionResult<IEnumerable<AppUserServicePermissionDb>> actionReturn = new();

        try
        {
            var allUsers = await _database.LoadData<AppUserServicePermissionDb, dynamic>(
                AppUsersTableMsSql.GetAllServiceAccountsForPermissions, new { });
            actionReturn.Succeed(allUsers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetAllServiceAccountsForPermissions.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> GetAllServiceAccountsPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetAllServiceAccountsPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetAllServiceAccountsPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> GetAllDisabledPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetAllDisabledPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetAllDisabledPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> GetAllLockedOutPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetAllLockedOutPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetAllLockedOutPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {AppUsersTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserSecurityDb>> GetByIdAsync(Guid userId)
    {
        DatabaseActionResult<AppUserSecurityDb> actionReturn = new();

        try
        {
            var foundUser = (await _database.LoadData<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetById, new {Id = userId})).FirstOrDefault();
            actionReturn.Succeed(foundUser!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserFullDb?>> GetByIdFullAsync(Guid userId)
    {
        DatabaseActionResult<AppUserFullDb?> actionReturn = new();

        try
        {
            var foundUser = (await _database.LoadData<AppUserFullDb, dynamic>(
                AppUsersTableMsSql.GetById, new {Id = userId})).FirstOrDefault();

            if (foundUser is not null)
            {
                foundUser.Roles = (await _database.LoadData<AppRoleDb, dynamic>(
                    AppUserRoleJunctionsTableMsSql.GetRolesOfUser, new {UserId = foundUser.Id})).ToList();

                foundUser.Permissions = (await _database.LoadData<AppPermissionDb, dynamic>(
                    AppPermissionsTableMsSql.GetByUserId, new {UserId = foundUser.Id})).ToList();

                foundUser.ExtendedAttributes = (await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                    AppUserExtendedAttributesTableMsSql.GetByOwnerId, new {OwnerId = foundUser.Id})).ToList();
            }

            actionReturn.Succeed(foundUser);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetByIdFull.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserSecurityDb>> GetByIdSecurityAsync(Guid id)
    {
        DatabaseActionResult<AppUserSecurityDb> actionReturn = new();

        try
        {
            var foundUser = (await _database.LoadData<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetByIdSecurity, new {Id = id})).FirstOrDefault();

            actionReturn.Succeed(foundUser!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetByIdSecurity.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserSecurityDb>> GetByUsernameAsync(string username)
    {
        DatabaseActionResult<AppUserSecurityDb> actionReturn = new();

        try
        {
            var foundUser = (await _database.LoadData<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetByUsername, new {Username = username})).FirstOrDefault();
            actionReturn.Succeed(foundUser!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetByUsername.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserFullDb>> GetByUsernameFullAsync(string username)
    {
        DatabaseActionResult<AppUserFullDb> actionReturn = new();

        try
        {
            var foundUser = (await _database.LoadData<AppUserFullDb, dynamic>(
                AppUsersTableMsSql.GetByUsername, new {Username = username})).FirstOrDefault();

            if (foundUser is not null)
            {
                foundUser.Roles = (await _database.LoadData<AppRoleDb, dynamic>(
                    AppUserRoleJunctionsTableMsSql.GetRolesOfUser, new {UserId = foundUser.Id})).ToList();

                foundUser.Permissions = (await _database.LoadData<AppPermissionDb, dynamic>(
                    AppPermissionsTableMsSql.GetByUserId, new {UserId = foundUser.Id})).ToList();

                foundUser.ExtendedAttributes = (await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                    AppUserExtendedAttributesTableMsSql.GetByOwnerId, new {OwnerId = foundUser.Id})).ToList();
            }
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetByUsernameFull.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserSecurityDb>> GetByUsernameSecurityAsync(string username)
    {
        DatabaseActionResult<AppUserSecurityDb> actionReturn = new();

        try
        {
            var foundUser = (await _database.LoadData<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetByUsernameSecurity, new {Username = username})).FirstOrDefault();

            actionReturn.Succeed(foundUser!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetByUsernameSecurity.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserSecurityDb>> GetByEmailAsync(string email)
    {
        DatabaseActionResult<AppUserSecurityDb> actionReturn = new();

        try
        {
            var foundUser = (await _database.LoadData<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetByEmail, new {Email = email})).FirstOrDefault();
            actionReturn.Succeed(foundUser!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetByEmail.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserFullDb>> GetByEmailFullAsync(string email)
    {
        DatabaseActionResult<AppUserFullDb> actionReturn = new();

        try
        {
            var foundUser = (await _database.LoadData<AppUserFullDb, dynamic>(
                AppUsersTableMsSql.GetByEmail, new {Email = email})).FirstOrDefault();

            if (foundUser is not null)
            {
                foundUser.Roles = (await _database.LoadData<AppRoleDb, dynamic>(
                    AppUserRoleJunctionsTableMsSql.GetRolesOfUser, new {UserId = foundUser.Id})).ToList();

                foundUser.Permissions = (await _database.LoadData<AppPermissionDb, dynamic>(
                    AppPermissionsTableMsSql.GetByUserId, new {UserId = foundUser.Id})).ToList();

                foundUser.ExtendedAttributes = (await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                    AppUserExtendedAttributesTableMsSql.GetByOwnerId, new {OwnerId = foundUser.Id})).ToList();
            }
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetByEmailFull.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(AppUserCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createObject.CreatedOn = _dateTime.NowDatabaseTime;
            createObject.Currency = _generalConfig.Value.StartingCurrency;

            var createdId = await _database.SaveDataReturnId(AppUsersTableMsSql.Insert, createObject);

            // All user get database calls also pull from the security attribute AuthState so we at least need one to exist
            await CreateSecurityAsync(new AppUserSecurityAttributeCreate
            {
                OwnerId = createdId,
                PasswordHash = "null",
                PasswordSalt = "null",
                TwoFactorEnabled = false,
                TwoFactorKey = null,
                AuthState = AuthState.Disabled,
                AuthStateTimestamp = null,
                BadPasswordAttempts = 0,
                LastBadPassword = null
            });

            var foundUser = await GetByIdAsync(createdId);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Users, foundUser.Result!.Id,
                createObject.CreatedBy, AuditAction.Create, null, foundUser.Result!.ToSlim());

            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateAsync(AppUserUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppUsersTableMsSql.Update, updateObject);
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid userId, Guid modifyingUser)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var foundUser = await GetByIdAsync(userId);
            if (!foundUser.Succeeded || foundUser.Result is null)
                throw new Exception(foundUser.ErrorMessage);
            var userUpdate = foundUser.Result.ToUpdate();

            // Update user w/ a property that is modified so we get the last updated on/by for the deleting user
            userUpdate.LastModifiedBy = modifyingUser;
            await UpdateAsync(userUpdate);
            await _database.SaveData(AppUsersTableMsSql.Delete,
                new { userId, DeletedOn = _dateTime.NowDatabaseTime });

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Users, userId,
                userUpdate.LastModifiedBy.GetFromNullable(), AuditAction.Delete, userUpdate);

            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> SetUserId(Guid currentId, Guid newId)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var updatedId = await _database.SaveDataReturnId(
                AppUsersTableMsSql.SetUserId, new { CurrentId = currentId, NewId = newId });
            var ownerId = await _database.SaveDataReturnId(
                AppUserSecurityAttributesTableMsSql.SetOwnerId, new { CurrentId = currentId, NewId = newId });
            if (updatedId != ownerId)
                throw new Exception("SetUserID failed, updated User ID doesn't equal security owner ID");

            actionReturn.Succeed(updatedId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.SetUserId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> SetCreatedById(Guid userId, Guid createdById)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppUsersTableMsSql.SetCreatedById, new { Id = userId, CreatedBy = createdById });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.SetCreatedById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserSecurityDb>>> SearchAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<AppUserSecurityDb>> actionReturn = new();

        try
        {
            var searchResults =
                await _database.LoadData<AppUserSecurityDb, dynamic>(AppUsersTableMsSql.Search, new { SearchTerm = searchText });
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset =  offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> SearchAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.Search, new { SearchTerm = searchText, Offset =  offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<bool>> IsInRoleAsync(Guid userId, Guid roleId)
    {
        DatabaseActionResult<bool> actionReturn = new();

        try
        {
            var foundMembership = await _database.LoadData<AppUserRoleJunctionDb, dynamic>(
                AppUserRoleJunctionsTableMsSql.GetByUserRoleId, new {UserId = userId, RoleId = roleId});

            var isMember = foundMembership.FirstOrDefault() is not null;
            actionReturn.Succeed(isMember);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserRoleJunctionsTableMsSql.GetByUserRoleId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> AddToRoleAsync(Guid userId, Guid roleId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppUserRoleJunctionsTableMsSql.Insert, new {UserId = userId, RoleId = roleId});
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserRoleJunctionsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> RemoveFromRoleAsync(Guid userId, Guid roleId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppUserRoleJunctionsTableMsSql.Delete, new {UserId = userId, RoleId = roleId});
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserRoleJunctionsTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> AddExtendedAttributeAsync(AppUserExtendedAttributeCreate addAttribute)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(AppUserExtendedAttributesTableMsSql.Insert, addAttribute);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateExtendedAttributeAsync(Guid attributeId, string? value, string? description)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppUserExtendedAttributesTableMsSql.Update,
                new {Id = attributeId, Value = value, Description = description});
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> RemoveExtendedAttributeAsync(Guid attributeId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppUserExtendedAttributesTableMsSql.Delete, new {Id = attributeId});
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdatePreferences(Guid userId, AppUserPreferenceUpdate preferenceUpdate)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var existingPreference = (await _database.LoadData<AppUserPreferenceDb, dynamic>(
                AppUserPreferencesTableMsSql.GetByOwnerId, new {OwnerId = userId})).FirstOrDefault();

            if (existingPreference is null)
                await _database.SaveData(AppUserPreferencesTableMsSql.Insert, preferenceUpdate.ToCreate());
            else
                await _database.SaveData(AppUserPreferencesTableMsSql.Update, preferenceUpdate);

            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, "UpdatePreferences", ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserPreferenceDb>> GetPreferences(Guid userId)
    {
        DatabaseActionResult<AppUserPreferenceDb> actionReturn = new();

        try
        {
            var existingPreference = (await _database.LoadData<AppUserPreferenceDb, dynamic>(
                AppUserPreferencesTableMsSql.GetByOwnerId, new {OwnerId = userId})).FirstOrDefault();

            if (existingPreference is null)
            {
                var newPreferences = new AppUserPreferenceCreate {OwnerId = userId};
                var createdId = await _database.SaveDataReturnId(
                    AppUserPreferencesTableMsSql.Insert, newPreferences);
                existingPreference = (await _database.LoadData<AppUserPreferenceDb, dynamic>(
                        AppUserPreferencesTableMsSql.GetById, new {Id = createdId})).FirstOrDefault();
            }

            actionReturn.Succeed(existingPreference!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserPreferencesTableMsSql.GetByOwnerId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserExtendedAttributeDb>> GetExtendedAttributeByIdAsync(Guid attributeId)
    {
        DatabaseActionResult<AppUserExtendedAttributeDb> actionReturn = new();

        try
        {
            var foundAttribute = (await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                AppUserExtendedAttributesTableMsSql.GetById, new {Id = attributeId})).FirstOrDefault();
            actionReturn.Succeed(foundAttribute!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb?>>> GetExtendedAttributeByTypeAndValueAsync(
        ExtendedAttributeType type, string value)
    {
        DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb?>> actionReturn = new();

        try
        {
            var foundAttribute = await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                AppUserExtendedAttributesTableMsSql.GetByTypeAndValue, new {Type = type, Value = value});
            actionReturn.Succeed(foundAttribute);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.GetByTypeAndValue.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetUserExtendedAttributesByTypeAsync(Guid userId,
        ExtendedAttributeType type)
    {
        DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>> actionReturn = new();

        try
        {
            var foundAttributes = await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                AppUserExtendedAttributesTableMsSql.GetAllOfTypeForOwner, new {OwnerId = userId, Type = type});
            actionReturn.Succeed(foundAttributes);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.GetAllOfTypeForOwner.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetUserExtendedAttributesByTypeAndValueAsync(
        Guid userId, ExtendedAttributeType type, string value)
    {
        DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>> actionReturn = new();

        try
        {
            var foundAttributes = await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                AppUserExtendedAttributesTableMsSql.GetByTypeAndValueForOwner, new {OwnerId = userId, Type = type, Value = value});
            actionReturn.Succeed(foundAttributes);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.GetByTypeAndValueForOwner.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetUserExtendedAttributesByNameAsync(Guid userId,
        string name)
    {
        DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>> actionReturn = new();

        try
        {
            var foundAttributes = await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                AppUserExtendedAttributesTableMsSql.GetAllOfNameForOwner, new {OwnerId = userId, Name = name});
            actionReturn.Succeed(foundAttributes);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.GetAllOfNameForOwner.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetAllUserExtendedAttributesAsync(Guid userId)
    {
        DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>> actionReturn = new();

        try
        {
            var foundAttributes = await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                AppUserExtendedAttributesTableMsSql.GetByOwnerId, new {OwnerId = userId});
            actionReturn.Succeed(foundAttributes);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.GetByOwnerId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetAllExtendedAttributesByTypeAsync(
        ExtendedAttributeType type)
    {
        DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>> actionReturn = new();

        try
        {
            var foundAttributes = await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                AppUserExtendedAttributesTableMsSql.GetAllOfType, new {Type = type});
            actionReturn.Succeed(foundAttributes);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.GetAllOfType.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetAllExtendedAttributesByNameAsync(string name)
    {
        DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>> actionReturn = new();

        try
        {
            var foundAttributes = await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                AppUserExtendedAttributesTableMsSql.GetByName, new {Name = name});
            actionReturn.Succeed(foundAttributes);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.GetByName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetAllExtendedAttributesAsync()
    {
        DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>> actionReturn = new();

        try
        {
            var foundAttributes = await _database.LoadData<AppUserExtendedAttributeDb, dynamic>(
                AppUserExtendedAttributesTableMsSql.GetAll, new { });
            actionReturn.Succeed(foundAttributes);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserExtendedAttributesTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateSecurityAsync(AppUserSecurityAttributeCreate securityCreate)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            Guid securityId;

            var existingSecurity = (await _database.LoadData<AppUserSecurityAttributeDb, dynamic>(
                AppUserSecurityAttributesTableMsSql.GetByOwnerId, new {securityCreate.OwnerId})).FirstOrDefault();

            if (existingSecurity is null)
                securityId = await _database.SaveDataReturnId(AppUserSecurityAttributesTableMsSql.Insert, securityCreate);
            else
            {
                securityId = existingSecurity.Id;
                await UpdateSecurityAsync(existingSecurity.ToUpdate());
            }

            actionReturn.Succeed(securityId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserSecurityAttributesTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppUserSecurityAttributeDb>> GetSecurityAsync(Guid userId)
    {
        DatabaseActionResult<AppUserSecurityAttributeDb> actionReturn = new();

        try
        {
            var userSecurity = (await _database.LoadData<AppUserSecurityAttributeDb, dynamic>(
                AppUserSecurityAttributesTableMsSql.GetByOwnerId, new {OwnerId = userId})).FirstOrDefault();

            actionReturn.Succeed(userSecurity!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserSecurityAttributesTableMsSql.GetByOwnerId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateSecurityAsync(AppUserSecurityAttributeUpdate securityUpdate)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppUserSecurityAttributesTableMsSql.Update, securityUpdate);

            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserSecurityAttributesTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserSecurityDb>>> GetAllLockedOutAsync()
    {
        DatabaseActionResult<IEnumerable<AppUserSecurityDb>> actionReturn = new();

        try
        {
            var allUsers = await _database.LoadData<AppUserSecurityDb, dynamic>(
                AppUsersTableMsSql.GetAllLockedOut, new { });
            actionReturn.Succeed(allUsers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUsersTableMsSql.GetAllLockedOut.Path, ex.Message);
        }

        return actionReturn;
    }
}