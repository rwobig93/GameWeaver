using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Host;
using Application.Repositories.GameServer;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.Database;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.GameServer;
using Infrastructure.Database.MsSql.Shared;

namespace Infrastructure.Repositories.MsSql.GameServer;

public class HostRepositoryMsSql : IHostRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTime;
    private readonly IAuditTrailsRepository _auditRepository;

    public HostRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTime, IAuditTrailsRepository auditRepository)
    {
        _database = database;
        _logger = logger;
        _dateTime = dateTime;
        _auditRepository = auditRepository;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<HostDb>> actionReturn = new();

        try
        {
            var allHosts = await _database.LoadData<HostDb, dynamic>(HostsTableMsSql.GetAll, new { });
            actionReturn.Succeed(allHosts);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostsTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<HostDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allHosts = await _database.LoadData<HostDb, dynamic>(
                HostsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(allHosts);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostsTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {HostsTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<HostDb>> GetByIdAsync(Guid id)
    {
        DatabaseActionResult<HostDb> actionReturn = new();

        try
        {
            var foundHost = (await _database.LoadData<HostDb, dynamic>(
                HostsTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundHost!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostsTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<HostDb>> GetByHostnameAsync(string hostName)
    {
        DatabaseActionResult<HostDb> actionReturn = new();

        try
        {
            var foundHost = (await _database.LoadData<HostDb, dynamic>(
                HostsTableMsSql.GetByHostname, new {Hostname = hostName})).FirstOrDefault();
            actionReturn.Succeed(foundHost!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostsTableMsSql.GetByHostname.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(HostCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createObject.CreatedOn = _dateTime.NowDatabaseTime;

            var createdId = await _database.SaveDataReturnId(HostsTableMsSql.Insert, createObject);

            var foundHost = await GetByIdAsync(createdId);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Hosts, foundHost.Result!.Id,
                createObject.CreatedBy, DatabaseActionType.Create, null, foundHost.Result!.ToSlim());

            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateAsync(HostUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var beforeObject = (await _database.LoadData<HostDb, dynamic>(
                HostsTableMsSql.GetById, new {updateObject.Id})).FirstOrDefault();
            
            await _database.SaveData(HostsTableMsSql.Update, updateObject);
            
            var afterObject = (await _database.LoadData<HostDb, dynamic>(
                HostsTableMsSql.GetById, new {updateObject.Id})).FirstOrDefault();

            var updateDiff = AuditHelpers.GetAuditDiff(beforeObject, afterObject);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Hosts, beforeObject!.Id,
                updateObject.LastModifiedBy.GetFromNullable(), DatabaseActionType.Update, updateDiff.Before, updateDiff.After);
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostsTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var foundHost = await GetByIdAsync(id);
            if (!foundHost.Succeeded || foundHost.Result is null)
                throw new Exception(foundHost.ErrorMessage);
            var hostUpdate = foundHost.Result.ToUpdate();
            
            // Update user w/ a property that is modified so we get the last updated on/by for the deleting user
            hostUpdate.LastModifiedBy = modifyingUserId;
            await UpdateAsync(hostUpdate);
            await _database.SaveData(HostsTableMsSql.Delete, 
                new { Id = id, DeletedOn = _dateTime.NowDatabaseTime });

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Hosts, id,
                hostUpdate.LastModifiedBy.GetFromNullable(), DatabaseActionType.Delete, hostUpdate);
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostsTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostDb>>> SearchAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<HostDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<HostDb, dynamic>(
                HostsTableMsSql.Search, new { SearchTerm = searchText });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostsTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<HostDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var searchResults = await _database.LoadData<HostDb, dynamic>(
                HostsTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostsTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllRegistrationsAsync()
    {
        DatabaseActionResult<IEnumerable<HostRegistrationDb>> actionReturn = new();

        try
        {
            var allRegistrations = await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.GetAll, new {});
            actionReturn.Succeed(allRegistrations);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllRegistrationsPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<HostRegistrationDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allRegistrations = await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(allRegistrations);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllActiveRegistrationsAsync()
    {
        DatabaseActionResult<IEnumerable<HostRegistrationDb>> actionReturn = new();

        try
        {
            var allRegistrations = await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.GetAllActive, new {});
            actionReturn.Succeed(allRegistrations);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.GetAllActive.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllInActiveRegistrationsAsync()
    {
        DatabaseActionResult<IEnumerable<HostRegistrationDb>> actionReturn = new();

        try
        {
            var allRegistrations = await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.GetAllInActive, new {});
            actionReturn.Succeed(allRegistrations);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.GetAllInActive.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetRegistrationCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {HostRegistrationsTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<HostRegistrationDb>> GetRegistrationByIdAsync(Guid id)
    {
        DatabaseActionResult<HostRegistrationDb> actionReturn = new();

        try
        {
            var foundRegistration = (await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundRegistration!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<HostRegistrationDb>> GetRegistrationByHostIdAsync(Guid hostId)
    {
        DatabaseActionResult<HostRegistrationDb> actionReturn = new();

        try
        {
            var foundRegistration = (await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.GetByHostId, new {HostId = hostId})).FirstOrDefault();
            actionReturn.Succeed(foundRegistration!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.GetByHostId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateRegistrationAsync(HostRegistrationCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createObject.CreatedOn = _dateTime.NowDatabaseTime;

            var createdId = await _database.SaveDataReturnId(HostRegistrationsTableMsSql.Insert, createObject);

            var foundRegistration = await GetRegistrationByIdAsync(createdId);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.HostRegistrations, foundRegistration.Result!.Id,
                createObject.CreatedBy, DatabaseActionType.Create, null, foundRegistration.Result!.ToFull());

            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateRegistrationAsync(HostRegistrationUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var beforeObject = (await _database.LoadData<HostDb, dynamic>(
                HostRegistrationsTableMsSql.GetById, new {updateObject.Id})).FirstOrDefault();
            
            await _database.SaveData(HostRegistrationsTableMsSql.Update, updateObject);
            
            var afterObject = (await _database.LoadData<HostDb, dynamic>(
                HostRegistrationsTableMsSql.GetById, new {updateObject.Id})).FirstOrDefault();

            var updateDiff = AuditHelpers.GetAuditDiff(beforeObject, afterObject);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.HostRegistrations, beforeObject!.Id,
                updateObject.LastModifiedBy.GetFromNullable(), DatabaseActionType.Update, updateDiff.Before, updateDiff.After);
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> SearchRegistrationsAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<HostRegistrationDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.Search, new { SearchTerm = searchText });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> SearchRegistrationsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<HostRegistrationDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var searchResults = await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }
}