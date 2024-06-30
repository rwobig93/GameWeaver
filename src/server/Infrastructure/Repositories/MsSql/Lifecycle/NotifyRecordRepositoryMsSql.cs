using Application.Helpers.Runtime;
using Application.Models.Lifecycle;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.DatabaseEntities.Lifecycle;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.Lifecycle;

namespace Infrastructure.Repositories.MsSql.Lifecycle;

public class NotifyRecordRepositoryMsSql : INotifyRecordRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;

    public NotifyRecordRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTimeService)
    {
        _database = database;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<NotifyRecordDb>> actionReturn = new();

        try
        {
            var foundRecords = await _database.LoadData<NotifyRecordDb, dynamic>(NotifyRecordsTableMsSql.GetAll, new { });
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<NotifyRecordDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var foundRecords = await _database.LoadData<NotifyRecordDb, dynamic>(
                NotifyRecordsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<NotifyRecordDb?>> GetByIdAsync(int id)
    {
        DatabaseActionResult<NotifyRecordDb?> actionReturn = new();

        try
        {
            var foundRecord = (await _database.LoadData<NotifyRecordDb, dynamic>(
                NotifyRecordsTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundRecord);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> GetByRecordIdAsync(Guid recordId, int recordCount)
    {
        DatabaseActionResult<IEnumerable<NotifyRecordDb>> actionReturn = new();

        try
        {
            var foundRecords = await _database.LoadData<NotifyRecordDb, dynamic>(
                NotifyRecordsTableMsSql.GetByRecordId, new {RecordId = recordId, RecordCount = recordCount});
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.GetByRecordId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> GetAllByRecordIdAsync(Guid recordId)
    {
        DatabaseActionResult<IEnumerable<NotifyRecordDb>> actionReturn = new();

        try
        {
            var foundRecords = await _database.LoadData<NotifyRecordDb, dynamic>(NotifyRecordsTableMsSql.GetAllByRecordId, new {RecordId = recordId});
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.GetAllByRecordId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> CreateAsync(NotifyRecordCreate createObject)
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            createObject.Timestamp = _dateTimeService.NowDatabaseTime;
            
            var createdId = await _database.SaveDataReturnIntId(NotifyRecordsTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> SearchAsync(string searchTerm)
    {
        DatabaseActionResult<IEnumerable<NotifyRecordDb>> actionReturn = new();

        try
        {
            var searchResults =
                await _database.LoadData<NotifyRecordDb, dynamic>(NotifyRecordsTableMsSql.Search, new { SearchTerm = searchTerm });
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<NotifyRecordDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var searchResults =
                await _database.LoadData<NotifyRecordDb, dynamic>(
                    NotifyRecordsTableMsSql.SearchPaginated, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize });
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> DeleteOlderThan(DateTime olderThan)
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowsDeleted = await _database.SaveData(NotifyRecordsTableMsSql.DeleteOlderThan, new {OlderThan = olderThan});
            actionReturn.Succeed(rowsDeleted);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.DeleteOlderThan.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> DeleteAllForRecordId(Guid recordId)
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowsDeleted = await _database.SaveData(NotifyRecordsTableMsSql.DeleteAllForRecordId, new {RecordId = recordId});
            actionReturn.Succeed(rowsDeleted);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.DeleteAllForRecordId.Path, ex.Message);
        }

        return actionReturn;
    }
}