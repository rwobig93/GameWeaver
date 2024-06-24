using Application.Mappers.Lifecycle;
using Application.Models.Lifecycle;
using Application.Repositories.Lifecycle;
using Application.Services.Lifecycle;
using Application.Services.System;
using Domain.Contracts;
using Domain.Enums.Lifecycle;

namespace Infrastructure.Services.Lifecycle;

public class TroubleshootingRecordService : ITroubleshootingRecordService
{
    private readonly ITroubleshootingRecordsRepository _tshootRepository;
    private readonly ISerializerService _serializer;
    private readonly ILogger _logger;

    public TroubleshootingRecordService(ITroubleshootingRecordsRepository tshootRepository, ISerializerService serializer, ILogger logger)
    {
        _tshootRepository = tshootRepository;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetAllAsync()
    {
        try
        {
            var troubleshootingRecords = await _tshootRepository.GetAllAsync();
            if (!troubleshootingRecords.Succeeded)
            {
                return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(troubleshootingRecords.ErrorMessage);
            }

            var convertedTroubleshootingRecords = troubleshootingRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<TroubleshootingRecordSlim>>.SuccessAsync(convertedTroubleshootingRecords);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            var troubleshootingRecords = await _tshootRepository.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!troubleshootingRecords.Succeeded)
            {
                return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(troubleshootingRecords.ErrorMessage);
            }

            var convertedTroubleshootingRecords = troubleshootingRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<TroubleshootingRecordSlim>>.SuccessAsync(convertedTroubleshootingRecords);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        try
        {
            var troubleshootingRecordCount = await _tshootRepository.GetCountAsync();
            if (!troubleshootingRecordCount.Succeeded)
            {
                return await Result<int>.FailAsync(troubleshootingRecordCount.ErrorMessage);
            }

            return await Result<int>.SuccessAsync(troubleshootingRecordCount.Result);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<TroubleshootingRecordSlim?>> GetByIdAsync(Guid id)
    {
        try
        {
            var troubleshootingRecord = await _tshootRepository.GetByIdAsync(id);
            if (!troubleshootingRecord.Succeeded)
            {
                return await Result<TroubleshootingRecordSlim?>.FailAsync(troubleshootingRecord.ErrorMessage);
            }

            var convertedTroubleshootingRecord = troubleshootingRecord.Result?.ToSlim();

            return await Result<TroubleshootingRecordSlim?>.SuccessAsync(convertedTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            return await Result<TroubleshootingRecordSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetByChangedByIdAsync(Guid id)
    {
        try
        {
            var troubleshootingRecords = await _tshootRepository.GetByChangedByIdAsync(id);
            if (!troubleshootingRecords.Succeeded)
            {
                return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(troubleshootingRecords.ErrorMessage);
            }

            var convertedTroubleshootingRecord = troubleshootingRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<TroubleshootingRecordSlim>>.SuccessAsync(convertedTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetByEntityTypeAsync(TroubleshootEntityType type)
    {
        try
        {
            var troubleshootingRecords = await _tshootRepository.GetByEntityTypeAsync(type);
            if (!troubleshootingRecords.Succeeded)
            {
                return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(troubleshootingRecords.ErrorMessage);
            }

            var convertedTroubleshootingRecord = troubleshootingRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<TroubleshootingRecordSlim>>.SuccessAsync(convertedTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetByRecordIdAsync(Guid id)
    {
        try
        {
            var troubleshootingRecords = await _tshootRepository.GetByRecordIdAsync(id);
            if (!troubleshootingRecords.Succeeded)
            {
                return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(troubleshootingRecords.ErrorMessage);
            }

            var convertedTroubleshootingRecord = troubleshootingRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<TroubleshootingRecordSlim>>.SuccessAsync(convertedTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<Guid>> CreateAsync(TroubleshootingRecordCreate createObject)
    {
        try
        {
            var createRecord = await _tshootRepository.CreateAsync(createObject);
            if (!createRecord.Succeeded)
            {
                return await Result<Guid>.FailAsync(createRecord.ErrorMessage);
            }

            return await Result<Guid>.SuccessAsync(createRecord.Result);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> SearchAsync(string searchText)
    {
        try
        {
            var troubleshootingRecords = await _tshootRepository.SearchAsync(searchText);
            if (!troubleshootingRecords.Succeeded)
            {
                return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(troubleshootingRecords.ErrorMessage);
            }

            var convertedTroubleshootingRecord = troubleshootingRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<TroubleshootingRecordSlim>>.SuccessAsync(convertedTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        try
        {
            var troubleshootingRecords = await _tshootRepository.SearchPaginatedAsync(searchText, pageNumber, pageSize);
            if (!troubleshootingRecords.Succeeded)
            {
                return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(troubleshootingRecords.ErrorMessage);
            }

            var convertedTroubleshootingRecord = troubleshootingRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<TroubleshootingRecordSlim>>.SuccessAsync(convertedTroubleshootingRecord);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<TroubleshootingRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<int>> DeleteOlderThan(CleanupTimeframe olderThan)
    {
        try
        {
            var deleteRecords = await _tshootRepository.DeleteOlderThan(olderThan);
            if (!deleteRecords.Succeeded)
            {
                return await Result<int>.FailAsync(deleteRecords.ErrorMessage);
            }
            
            switch (deleteRecords.Result)
            {
                case > 0:
                    _logger.Information("Successfully cleaned up {RecordCount} old troubleshooting record", deleteRecords.Result);
                    break;
                case <= 0:
                    _logger.Information("No troubleshooting records older than {Timeframe} found, no records cleaned up", olderThan.ToString());
                    break;
            }

            return await Result<int>.SuccessAsync(deleteRecords.Result);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
}