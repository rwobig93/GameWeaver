using Application.Helpers.Runtime;
using Application.Models.Integrations;
using Application.Repositories.Integrations;
using Application.Services.Database;
using Application.Services.System;
using Domain.Contracts;
using Domain.DatabaseEntities.Integrations;
using Domain.Enums.Integrations;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.Integrations;
using Infrastructure.Database.MsSql.Shared;

namespace Infrastructure.Repositories.MsSql.Integrations;

public class FileStorageRecordRepositoryMsSql : IFileStorageRecordRepository
{
        private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;

    public FileStorageRecordRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTimeService)
    {
        _database = database;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<FileStorageRecordDb>> actionReturn = new();

        try
        {
            var foundRecords = await _database.LoadData<FileStorageRecordDb, dynamic>(FileStorageRecordsTableMsSql.GetAll, new { });
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<FileStorageRecordDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<FileStorageRecordDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<FileStorageRecordDb, dynamic>(
                FileStorageRecordsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<FileStorageRecordDb?>> GetByIdAsync(Guid id)
    {
        DatabaseActionResult<FileStorageRecordDb?> actionReturn = new();

        try
        {
            var foundRecord = (await _database.LoadData<FileStorageRecordDb, dynamic>(
                FileStorageRecordsTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundRecord);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> GetByFormatAsync(FileStorageFormat format)
    {
        DatabaseActionResult<IEnumerable<FileStorageRecordDb>> actionReturn = new();

        try
        {
            var foundRecords = await _database.LoadData<FileStorageRecordDb, dynamic>(
                FileStorageRecordsTableMsSql.GetByFormat, new {Format = format});
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.GetByFormat.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> GetByLinkedIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<FileStorageRecordDb>> actionReturn = new();

        try
        {
            var foundRecords = await _database.LoadData<FileStorageRecordDb, dynamic>(
                FileStorageRecordsTableMsSql.GetByLinkedId, new {LinkedId = id});
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.GetByLinkedId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> GetByLinkedTypeAsync(FileStorageType type)
    {
        DatabaseActionResult<IEnumerable<FileStorageRecordDb>> actionReturn = new();

        try
        {
            var foundRecords = await _database.LoadData<FileStorageRecordDb, dynamic>(
                FileStorageRecordsTableMsSql.GetByLinkedType, new {LinkedType = type});
            actionReturn.Succeed(foundRecords);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.GetByLinkedType.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {FileStorageRecordsTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    async Task<DatabaseActionResult<Guid>> IFileStorageRecordRepository.CreateAsync(FileStorageRecordCreate request)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(FileStorageRecordsTableMsSql.Insert, request);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid id, Guid requestUserId)
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowsDeleted = await _database.SaveData(FileStorageRecordsTableMsSql.Delete, new
            {
                Id = id,
                DeletedBy = requestUserId,
                DeletedOn = _dateTimeService.NowDatabaseTime
            });
            actionReturn.Succeed(rowsDeleted);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> SearchAsync(string searchTerm)
    {
        DatabaseActionResult<IEnumerable<FileStorageRecordDb>> actionReturn = new();

        try
        {
            var searchResults =
                await _database.LoadData<FileStorageRecordDb, dynamic>(FileStorageRecordsTableMsSql.Search, new { SearchTerm = searchTerm });
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<FileStorageRecordDb>>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<FileStorageRecordDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response =
                await _database.LoadDataPaginated<FileStorageRecordDb, dynamic>(
                    FileStorageRecordsTableMsSql.SearchPaginated, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateAsync(FileStorageRecordUpdate request)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(FileStorageRecordsTableMsSql.Update, request);
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, FileStorageRecordsTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }
}