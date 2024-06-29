using Application.Mappers.Integrations;
using Application.Models.System;
using Application.Repositories.Integrations;
using Application.Requests.Integrations;
using Application.Services.Integrations;
using Application.Services.System;
using Domain.Contracts;
using Domain.Enums.Integrations;

namespace Infrastructure.Services.Integrations;

public class FileStorageRecordService : IFileStorageRecordService
{
    private readonly IFileStorageRecordRepository _recordRepository;
    private readonly IDateTimeService _dateTime;

    public FileStorageRecordService(IFileStorageRecordRepository recordRepository, IDateTimeService dateTime)
    {
        _recordRepository = recordRepository;
        _dateTime = dateTime;
    }

    public async Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetAllAsync()
    {
        try
        {
            var foundRecords = await _recordRepository.GetAllAsync();
            if (!foundRecords.Succeeded)
            {
                return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(foundRecords.ErrorMessage);
            }

            var convertedRecords = foundRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<FileStorageRecordSlim>>.SuccessAsync(convertedRecords);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            var foundRecords = await _recordRepository.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!foundRecords.Succeeded)
            {
                return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(foundRecords.ErrorMessage);
            }

            var convertedFileStorageRecords = foundRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<FileStorageRecordSlim>>.SuccessAsync(convertedFileStorageRecords);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        try
        {
            var recordCount = await _recordRepository.GetCountAsync();
            if (!recordCount.Succeeded)
            {
                return await Result<int>.FailAsync(recordCount.ErrorMessage);
            }

            return await Result<int>.SuccessAsync(recordCount.Result);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<FileStorageRecordSlim?>> GetByIdAsync(Guid id)
    {
        try
        {
            var foundRecord = await _recordRepository.GetByIdAsync(id);
            if (!foundRecord.Succeeded)
            {
                return await Result<FileStorageRecordSlim?>.FailAsync(foundRecord.ErrorMessage);
            }

            var convertedFileStorageRecord = foundRecord.Result?.ToSlim();

            return await Result<FileStorageRecordSlim?>.SuccessAsync(convertedFileStorageRecord);
        }
        catch (Exception ex)
        {
            return await Result<FileStorageRecordSlim?>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetByLinkedIdAsync(Guid id)
    {
        try
        {
            var foundRecords = await _recordRepository.GetByLinkedIdAsync(id);
            if (!foundRecords.Succeeded)
            {
                return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(foundRecords.ErrorMessage);
            }

            var convertedFileStorageRecord = foundRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<FileStorageRecordSlim>>.SuccessAsync(convertedFileStorageRecord);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetByLinkedTypeAsync(FileStorageType type)
    {
        try
        {
            var foundRecords = await _recordRepository.GetByLinkedTypeAsync(type);
            if (!foundRecords.Succeeded)
            {
                return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(foundRecords.ErrorMessage);
            }

            var convertedFileStorageRecord = foundRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<FileStorageRecordSlim>>.SuccessAsync(convertedFileStorageRecord);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<Guid>> CreateAsync(FileStorageRecordCreateRequest request, Guid requestUserId)
    {
        try
        {
            var convertedRecord = request.ToCreate();
            convertedRecord.CreatedBy = requestUserId;
            convertedRecord.CreatedOn = _dateTime.NowDatabaseTime;
            
            var recordCreate = await _recordRepository.CreateAsync(convertedRecord);
            if (!recordCreate.Succeeded)
            {
                return await Result<Guid>.FailAsync(recordCreate.ErrorMessage);
            }

            return await Result<Guid>.SuccessAsync(recordCreate.Result);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid requestUserId)
    {
        try
        {
            var recordDelete = await _recordRepository.DeleteAsync(id, requestUserId);
            if (!recordDelete.Succeeded)
            {
                return await Result<Guid>.FailAsync(recordDelete.ErrorMessage);
            }

            return await Result<Guid>.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<FileStorageRecordSlim>>> SearchAsync(string searchText)
    {
        try
        {
            var fileStorageRecords = await _recordRepository.SearchAsync(searchText);
            if (!fileStorageRecords.Succeeded)
            {
                return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(fileStorageRecords.ErrorMessage);
            }

            var convertedFileStorageRecord = fileStorageRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<FileStorageRecordSlim>>.SuccessAsync(convertedFileStorageRecord);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult<IEnumerable<FileStorageRecordSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        try
        {
            var fileStorageRecords = await _recordRepository.SearchPaginatedAsync(searchText, pageNumber, pageSize);
            if (!fileStorageRecords.Succeeded)
            {
                return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(fileStorageRecords.ErrorMessage);
            }

            var convertedFileStorageRecord = fileStorageRecords.Result?.ToSlims().ToList() ?? [];

            return await Result<IEnumerable<FileStorageRecordSlim>>.SuccessAsync(convertedFileStorageRecord);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<FileStorageRecordSlim>>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> UpdateAsync(FileStorageRecordUpdateRequest request, Guid requestUserId)
    {
        try
        {
            var convertedRecord = request.ToUpdate();
            convertedRecord.LastModifiedBy = requestUserId;
            convertedRecord.LastModifiedOn = _dateTime.NowDatabaseTime;
            
            var updateRecord = await _recordRepository.UpdateAsync(convertedRecord);
            if (!updateRecord.Succeeded)
            {
                return await Result<Guid>.FailAsync(updateRecord.ErrorMessage);
            }

            return await Result<Guid>.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }
}