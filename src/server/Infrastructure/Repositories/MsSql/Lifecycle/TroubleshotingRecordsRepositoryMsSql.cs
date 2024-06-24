using System.Globalization;
using Application.Helpers.Runtime;
using Application.Models.Lifecycle;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.DatabaseEntities.Lifecycle;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.Lifecycle;
using Infrastructure.Database.MsSql.Shared;

namespace Infrastructure.Repositories.MsSql.Lifecycle;

public class TroubleshootingRecordsRepositoryMsSql : ITroubleshootingRecordsRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;

    public TroubleshootingRecordsRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTimeService)
    {
        _database = database;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }
    
    public async Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>> actionReturn = new();

        try
        {
            var allTroubleshootingRecords = await _database.LoadData<TroubleshootingRecordDb, dynamic>(TroubleshootingRecordsTableMsSql.GetAll, new { });
            actionReturn.Succeed(allTroubleshootingRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allTroubleshootingRecords = await _database.LoadData<TroubleshootingRecordDb, dynamic>(
                TroubleshootingRecordsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(allTroubleshootingRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }
    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {TroubleshootingRecordsTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<TroubleshootingRecordDb?>> GetByIdAsync(Guid id)
    {
        DatabaseActionResult<TroubleshootingRecordDb?> actionReturn = new();

        try
        {
            var foundTroubleshootingRecord = (await _database.LoadData<TroubleshootingRecordDb, dynamic>(
                TroubleshootingRecordsTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetByChangedByIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>> actionReturn = new();

        try
        {
            var foundTroubleshootingRecord = (await _database.LoadData<TroubleshootingRecordDb, dynamic>(
                TroubleshootingRecordsTableMsSql.GetByChangedBy, new {UserId = id}));
            actionReturn.Succeed(foundTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.GetByChangedBy.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetByEntityTypeAsync(TroubleshootEntityType type)
    {
        DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>> actionReturn = new();

        try
        {
            var foundTroubleshootingRecord = await _database.LoadData<TroubleshootingRecordDb, dynamic>(
                TroubleshootingRecordsTableMsSql.GetByEntityType, new {EntityType = type});
            actionReturn.Succeed(foundTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.GetByEntityType.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetByRecordIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>> actionReturn = new();

        try
        {
            var foundTroubleshootingRecord = (await _database.LoadData<TroubleshootingRecordDb, dynamic>(
                TroubleshootingRecordsTableMsSql.GetByRecordId, new {RecordId = id}));
            actionReturn.Succeed(foundTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.GetByRecordId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(TroubleshootingRecordCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createObject.Timestamp = _dateTimeService.NowDatabaseTime;
            
            var createdId = await _database.SaveDataReturnId(TroubleshootingRecordsTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> SearchAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>> actionReturn = new();

        try
        {
            var searchResults =
                await _database.LoadData<TroubleshootingRecordDb, dynamic>(TroubleshootingRecordsTableMsSql.Search, new { SearchTerm = searchText });
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var searchResults =
                await _database.LoadData<TroubleshootingRecordDb, dynamic>(
                    TroubleshootingRecordsTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> DeleteOlderThan(CleanupTimeframe olderThan)
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

            var rowsDeleted = await _database.SaveData(TroubleshootingRecordsTableMsSql.DeleteOlderThan, new {OlderThan = cleanupTimestamp});
            actionReturn.Succeed(rowsDeleted);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, TroubleshootingRecordsTableMsSql.DeleteOlderThan.Path, ex.Message);
        }

        return actionReturn;
    }
}