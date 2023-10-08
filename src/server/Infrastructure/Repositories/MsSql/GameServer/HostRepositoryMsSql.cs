using System.Globalization;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.HostRegistration;
using Application.Models.GameServer.WeaverWork;
using Application.Repositories.GameServer;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.Database;
using Domain.Enums.GameServer;
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

    public async Task<DatabaseActionResult<HostRegistrationDb>> GetRegistrationByHostIdAndKeyAsync(Guid hostId, string key)
    {
        DatabaseActionResult<HostRegistrationDb> actionReturn = new();

        try
        {
            var foundRegistration = (await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.GetByHostIdAndKey, new {HostId = hostId, Key = key})).FirstOrDefault();
            actionReturn.Succeed(foundRegistration!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.GetByHostIdAndKey.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetActiveRegistrationsByDescriptionAsync(string description)
    {
        DatabaseActionResult<IEnumerable<HostRegistrationDb>> actionReturn = new();

        try
        {
            var foundRegistrations = await _database.LoadData<HostRegistrationDb, dynamic>(
                HostRegistrationsTableMsSql.GetActiveByDescription, new {Description = description});
            actionReturn.Succeed(foundRegistrations);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostRegistrationsTableMsSql.GetActiveByDescription.Path, ex.Message);
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

    public async Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetAllCheckInsAsync()
    {
        DatabaseActionResult<IEnumerable<HostCheckInDb>> actionReturn = new();

        try
        {
            var allCheckIns = await _database.LoadData<HostCheckInDb, dynamic>(HostCheckInTableMsSql.GetAll, new { });
            
            actionReturn.Succeed(allCheckIns);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetAllCheckInsAfterAsync(DateTime afterDate)
    {
        DatabaseActionResult<IEnumerable<HostCheckInDb>> actionReturn = new();

        try
        {
            var foundCheckIns = await _database.LoadData<HostCheckInDb, dynamic>(
                HostCheckInTableMsSql.GetAllAfter, new {AfterDate = afterDate});
            
            actionReturn.Succeed(foundCheckIns);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.GetAllAfter.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetAllCheckInsPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<HostCheckInDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            
            var foundCheckIns = await _database.LoadData<HostCheckInDb, dynamic>(
                HostCheckInTableMsSql.GetAllPaginated, new {Offset = offset, PageSize = pageSize});
            
            actionReturn.Succeed(foundCheckIns);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCheckInCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {HostCheckInTableMsSql.Table.TableName})).FirstOrDefault();
            
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<HostCheckInDb>> GetCheckInByIdAsync(int id)
    {
        DatabaseActionResult<HostCheckInDb> actionReturn = new();

        try
        {
            var foundCheckIn = (await _database.LoadData<HostCheckInDb, dynamic>(
                HostCheckInTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            
            actionReturn.Succeed(foundCheckIn!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetCheckInByHostIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<HostCheckInDb>> actionReturn = new();

        try
        {
            var foundCheckIns = await _database.LoadData<HostCheckInDb, dynamic>(
                HostCheckInTableMsSql.GetByHostId, new {HostId = id});
            
            actionReturn.Succeed(foundCheckIns);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.GetByHostId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> CreateCheckInAsync(HostCheckInCreate createObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveDataReturnId(HostCheckInTableMsSql.Insert, createObject);

            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> DeleteAllCheckInsForHostIdAsync(Guid id)
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowsDeleted = await _database.SaveData(HostCheckInTableMsSql.DeleteAllForHostId, new {HostId = id});
            actionReturn.Succeed(rowsDeleted);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.DeleteAllForHostId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> DeleteAllOldCheckInsAsync(CleanupTimeframe olderThan)
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var cleanupTimestamp = olderThan switch
            {
                CleanupTimeframe.OneMonth => _dateTime.NowDatabaseTime.AddMonths(-1).ToString(CultureInfo.CurrentCulture),
                CleanupTimeframe.ThreeMonths => _dateTime.NowDatabaseTime.AddMonths(-3).ToString(CultureInfo.CurrentCulture),
                CleanupTimeframe.SixMonths => _dateTime.NowDatabaseTime.AddMonths(-6).ToString(CultureInfo.CurrentCulture),
                CleanupTimeframe.OneYear => _dateTime.NowDatabaseTime.AddYears(-1).ToString(CultureInfo.CurrentCulture),
                CleanupTimeframe.TenYears => _dateTime.NowDatabaseTime.AddYears(-10).ToString(CultureInfo.CurrentCulture),
                _ => _dateTime.NowDatabaseTime.AddMonths(-6).ToString(CultureInfo.CurrentCulture)
            };

            var rowsDeleted = await _database.SaveData(HostCheckInTableMsSql.DeleteOlderThan, new {OlderThan = cleanupTimestamp});
            actionReturn.Succeed(rowsDeleted);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.DeleteOlderThan.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> SearchCheckInsAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<HostCheckInDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<HostCheckInDb, dynamic>(
                HostCheckInTableMsSql.Search, new { SearchTerm = searchText });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> SearchCheckInsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<HostCheckInDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            
            var searchResults = await _database.LoadData<HostCheckInDb, dynamic>(
                HostCheckInTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, HostCheckInTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetAllWeaverWorkAsync()
    {
        DatabaseActionResult<IEnumerable<WeaverWorkDb>> actionReturn = new();

        try
        {
            var allWork = await _database.LoadData<WeaverWorkDb, dynamic>(WeaverWorksTableMsSql.GetAll, new { });
            
            actionReturn.Succeed(allWork);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, WeaverWorksTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetAllWeaverWorkPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<WeaverWorkDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allWork = await _database.LoadData<WeaverWorkDb, dynamic>(
                WeaverWorksTableMsSql.GetAllPaginated, new {Offset = offset, PageSize = pageSize});
            
            actionReturn.Succeed(allWork);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, WeaverWorksTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetWeaverWorkCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {WeaverWorksTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<WeaverWorkDb>> GetWeaverWorkByIdAsync(Guid id)
    {
        DatabaseActionResult<WeaverWorkDb> actionReturn = new();

        try
        {
            var foundWork = (await _database.LoadData<WeaverWorkDb, dynamic>(
                WeaverWorksTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundWork!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, WeaverWorksTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverWorkByHostIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<WeaverWorkDb>> actionReturn = new();

        try
        {
            var foundWork = await _database.LoadData<WeaverWorkDb, dynamic>(
                WeaverWorksTableMsSql.GetByHostId, new {HostId = id});
            actionReturn.Succeed(foundWork);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, WeaverWorksTableMsSql.GetByHostId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverWorkByGameServerIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<WeaverWorkDb>> actionReturn = new();

        try
        {
            var foundWork = await _database.LoadData<WeaverWorkDb, dynamic>(
                WeaverWorksTableMsSql.GetByGameServerId, new {GameServerId = id});
            actionReturn.Succeed(foundWork);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, WeaverWorksTableMsSql.GetByGameServerId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverWorkByTargetTypeAsync(WeaverWorkTarget target)
    {
        DatabaseActionResult<IEnumerable<WeaverWorkDb>> actionReturn = new();

        try
        {
            var foundWork = await _database.LoadData<WeaverWorkDb, dynamic>(
                WeaverWorksTableMsSql.GetByTargetType, new {TargetType = target});
            actionReturn.Succeed(foundWork);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, WeaverWorksTableMsSql.GetByTargetType.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverWorkByStatusAsync(WeaverWorkState status)
    {
        DatabaseActionResult<IEnumerable<WeaverWorkDb>> actionReturn = new();

        try
        {
            var foundWork = await _database.LoadData<WeaverWorkDb, dynamic>(
                WeaverWorksTableMsSql.GetByStatus, new {Status = status});
            actionReturn.Succeed(foundWork);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, WeaverWorksTableMsSql.GetByStatus.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateWeaverWorkAsync(WeaverWorkCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createObject.CreatedOn = _dateTime.NowDatabaseTime;

            var createdId = await _database.SaveDataReturnId(WeaverWorksTableMsSql.Insert, createObject);

            var foundWork = await GetWeaverWorkByIdAsync(createdId);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundWork.Result!.Id,
                createObject.CreatedBy, DatabaseActionType.Create, null, foundWork.Result!.ToSlim());

            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, WeaverWorksTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateWeaverWorkAsync(WeaverWorkUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteWeaverWorkAsync(Guid id, Guid modifyingUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> SearchWeaverWorkAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> SearchWeaverWorkPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }
}