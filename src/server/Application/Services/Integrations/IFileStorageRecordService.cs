using Application.Models.System;
using Application.Requests.Integrations;
using Domain.Contracts;
using Domain.Enums.Integrations;

namespace Application.Services.Integrations;

public interface IFileStorageRecordService
{

    Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetAllAsync();
    Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<FileStorageRecordSlim?>> GetByIdAsync(Guid id);
    Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetByLinkedIdAsync(Guid id);
    Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetByLinkedTypeAsync(FileStorageType type);
    Task<IResult<int>> GetCountAsync();
    Task<IResult<Guid>> CreateAsync(FileStorageRecordCreateRequest request, Guid requestUserId);
    Task<IResult> DeleteAsync(Guid id, Guid requestUserId);
    Task<IResult<IEnumerable<FileStorageRecordSlim>>> SearchAsync(string searchTerm);
    Task<IResult<IEnumerable<FileStorageRecordSlim>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize);
    Task<IResult> UpdateAsync(FileStorageRecordUpdateRequest request, Guid requestUserId);
}