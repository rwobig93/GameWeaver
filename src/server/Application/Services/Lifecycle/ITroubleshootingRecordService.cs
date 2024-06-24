using Application.Models.Lifecycle;
using Domain.Contracts;
using Domain.Enums.Lifecycle;

namespace Application.Services.Lifecycle;

public interface ITroubleshootingRecordService
{
    Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetAllAsync();
    Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetCountAsync();
    Task<IResult<TroubleshootingRecordSlim?>> GetByIdAsync(Guid id);
    Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetByChangedByIdAsync(Guid id);
    Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetByEntityTypeAsync(TroubleshootEntityType type);
    Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> GetByRecordIdAsync(Guid id);
    Task<IResult<Guid>> CreateAsync(TroubleshootingRecordCreate createObject);
    Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> SearchAsync(string searchText);
    Task<IResult<IEnumerable<TroubleshootingRecordSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<IResult<int>> DeleteOlderThan(CleanupTimeframe olderThan);
}