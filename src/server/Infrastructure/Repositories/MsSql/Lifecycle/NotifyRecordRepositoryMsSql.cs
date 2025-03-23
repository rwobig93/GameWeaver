using Application.Helpers.Runtime;
using Application.Models.Lifecycle;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.Contracts;
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

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<NotifyRecordDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<NotifyRecordDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<NotifyRecordDb, dynamic>(
                NotifyRecordsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            
            response.UpdatePaginationProperties(pageNumber, pageSize);
            
            actionReturn.Succeed(response);
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

    public async Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> GetByEntityIdAsync(Guid id, int recordCount)
    {
        DatabaseActionResult<IEnumerable<NotifyRecordDb>> actionReturn = new();

        try
        {
            var foundRecords = await _database.LoadData<NotifyRecordDb, dynamic>(
                NotifyRecordsTableMsSql.GetByEntityId, new {EntityId = id, RecordCount = recordCount});
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.GetByEntityId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> GetAllByEntityIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<NotifyRecordDb>> actionReturn = new();

        try
        {
            var foundRecords = await _database.LoadData<NotifyRecordDb, dynamic>(NotifyRecordsTableMsSql.GetAllByEntityId, new {EntityId = id});
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.GetAllByEntityId.Path, ex.Message);
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

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<NotifyRecordDb>>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<NotifyRecordDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response =
                await _database.LoadDataPaginated<NotifyRecordDb, dynamic>(
                    NotifyRecordsTableMsSql.SearchPaginated, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize });
            
            response.UpdatePaginationProperties(pageNumber, pageSize);
            
            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<NotifyRecordDb>>>> SearchPaginatedByEntityIdAsync(Guid id, string searchTerm, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<NotifyRecordDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response =
                await _database.LoadDataPaginated<NotifyRecordDb, dynamic>(
                    NotifyRecordsTableMsSql.SearchPaginatedByEntityId, new { Id = id, SearchTerm = searchTerm, Offset = offset, PageSize = pageSize });
            
            response.UpdatePaginationProperties(pageNumber, pageSize);
            
            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.SearchPaginatedByEntityId.Path, ex.Message);
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

    public async Task<DatabaseActionResult<int>> DeleteAllForEntityId(Guid id)
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowsDeleted = await _database.SaveData(NotifyRecordsTableMsSql.DeleteAllForEntityId, new {EntityId = id});
            actionReturn.Succeed(rowsDeleted);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, NotifyRecordsTableMsSql.DeleteAllForEntityId.Path, ex.Message);
        }

        return actionReturn;
    }
}