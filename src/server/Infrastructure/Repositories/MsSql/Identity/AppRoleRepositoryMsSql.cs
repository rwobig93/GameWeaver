using Application.Constants.Communication;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.Role;
using Application.Repositories.Identity;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.Contracts;
using Domain.DatabaseEntities.Identity;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.Identity;
using Infrastructure.Database.MsSql.Shared;

namespace Infrastructure.Repositories.MsSql.Identity;

public class AppRoleRepositoryMsSql : IAppRoleRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IAuditTrailsRepository _auditRepository;
    private readonly IDateTimeService _dateTimeService;

    public AppRoleRepositoryMsSql(ISqlDataService database, ILogger logger, IAuditTrailsRepository auditRepository,
        IDateTimeService dateTimeService)
    {
        _database = database;
        _logger = logger;
        _auditRepository = auditRepository;
        _dateTimeService = dateTimeService;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppRoleDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<AppRoleDb>> actionReturn = new();

        try
        {
            var allRoles = await _database.LoadData<AppRoleDb, dynamic>(AppRolesTableMsSql.GetAll, new { });
            actionReturn.Succeed(allRoles);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppRoleDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppRoleDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<AppRoleDb, dynamic>(
                AppRolesTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            
            response.UpdatePaginationProperties(pageNumber, pageSize);
            
            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {AppRolesTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppRoleDb>> GetByIdAsync(Guid roleId)
    {
        DatabaseActionResult<AppRoleDb> actionReturn = new();

        try
        {
            var foundRole = (await _database.LoadData<AppRoleDb, dynamic>(AppRolesTableMsSql.GetById, new {Id = roleId})).FirstOrDefault();
            actionReturn.Succeed(foundRole!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AppRoleDb>> GetByNameAsync(string roleName)
    {
        DatabaseActionResult<AppRoleDb> actionReturn = new();

        try
        {
            var foundRole = (await _database.LoadData<AppRoleDb, dynamic>(
                AppRolesTableMsSql.GetByName, new {Name = roleName})).FirstOrDefault();
            actionReturn.Succeed(foundRole!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.GetByName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppRoleDb>>> SearchAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<AppRoleDb>> actionReturn = new();

        try
        {
            var searchResults =
                await _database.LoadData<AppRoleDb, dynamic>(AppRolesTableMsSql.Search, new { SearchTerm = searchText });
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppRoleDb>>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppRoleDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<AppRoleDb, dynamic>(
                AppRolesTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset =  offset, PageSize = pageSize });
            
            response.UpdatePaginationProperties(pageNumber, pageSize);
            
            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(AppRoleCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createObject.CreatedOn = _dateTimeService.NowDatabaseTime;
            
            var createdId = await _database.SaveDataReturnId(AppRolesTableMsSql.Insert, createObject);

            var createdRole = await GetByIdAsync(createdId);

            await _auditRepository.CreateAuditTrail(_dateTimeService, AuditTableName.Roles, createdId,
                createObject.CreatedBy, AuditAction.Create, null, createdRole.Result);
            
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateAsync(AppRoleUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            // Get role before update for auditing
            var foundRole = await GetByIdAsync(updateObject.Id);

            updateObject.LastModifiedOn = _dateTimeService.NowDatabaseTime;
            
            await _database.SaveData(AppRolesTableMsSql.Update, updateObject);
            
            // Get role after update for auditing
            var foundRoleAfterUpdate = await GetByIdAsync(updateObject.Id);

            await _auditRepository.CreateAuditTrail(_dateTimeService, AuditTableName.Roles, updateObject.Id,
                updateObject.LastModifiedBy.GetFromNullable(), AuditAction.Update,
                foundRole.Result!.ToSlim(), foundRoleAfterUpdate.Result!.ToSlim());
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            // Get role before deletion for auditing
            var foundRole = await GetByIdAsync(id);
            
            await _database.SaveData(AppRolesTableMsSql.Delete, new {Id = id});

            await _auditRepository.CreateAuditTrail(_dateTimeService, AuditTableName.Roles, id,
                modifyingUserId, AuditAction.Delete, foundRole.Result!.ToSlim());
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> SetCreatedById(Guid roleId, Guid createdById)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppRolesTableMsSql.SetCreatedById, new { Id = roleId, CreatedBy = createdById });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppRolesTableMsSql.SetCreatedById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<bool>> IsUserInRoleAsync(Guid userId, Guid roleId)
    {
        DatabaseActionResult<bool> actionReturn = new();

        try
        {
            var userRoleJunction = (await _database.LoadData<AppUserRoleJunctionDb, dynamic>(
                AppUserRoleJunctionsTableMsSql.GetByUserRoleId, new {UserId = userId, RoleId = roleId})).FirstOrDefault();
            var hasRole = userRoleJunction is not null;
            actionReturn.Succeed(hasRole);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserRoleJunctionsTableMsSql.GetByUserRoleId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<bool>> IsUserInRoleAsync(Guid userId, string roleName)
    {
        DatabaseActionResult<bool> actionReturn = new();

        try
        {
            var foundRole = (await _database.LoadData<AppRoleDb, dynamic>(
                AppRolesTableMsSql.GetByName, new {Name = roleName})).FirstOrDefault();
            if (foundRole is null)
            {
                actionReturn.Fail(ErrorMessageConstants.Generic.NotFound);
                return actionReturn;
            }
            
            var userRoleJunction = (await _database.LoadData<AppUserRoleJunctionDb, dynamic>(
                AppUserRoleJunctionsTableMsSql.GetByUserRoleId, new {UserId = userId, RoleId = foundRole.Id})).FirstOrDefault();
            var hasRole = userRoleJunction is not null;
            actionReturn.Succeed(hasRole);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, "IsUserInRoleAsync_RoleName", ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> AddUserToRoleAsync(Guid userId, Guid roleId, Guid modifyingUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppUserRoleJunctionsTableMsSql.Insert, 
                new {UserId = userId, RoleId = roleId});

            await _auditRepository.CreateAuditTrail(_dateTimeService, AuditTableName.Roles, roleId,
                modifyingUserId, AuditAction.Update, null, new Dictionary<string, string>()
                {
                    {"User Addition", userId.ToString()}
                });
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserRoleJunctionsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> RemoveUserFromRoleAsync(Guid userId, Guid roleId, Guid modifyingUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(AppUserRoleJunctionsTableMsSql.Delete, 
                new {UserId = userId, RoleId = roleId});

            await _auditRepository.CreateAuditTrail(_dateTimeService, AuditTableName.Roles, roleId,
                modifyingUserId, AuditAction.Update, null, new Dictionary<string, string>()
                {
                    {"User Removal", userId.ToString()}
                });
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserRoleJunctionsTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppRoleDb>>> GetRolesForUser(Guid userId)
    {
        DatabaseActionResult<IEnumerable<AppRoleDb>> actionReturn = new();

        try
        {
            var roles = await _database.LoadData<AppRoleDb, dynamic>(
                AppUserRoleJunctionsTableMsSql.GetRolesOfUser, new {UserId = userId});

            actionReturn.Succeed(roles);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserRoleJunctionsTableMsSql.GetRolesOfUser.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AppUserDb>>> GetUsersForRole(Guid roleId)
    {
        DatabaseActionResult<IEnumerable<AppUserDb>> actionReturn = new();

        try
        {
            var users = await _database.LoadData<AppUserDb, dynamic>(
                AppUserRoleJunctionsTableMsSql.GetUsersOfRole, new {RoleId = roleId});
            
            actionReturn.Succeed(users);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AppUserRoleJunctionsTableMsSql.GetUsersOfRole.Path, ex.Message);
        }

        return actionReturn;
    }
}