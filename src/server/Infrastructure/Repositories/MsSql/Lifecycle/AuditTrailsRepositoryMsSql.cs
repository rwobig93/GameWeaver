using System.Globalization;
using Application.Helpers.Runtime;
using Application.Models.Lifecycle;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.Contracts;
using Domain.DatabaseEntities.Lifecycle;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.Lifecycle;
using Infrastructure.Database.MsSql.Shared;

namespace Infrastructure.Repositories.MsSql.Lifecycle;

public class AuditTrailsRepositoryMsSql : IAuditTrailsRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;

    public AuditTrailsRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTimeService)
    {
        _database = database;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<DatabaseActionResult<IEnumerable<AuditTrailDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<AuditTrailDb>> actionReturn = new();

        try
        {
            var allAuditTrails = await _database.LoadData<AuditTrailDb, dynamic>(AuditTrailsTableMsSql.GetAll, new { });
            actionReturn.Succeed(allAuditTrails);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> GetAllWithUsersAsync()
    {
        DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>> actionReturn = new();

        try
        {
            var allAuditTrails = await _database.LoadData<AuditTrailWithUserDb, dynamic>(
                AuditTrailsTableMsSql.GetAllWithUsers, new { });
            actionReturn.Succeed(allAuditTrails);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.GetAllWithUsers.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AuditTrailDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AuditTrailDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<AuditTrailDb, dynamic>(
                AuditTrailsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AuditTrailWithUserDb>>>> GetAllPaginatedWithUsersAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AuditTrailWithUserDb>>> actionReturn = new();

        try
        {
            var offset = (pageNumber - 1) * pageSize;
            var response = await _database.LoadDataPaginated<AuditTrailWithUserDb, dynamic>(
                AuditTrailsTableMsSql.GetAllPaginatedWithUsers, new {Offset =  offset, PageSize = pageSize});
            
            response.UpdatePaginationProperties(pageNumber, pageSize);
            
            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.GetAllPaginatedWithUsers.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {AuditTrailsTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AuditTrailDb>> GetByIdAsync(Guid id)
    {
        DatabaseActionResult<AuditTrailDb> actionReturn = new();

        try
        {
            var foundAuditTrail = (await _database.LoadData<AuditTrailDb, dynamic>(
                AuditTrailsTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundAuditTrail!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<AuditTrailWithUserDb>> GetByIdWithUserAsync(Guid id)
    {
        DatabaseActionResult<AuditTrailWithUserDb> actionReturn = new();

        try
        {
            var foundAuditTrail = (await _database.LoadData<AuditTrailWithUserDb, dynamic>(
                AuditTrailsTableMsSql.GetByIdWithUser, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundAuditTrail!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.GetByIdWithUser.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> GetByChangedByIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>> actionReturn = new();

        try
        {
            var foundAuditTrail = (await _database.LoadData<AuditTrailWithUserDb, dynamic>(
                AuditTrailsTableMsSql.GetByChangedBy, new {UserId = id}));
            actionReturn.Succeed(foundAuditTrail);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.GetByChangedBy.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> GetByRecordIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>> actionReturn = new();

        try
        {
            var foundAuditTrail = (await _database.LoadData<AuditTrailWithUserDb, dynamic>(
                AuditTrailsTableMsSql.GetByRecordId, new {RecordId = id}));
            actionReturn.Succeed(foundAuditTrail);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.GetByRecordId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(AuditTrailCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createObject.Timestamp = _dateTimeService.NowDatabaseTime;

            var createdId = await _database.SaveDataReturnId(AuditTrailsTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AuditTrailDb>>> SearchAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<AuditTrailDb>> actionReturn = new();

        try
        {
            var searchResults =
                await _database.LoadData<AuditTrailDb, dynamic>(AuditTrailsTableMsSql.Search, new { SearchTerm = searchText });
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AuditTrailDb>>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AuditTrailDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response =
                await _database.LoadDataPaginated<AuditTrailDb, dynamic>(
                    AuditTrailsTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AuditTrailWithUserDb>>>> SearchPaginatedWithUserAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<AuditTrailWithUserDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response =
                await _database.LoadDataPaginated<AuditTrailWithUserDb, dynamic>(
                    AuditTrailsTableMsSql.SearchPaginatedWithUser, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.SearchPaginatedWithUser.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> SearchWithUserAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>> actionReturn = new();

        try
        {
            var searchResults =
                await _database.LoadData<AuditTrailWithUserDb, dynamic>(AuditTrailsTableMsSql.SearchWithUser, new { SearchTerm = searchText });
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.SearchWithUser.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> DeleteOld(CleanupTimeframe olderThan)
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var cleanupTimestamp = olderThan switch
            {
                CleanupTimeframe.OneMonth => _dateTimeService.NowDatabaseTime.AddMonths(-1).ToString(CultureInfo.CurrentCulture),
                CleanupTimeframe.ThreeMonths => _dateTimeService.NowDatabaseTime.AddMonths(-3).ToString(CultureInfo.CurrentCulture),
                CleanupTimeframe.SixMonths => _dateTimeService.NowDatabaseTime.AddMonths(-6).ToString(CultureInfo.CurrentCulture),
                CleanupTimeframe.OneYear => _dateTimeService.NowDatabaseTime.AddYears(-1).ToString(CultureInfo.CurrentCulture),
                CleanupTimeframe.TenYears => _dateTimeService.NowDatabaseTime.AddYears(-10).ToString(CultureInfo.CurrentCulture),
                _ => _dateTimeService.NowDatabaseTime.AddMonths(-6).ToString(CultureInfo.CurrentCulture)
            };

            var rowsDeleted = await _database.SaveData(AuditTrailsTableMsSql.DeleteOlderThan, new {OlderThan = cleanupTimestamp});
            actionReturn.Succeed(rowsDeleted);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, AuditTrailsTableMsSql.DeleteOlderThan.Path, ex.Message);
        }

        return actionReturn;
    }
}