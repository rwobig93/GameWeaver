using Application.Models.Lifecycle;
using Domain.DatabaseEntities.Lifecycle;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;

namespace Application.Repositories.Lifecycle;

public interface IAuditTrailsRepository
{
    Task<DatabaseActionResult<IEnumerable<AuditTrailDb>>> GetAllAsync();
    Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> GetAllWithUsersAsync();
    Task<DatabaseActionResult<IEnumerable<AuditTrailDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> GetAllPaginatedWithUsersAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<AuditTrailDb>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<AuditTrailWithUserDb>> GetByIdWithUserAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> GetByChangedByIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> GetByRecordIdAsync(Guid id);
    Task<DatabaseActionResult<Guid>> CreateAsync(AuditTrailCreate createObject);
    Task<DatabaseActionResult<IEnumerable<AuditTrailDb>>> SearchAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<AuditTrailDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> SearchPaginatedWithUserAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<AuditTrailWithUserDb>>> SearchWithUserAsync(string searchText);
    Task<DatabaseActionResult<int>> DeleteOld(CleanupTimeframe olderThan);
}