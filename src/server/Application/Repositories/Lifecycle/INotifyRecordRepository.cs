using Application.Models.Lifecycle;
using Domain.Contracts;
using Domain.DatabaseEntities.Lifecycle;
using Domain.Models.Database;

namespace Application.Repositories.Lifecycle;

public interface INotifyRecordRepository
{
    Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> GetAllAsync();
    Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<NotifyRecordDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<NotifyRecordDb?>> GetByIdAsync(int id);
    Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> GetByRecordIdAsync(Guid recordId, int recordCount);
    Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> GetAllByRecordIdAsync(Guid recordId);
    Task<DatabaseActionResult<int>> CreateAsync(NotifyRecordCreate createObject);
    Task<DatabaseActionResult<IEnumerable<NotifyRecordDb>>> SearchAsync(string searchTerm);
    Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<NotifyRecordDb>>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> DeleteOlderThan(DateTime olderThan);
    Task<DatabaseActionResult<int>> DeleteAllForRecordId(Guid recordId);
}