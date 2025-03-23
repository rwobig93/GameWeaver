using Application.Models.Integrations;
using Application.Requests.Integrations;
using Domain.Contracts;
using Domain.Enums.Integrations;

namespace Application.Services.Integrations;

public interface IFileStorageRecordService
{

    Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetAllAsync();
    Task<PaginatedResult<IEnumerable<FileStorageRecordSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<FileStorageRecordSlim?>> GetByIdAsync(Guid id);
    Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetByFormatAsync(FileStorageFormat format);
    Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetByLinkedIdAsync(Guid id);
    Task<IResult<IEnumerable<FileStorageRecordSlim>>> GetByLinkedTypeAsync(FileStorageType type);
    Task<IResult<int>> GetCountAsync();
    Task<IResult<Guid>> CreateAsync(FileStorageRecordCreateRequest request, Stream content, Guid requestUserId);
    Task<IResult> DeleteAsync(Guid id, Guid requestUserId);
    Task<IResult<IEnumerable<FileStorageRecordSlim>>> SearchAsync(string searchTerm);
    Task<PaginatedResult<IEnumerable<FileStorageRecordSlim>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize);
    Task<IResult> UpdateAsync(FileStorageRecordUpdateRequest request, Guid requestUserId);
}