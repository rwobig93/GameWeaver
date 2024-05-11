using Application.Constants;
using Application.Models;
using Application.Repositories;
using Application.Services;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models.ControlServer;

namespace Infrastructure.Repositories;

public class WeaverWorkRepositoryMemoryJson : IWeaverWorkRepository
{
    private readonly ISerializerService _serializerService;
    private readonly ILogger _logger;

    public WeaverWorkRepositoryMemoryJson(ISerializerService serializerService, ILogger logger)
    {
        _serializerService = serializerService;
        _logger = logger;

        Task.Run(LoadAsync);
    }

    private static List<WeaverWork> _weaverWork = [];

    public async Task<IResult> CreateAsync(WeaverWork work)
    {
        var matchingWeaverWork = _weaverWork.FirstOrDefault(x => x.Id == work.Id);
        if (matchingWeaverWork is not null)
            return await Result.FailAsync($"Weaver work with Id [{work.Id}] already exists");
        
        _weaverWork.Add(work);
        _logger.Debug("Added weaver work: [{WeaverworkId}]{WeaverworkTarget}", work.Id, work.TargetType);
        return await Result.SuccessAsync();
    }

    public async Task<IResult> UpdateAsync(WeaverWorkUpdate workUpdate)
    {
        var matchingWeaverWork = _weaverWork.FirstOrDefault(x => x.Id == workUpdate.Id);
        if (matchingWeaverWork is null)
            return await Result.FailAsync($"Weaver work with Id [{workUpdate.Id}] doesn't exist");

        matchingWeaverWork.Status = workUpdate.Status;
        
        _logger.Debug("Updated weaver work: [{WeaverworkId}]{WeaverworkTarget}", matchingWeaverWork.Id, matchingWeaverWork.TargetType);
        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteAsync(int id)
    {
        var matchingWeaverWork = _weaverWork.FirstOrDefault(x => x.Id == id);
        if (matchingWeaverWork is null)
            return await Result.FailAsync($"Weaver work with Id [{id}] doesn't exist");

        _weaverWork.Remove(matchingWeaverWork);
        _logger.Debug("Deleted weaver work: [{WeaverworkId}]{WeaverworkTarget}", matchingWeaverWork.Id, matchingWeaverWork.TargetType);
        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteCompletedAsync()
    {
        try
        {
            var removedCount = _weaverWork.RemoveAll(x => x.Status is WeaverWorkState.Completed or
                WeaverWorkState.Failed or
                WeaverWorkState.Cancelled);
        
            _logger.Information("Removed {WeaverworkCount} units of completed weaver work from the repository", removedCount);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred removing completed work from weaver work repository: {Error}", ex.Message);
            return await Result.FailAsync($"Failure occurred removing completed work from weaver work repository: {ex.Message}");
        }
    }

    public async Task<IResult> SaveAsync()
    {
        try
        {
            var serializedWorkQueue = _serializerService.SerializeJson(_weaverWork);
            await File.WriteAllTextAsync(SerializerConstants.WeaverWorkPath, serializedWorkQueue);
            _logger.Information("Serialized weaver work file: {FilePath}", SerializerConstants.WeaverWorkPath);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred serializing weaver work repository to json: {Error}", ex.Message);
            return await Result.FailAsync($"Failure occurred serializing weaver work repository to json: {ex.Message}");
        }
    }

    public async Task<IResult> LoadAsync()
    {
        if (!File.Exists(SerializerConstants.WeaverWorkPath))
        {
            _logger.Debug("Weaver work queue file doesn't exist, creating... [{FilePath}]", SerializerConstants.WeaverWorkPath);
            await SaveAsync();
        }

        try
        {
            var weaverWorkQueue = await File.ReadAllTextAsync(SerializerConstants.WeaverWorkPath);
            _weaverWork = _serializerService.DeserializeJson<List<WeaverWork>>(weaverWorkQueue);
            _logger.Information("Deserialized weaver work queue: {FilePath}", SerializerConstants.WeaverWorkPath);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred deserializing weaver work repository from json: {Error}", ex.Message);
            return await Result.FailAsync($"Failure occurred deserializing weaver work repository from json: {ex.Message}");
        }
    }

    public async Task<IResult<WeaverWork?>> GetByIdAsync(int id)
    {
        var matchingWeaverWork = _weaverWork.FirstOrDefault(x => x.Id == id);
        if (matchingWeaverWork is null)
            return await Result<WeaverWork?>.FailAsync($"Weaver work with Id [{id}] doesn't exist");

        return await Result<WeaverWork?>.SuccessAsync(matchingWeaverWork);
    }

    public async Task<IResult<IEnumerable<WeaverWork>>> GetInProgressHostAsync()
    {
        try
        {
            var hostInProgressWork = _weaverWork.Where(x => x.TargetType is >= WeaverWorkTarget.Host and < WeaverWorkTarget.GameServer
                                                            && x.Status == WeaverWorkState.InProgress);
            return await Result<IEnumerable<WeaverWork>>.SuccessAsync(hostInProgressWork.ToList());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<WeaverWork>>.FailAsync($"Failure occurred getting host in progress work: {ex.Message}");
        }
    }

    public async Task<IResult<IEnumerable<WeaverWork>>> GetInProgressGameserverAsync()
    {
        try
        {
            var gameserverInProgressWork = _weaverWork.Where(x => x.TargetType is >= WeaverWorkTarget.GameServer and < WeaverWorkTarget.CurrentEnd
                                                            && x.Status == WeaverWorkState.InProgress);
            return await Result<IEnumerable<WeaverWork>>.SuccessAsync(gameserverInProgressWork.ToList());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<WeaverWork>>.FailAsync($"Failure occurred getting gameserver in progress work: {ex.Message}");
        }
    }

    public async Task<IResult<WeaverWork?>> GetNextWaitingHostAsync()
    {
        try
        {
            var nextHostWork = _weaverWork.FirstOrDefault(x => x.TargetType is >= WeaverWorkTarget.Host and < WeaverWorkTarget.GameServer
                                                               && x.Status == WeaverWorkState.WaitingToBePickedUp);
            if (nextHostWork is null)
                return await Result<WeaverWork?>.SuccessAsync(null, $"No host work is currently waiting to be picked up");

            return await Result<WeaverWork?>.SuccessAsync(nextHostWork);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting next host work: {Error}", ex.Message);
            return await Result<WeaverWork?>.FailAsync($"Failure occurred getting next host work: {ex.Message}");
        }
    }

    public async Task<IResult<WeaverWork?>> GetNextWaitingGameserverAsync()
    {
        try
        {
            var nextGameserverWork = _weaverWork.FirstOrDefault(x => x.TargetType is >= WeaverWorkTarget.GameServer and < WeaverWorkTarget.CurrentEnd
                                                               && x.Status == WeaverWorkState.WaitingToBePickedUp);
            if (nextGameserverWork is null)
                return await Result<WeaverWork?>.SuccessAsync(null, $"No gameserver work is currently waiting to be picked up");

            return await Result<WeaverWork?>.SuccessAsync(nextGameserverWork);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting next gameserver work: {Error}", ex.Message);
            return await Result<WeaverWork?>.FailAsync($"Failure occurred getting next gameserver work: {ex.Message}");
        }
    }

    public async Task<IResult<int>> GetCountAllAsync()
    {
        try
        {
            var count = _weaverWork.Count;
            return await Result<int>.SuccessAsync(count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting work count: {Error}", ex.Message);
            return await Result<int>.FailAsync($"Failure occurred getting work count: {ex.Message}");
        }
    }

    public async Task<IResult<int>> GetCountInProgressAsync()
    {
        try
        {
            var count = _weaverWork.Count(x => x.Status == WeaverWorkState.InProgress);
            return await Result<int>.SuccessAsync(count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting work count: {Error}", ex.Message);
            return await Result<int>.FailAsync($"Failure occurred getting work count: {ex.Message}");
        }
    }

    public async Task<IResult<int>> GetCountWaitingAsync()
    {
        try
        {
            var count = _weaverWork.Count(x => x.Status == WeaverWorkState.WaitingToBePickedUp);
            return await Result<int>.SuccessAsync(count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting work count: {Error}", ex.Message);
            return await Result<int>.FailAsync($"Failure occurred getting work count: {ex.Message}");
        }
    }

    public async Task<IResult<int>> GetCountAllHostAsync()
    {
        try
        {
            var count = _weaverWork.Count(x => x.TargetType is >= WeaverWorkTarget.Host and < WeaverWorkTarget.GameServer);
            return await Result<int>.SuccessAsync(count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting work count: {Error}", ex.Message);
            return await Result<int>.FailAsync($"Failure occurred getting work count: {ex.Message}");
        }
    }

    public async Task<IResult<int>> GetCountInProgressHostAsync()
    {
        try
        {
            var count = _weaverWork.Count(x => x.TargetType is >= WeaverWorkTarget.Host and < WeaverWorkTarget.GameServer
                                               && x.Status == WeaverWorkState.InProgress);
            return await Result<int>.SuccessAsync(count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting work count: {Error}", ex.Message);
            return await Result<int>.FailAsync($"Failure occurred getting work count: {ex.Message}");
        }
    }

    public async Task<IResult<int>> GetCountWaitingHostAsync()
    {
        try
        {
            var count = _weaverWork.Count(x => x.TargetType is >= WeaverWorkTarget.Host and < WeaverWorkTarget.GameServer
                                               && x.Status == WeaverWorkState.WaitingToBePickedUp);
            return await Result<int>.SuccessAsync(count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting work count: {Error}", ex.Message);
            return await Result<int>.FailAsync($"Failure occurred getting work count: {ex.Message}");
        }
    }

    public async Task<IResult<int>> GetCountAllGameserverAsync()
    {
        try
        {
            var count = _weaverWork.Count(x => x.TargetType is >= WeaverWorkTarget.GameServer and < WeaverWorkTarget.CurrentEnd);
            return await Result<int>.SuccessAsync(count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting work count: {Error}", ex.Message);
            return await Result<int>.FailAsync($"Failure occurred getting work count: {ex.Message}");
        }
    }

    public async Task<IResult<int>> GetCountInProgressGameserverAsync()
    {
        try
        {
            var count = _weaverWork.Count(x => x.TargetType is >= WeaverWorkTarget.GameServer and < WeaverWorkTarget.CurrentEnd
                                               && x.Status == WeaverWorkState.InProgress);
            return await Result<int>.SuccessAsync(count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting work count: {Error}", ex.Message);
            return await Result<int>.FailAsync($"Failure occurred getting work count: {ex.Message}");
        }
    }

    public async Task<IResult<int>> GetCountWaitingGameserverAsync()
    {
        try
        {
            var count = _weaverWork.Count(x => x.TargetType is >= WeaverWorkTarget.GameServer and < WeaverWorkTarget.CurrentEnd
                                               && x.Status == WeaverWorkState.WaitingToBePickedUp);
            return await Result<int>.SuccessAsync(count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred getting work count: {Error}", ex.Message);
            return await Result<int>.FailAsync($"Failure occurred getting work count: {ex.Message}");
        }
    }
}