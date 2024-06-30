using Application.Models.Integrations;
using Domain.DatabaseEntities.Integrations;
using Domain.Enums.Integrations;
using Domain.Models.Database;

namespace Application.Repositories.Integrations;

public interface IFileStorageRecordRepository
{
    Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> GetAllAsync();
    Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<FileStorageRecordDb?>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> GetByFormatAsync(FileStorageFormat format);
    Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> GetByLinkedIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> GetByLinkedTypeAsync(FileStorageType type);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<Guid>> CreateAsync(FileStorageRecordCreate request);
    Task<DatabaseActionResult> DeleteAsync(Guid id, Guid requestUserId);
    Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> SearchAsync(string searchTerm);
    Task<DatabaseActionResult<IEnumerable<FileStorageRecordDb>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize);
    Task<DatabaseActionResult> UpdateAsync(FileStorageRecordUpdate request);
}