using Domain.Contracts;
using Domain.Enums;
using Domain.Models.ControlServer;

namespace Application.Services;

public interface IWeaverWorkService
{
    Task<IResult> CreateAsync(WeaverWork work);
    Task<IResult> UpdateStatusAsync(int id, WeaverWorkState status);
    Task<IResult> DeleteCompletedAsync();
    Task<IResult> SaveAsync();
    Task<IResult> LoadAsync();
    Task<IResult<WeaverWork?>> GetByIdAsync(int id);
    Task<IResult<IEnumerable<WeaverWork>>> GetInProgressHostAsync();
    Task<IResult<IEnumerable<WeaverWork>>> GetInProgressGameserverAsync();
    Task<IResult<WeaverWork?>> GetNextWaitingHostAsync();
    Task<IResult<WeaverWork?>> GetNextWaitingGameserverAsync();
    Task<IResult<int>> GetCountAllAsync();
    Task<IResult<int>> GetCountInProgressAsync();
    Task<IResult<int>> GetCountWaitingAsync();
    Task<IResult<int>> GetCountAllHostAsync();
    Task<IResult<int>> GetCountInProgressHostAsync();
    Task<IResult<int>> GetCountWaitingHostAsync();
    Task<IResult<int>> GetCountAllGameserverAsync();
    Task<IResult<int>> GetCountInProgressGameserverAsync();
    Task<IResult<int>> GetCountWaitingGameserverAsync();
    Task<IResult> Housekeeping();
}