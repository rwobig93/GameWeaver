using Application.Models.Lifecycle;
using Domain.Contracts;

namespace Application.Services.Lifecycle;

public interface INotifyRecordService
{
    Task<IResult<IEnumerable<NotifyRecordSlim>>> GetAllAsync();
    Task<PaginatedResult<IEnumerable<NotifyRecordSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<NotifyRecordSlim?>> GetByIdAsync(int id);
    Task<IResult<IEnumerable<NotifyRecordSlim>>> GetByEntityIdAsync(Guid id, int recordCount);
    Task<IResult<IEnumerable<NotifyRecordSlim>>> GetAllByEntityIdAsync(Guid id);
    Task<IResult<int>> CreateAsync(NotifyRecordCreate createObject);
    Task<IResult<IEnumerable<NotifyRecordSlim>>> SearchAsync(string searchTerm);
    Task<PaginatedResult<IEnumerable<NotifyRecordSlim>>> SearchPaginatedAsync(string searchTerm, int pageNumber, int pageSize);
    Task<PaginatedResult<IEnumerable<NotifyRecordSlim>>> SearchPaginatedByEntityIdAsync(Guid id, string searchTerm, int pageNumber, int pageSize);
    Task<IResult<int>> DeleteOlderThan(DateTime olderThan);
    Task<IResult<int>> DeleteAllForEntityId(Guid id);
}