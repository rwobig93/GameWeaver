using Application.Models;
using Application.Models.GameServer;
using Domain.Contracts;
using Domain.Models.ControlServer;

namespace Application.Repositories;

public interface IWeaverWorkRepository
{
    Task<IResult> CreateAsync(WeaverWork work);
    Task<IResult> UpdateAsync(WeaverWorkUpdate workUpdate);
    Task<IResult> DeleteAsync(int id);
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
}