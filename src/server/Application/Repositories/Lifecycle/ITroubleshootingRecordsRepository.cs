using Application.Models.Lifecycle;
using Domain.DatabaseEntities.Lifecycle;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;

namespace Application.Repositories.Lifecycle;

public interface ITroubleshootingRecordsRepository
{
    Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetAllAsync();
    Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<TroubleshootingRecordDb?>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetByChangedByIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetByEntityTypeAsync(TroubleshootEntityType type);
    Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> GetByRecordIdAsync(Guid id);
    Task<DatabaseActionResult<Guid>> CreateAsync(TroubleshootingRecordCreate createObject);
    Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> SearchAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<TroubleshootingRecordDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> DeleteOlderThan(CleanupTimeframe olderThan);
}