using Application.Constants.Communication;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.Integrations;
using Application.Models.Integrations;
using Application.Repositories.Integrations;
using Application.Repositories.Lifecycle;
using Application.Requests.Integrations;
using Application.Services.Integrations;
using Application.Services.Lifecycle;
using Application.Services.System;
using Domain.Contracts;
using Domain.Enums.Integrations;
using Domain.Enums.Lifecycle;

namespace Infrastructure.Services.Integrations;

public class FileStorageRecordService : IFileStorageRecordService
{
    private readonly IFileStorageRecordRepository _recordRepository;
    private readonly IDateTimeService _dateTime;
    private readonly IRunningServerState _serverState;
    private readonly ITroubleshootingRecordsRepository _tshootRepository;

    public FileStorageRecordService(IFileStorageRecordRepository recordRepository, IDateTimeService dateTime, IRunningServerState serverState,
        ITroubleshootingRecordsRepository tshootRepository)
    {
        _recordRepository = recordRepository;
        _dateTime = dateTime;
        _serverState = serverState;
        _tshootRepository = tshootRepository;
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

    public async Task<IResult<Guid>> CreateAsync(FileStorageRecordCreateRequest request, Stream content, Guid requestUserId)
    {
        try
        {
            var convertedRecord = request.ToCreate();
            convertedRecord.CreatedBy = requestUserId;
            convertedRecord.CreatedOn = _dateTime.NowDatabaseTime;
            convertedRecord.HashSha256 = "";
            
            var recordCreate = await _recordRepository.CreateAsync(convertedRecord);
            if (!recordCreate.Succeeded)
            {
                var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.FileStorage, Guid.Empty, requestUserId,
                    "Failed to create file record for new file upload", new Dictionary<string, string>
                    {
                        {"FriendlyName", request.FriendlyName},
                        {"Description", request.Description},
                        {"Filename", request.Filename},
                        {"Version", request.Version},
                        {"LinkedId", request.LinkedId.ToString()},
                        {"LinkedType", request.LinkedType.ToString()},
                        {"Error", recordCreate.ErrorMessage}
                    });
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
            }

            var createdRecord = await _recordRepository.GetByIdAsync(recordCreate.Result);
            var filePath = createdRecord.Result!.GetLocalFilePath();
            try
            {
                Directory.CreateDirectory(new FileInfo(filePath).DirectoryName!);
                await using (var stream = new FileStream(createdRecord.Result!.GetLocalFilePath(), FileMode.Create))
                {
                    await content.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                await _recordRepository.DeleteAsync(recordCreate.Result, _serverState.SystemUserId);
                var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.FileStorage, recordCreate.Result, requestUserId,
                    "Failed to upload and save new file", new Dictionary<string, string>
                    {
                        {"CreatedAndDeletedRecordId", recordCreate.Result.ToString()},
                        {"FriendlyName", request.FriendlyName},
                        {"Description", request.Description},
                        {"Filename", request.Filename},
                        {"Version", request.Version},
                        {"LinkedId", request.LinkedId.ToString()},
                        {"LinkedType", request.LinkedType.ToString()},
                        {"Error", ex.Message}
                    });
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
            }

            var fileHash = FileHelpers.ComputeSha256Hash(filePath);
            var recordUpdate = await _recordRepository.UpdateAsync(new FileStorageRecordUpdate() {Id = recordCreate.Result, HashSha256 = fileHash});
            if (recordUpdate.Succeeded) return await Result<Guid>.SuccessAsync(recordCreate.Result);
            {
                var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.FileStorage, recordCreate.Result, requestUserId,
                    "Failed to update new file hash", new Dictionary<string, string>
                    {
                        {"HashSha256", fileHash ?? ""},
                        {"Error", recordUpdate.ErrorMessage}
                    });
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
            }
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
            var foundRecord = await _recordRepository.GetByIdAsync(id);
            if (foundRecord.Result is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.FileStorage.NotFound);
            }

            try
            {
                var localFilePath = foundRecord.Result.GetLocalFilePath();
                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                }
            }
            catch (Exception ex)
            {
                var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.FileStorage, foundRecord.Result.Id, requestUserId,
                    "Failed to delete local file for file record", new Dictionary<string, string>
                    {
                        {"Error", ex.Message}
                    });
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
            }
            
            var recordDelete = await _recordRepository.DeleteAsync(id, requestUserId);
            if (recordDelete.Succeeded) return await Result<Guid>.SuccessAsync();
            {
                var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.FileStorage, foundRecord.Result.Id, requestUserId,
                    "Failed to delete file record after deleting local file", new Dictionary<string, string>
                    {
                        {"Error", recordDelete.ErrorMessage}
                    });
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
            }
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