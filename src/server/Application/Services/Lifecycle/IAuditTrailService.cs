using Application.Models.Lifecycle;
using Domain.Contracts;
using Domain.Enums.Lifecycle;

namespace Application.Services.Lifecycle;

public interface IAuditTrailService
{
    Task<IResult<IEnumerable<AuditTrailSlim>>> GetAllAsync();
    Task<IResult<IEnumerable<AuditTrailSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetCountAsync();
    Task<IResult<AuditTrailSlim?>> GetByIdAsync(Guid id);
    Task<IResult<IEnumerable<AuditTrailSlim>>> GetByChangedByIdAsync(Guid id);
    Task<IResult<IEnumerable<AuditTrailSlim>>> GetByRecordIdAsync(Guid id);
    Task<IResult<Guid>> CreateAsync(AuditTrailCreate createObject);
    Task<IResult<IEnumerable<AuditTrailSlim>>> SearchAsync(string searchText);
    Task<IResult<IEnumerable<AuditTrailSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<IResult<int>> DeleteOld(CleanupTimeframe olderThan);
}