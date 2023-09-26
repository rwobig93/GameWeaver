using Application.Models.Lifecycle;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.DatabaseEntities.Lifecycle;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.Lifecycle;
using Infrastructure.Database.MsSql.Shared;

namespace Infrastructure.Repositories.MsSql.Lifecycle;

public class ServerStateRecordsRepositoryMsSql : IServerStateRecordsRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTime;

    public ServerStateRecordsRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTime)
    {
        _database = database;
        _logger = logger;
        _dateTime = dateTime;
    }

    public async Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<ServerStateRecordDb>> actionReturn = new();

        try
        {
            var allStateRecords = await _database.LoadData<ServerStateRecordDb, dynamic>(
                ServerStateRecordsTableMsSql.GetAll, new { });
            actionReturn.Succeed(allStateRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ServerStateRecordsTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetAllBeforeDateAsync(DateTime olderThan)
    {
        DatabaseActionResult<IEnumerable<ServerStateRecordDb>> actionReturn = new();

        try
        {
            var foundStateRecords = await _database.LoadData<ServerStateRecordDb, dynamic>(
                ServerStateRecordsTableMsSql.GetAllBeforeDate, new {OlderThan = olderThan});
            actionReturn.Succeed(foundStateRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ServerStateRecordsTableMsSql.GetAllBeforeDate.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetAllAfterDateAsync(DateTime newerThan)
    {
        DatabaseActionResult<IEnumerable<ServerStateRecordDb>> actionReturn = new();

        try
        {
            var foundStateRecords = await _database.LoadData<ServerStateRecordDb, dynamic>(
                ServerStateRecordsTableMsSql.GetAllAfterDate, new {NewerThan = newerThan});
            actionReturn.Succeed(foundStateRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ServerStateRecordsTableMsSql.GetAllAfterDate.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {ServerStateRecordsTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<ServerStateRecordDb>> GetLatestAsync()
    {
        DatabaseActionResult<ServerStateRecordDb> actionReturn = new();

        try
        {
            var foundServerState = (await _database.LoadData<ServerStateRecordDb, dynamic>(
                ServerStateRecordsTableMsSql.GetLatest, new {})).FirstOrDefault();
            actionReturn.Succeed(foundServerState!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ServerStateRecordsTableMsSql.GetLatest.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<ServerStateRecordDb>> GetByIdAsync(Guid id)
    {
        DatabaseActionResult<ServerStateRecordDb> actionReturn = new();

        try
        {
            var foundServerState = (await _database.LoadData<ServerStateRecordDb, dynamic>(
                ServerStateRecordsTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundServerState!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ServerStateRecordsTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetByAppVersionAsync(Version version)
    {
        DatabaseActionResult<IEnumerable<ServerStateRecordDb>> actionReturn = new();

        try
        {
            var foundStateRecords = await _database.LoadData<ServerStateRecordDb, dynamic>(
                ServerStateRecordsTableMsSql.GetByAppVersion, new {Version = version.ToString()});
            actionReturn.Succeed(foundStateRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ServerStateRecordsTableMsSql.GetByAppVersion.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ServerStateRecordDb>>> GetByDatabaseVersionAsync(Version version)
    {
        DatabaseActionResult<IEnumerable<ServerStateRecordDb>> actionReturn = new();

        try
        {
            var foundStateRecords = await _database.LoadData<ServerStateRecordDb, dynamic>(
                ServerStateRecordsTableMsSql.GetByDatabaseVersion, new {Version = version.ToString()});
            actionReturn.Succeed(foundStateRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ServerStateRecordsTableMsSql.GetByDatabaseVersion.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(ServerStateRecordCreate createRecord)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createRecord.Timestamp = _dateTime.NowDatabaseTime;
            
            var createdId = await _database.SaveDataReturnId(ServerStateRecordsTableMsSql.Insert, createRecord);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ServerStateRecordsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }
}