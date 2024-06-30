using Application.Models.GameServer;
using Application.Repositories;
using Application.Services;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models.ControlServer;

namespace Infrastructure.Services;

public class WeaverWorkService : IWeaverWorkService
{
    private readonly IWeaverWorkRepository _weaverWorkRepository;

    public WeaverWorkService(IWeaverWorkRepository weaverWorkRepository)
    {
        _weaverWorkRepository = weaverWorkRepository;
    }

    public async Task<IResult> CreateAsync(WeaverWork work)
    {
        return await _weaverWorkRepository.CreateAsync(work);
    }

    public async Task<IResult> UpdateStatusAsync(int id, WeaverWorkState status)
    {
        var workUpdate = new WeaverWorkUpdate {Id = id, Status = status};
        var updateRequest = await _weaverWorkRepository.UpdateAsync(workUpdate);
        if (!updateRequest.Succeeded)
            return await Result.FailAsync(updateRequest.Messages);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteCompletedAsync()
    {
        return await _weaverWorkRepository.DeleteCompletedAsync();
    }

    public async Task<IResult> SaveAsync()
    {
        return await _weaverWorkRepository.SaveAsync();
    }

    public async Task<IResult> LoadAsync()
    {
        return await _weaverWorkRepository.LoadAsync();
    }

    public async Task<IResult<WeaverWork?>> GetByIdAsync(int id)
    {
        return await _weaverWorkRepository.GetByIdAsync(id);
    }

    public async Task<IResult<IEnumerable<WeaverWork>>> GetInProgressHostAsync()
    {
        return await _weaverWorkRepository.GetInProgressHostAsync();
    }

    public async Task<IResult<IEnumerable<WeaverWork>>> GetInProgressGameserverAsync()
    {
        return await _weaverWorkRepository.GetInProgressGameserverAsync();
    }

    public async Task<IResult<WeaverWork?>> GetNextWaitingHostAsync()
    {
        return await _weaverWorkRepository.GetNextWaitingHostAsync();
    }

    public async Task<IResult<WeaverWork?>> GetNextWaitingGameserverAsync()
    {
        return await _weaverWorkRepository.GetNextWaitingGameserverAsync();
    }

    public async Task<IResult<int>> GetCountAllAsync()
    {
        return await _weaverWorkRepository.GetCountAllAsync();
    }

    public async Task<IResult<int>> GetCountInProgressAsync()
    {
        return await _weaverWorkRepository.GetCountInProgressAsync();
    }

    public async Task<IResult<int>> GetCountWaitingAsync()
    {
        return await _weaverWorkRepository.GetCountWaitingAsync();
    }

    public async Task<IResult<int>> GetCountAllHostAsync()
    {
        return await _weaverWorkRepository.GetCountAllHostAsync();
    }

    public async Task<IResult<int>> GetCountInProgressHostAsync()
    {
        return await _weaverWorkRepository.GetCountInProgressHostAsync();
    }

    public async Task<IResult<int>> GetCountWaitingHostAsync()
    {
        return await _weaverWorkRepository.GetCountWaitingHostAsync();
    }

    public async Task<IResult<int>> GetCountAllGameserverAsync()
    {
        return await _weaverWorkRepository.GetCountAllGameserverAsync();
    }

    public async Task<IResult<int>> GetCountInProgressGameserverAsync()
    {
        return await _weaverWorkRepository.GetCountInProgressGameserverAsync();
    }

    public async Task<IResult<int>> GetCountWaitingGameserverAsync()
    {
        return await _weaverWorkRepository.GetCountWaitingGameserverAsync();
    }

    public async Task<IResult> Housekeeping()
    {
        return await _weaverWorkRepository.SaveAsync();
    }
}