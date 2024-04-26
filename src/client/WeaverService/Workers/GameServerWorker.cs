using System.Collections.Concurrent;
using Application.Constants;
using Application.Helpers;
using Application.Mappers;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models.ControlServer;
using Domain.Models.GameServer;
using Microsoft.Extensions.Options;
using Serilog;
using WeaverService.Helpers;

namespace WeaverService.Workers;

public class GameServerWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IGameServerService _gameServerService;
    private readonly IOptions<GeneralConfiguration> _generalConfig;
    private readonly ISerializerService _serializerService;

    private static DateTime _lastRuntime;
    private static DateTime? _lastBackupTime;
    private static ConcurrentQueue<WeaverWork> _workQueue = new();
    private static int _inProgressWorkCount;

    /// <summary>
    /// Handles Gameserver work including updates, configuration and per server IO
    /// </summary>
    public GameServerWorker(ILogger logger, IDateTimeService dateTimeService, IGameServerService gameServerService, IOptions<GeneralConfiguration> generalConfig,
        ISerializerService serializerService)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
        _gameServerService = gameServerService;
        _generalConfig = generalConfig;
        _serializerService = serializerService;
    }

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(GameServerWorker));
        
        // TODO: Implement Sqlite for game server state tracking, for now we'll serialize/deserialize a json file
        await DeserializeWorkQueues();
        await base.StartAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _lastRuntime = _dateTimeService.NowDatabaseTime;
                
                await ProcessWorkQueue();
                await BackupGameServers();

                var millisecondsPassed = (_dateTimeService.NowDatabaseTime - _lastRuntime).Milliseconds;
                if (millisecondsPassed < _generalConfig.Value.GameServerWorkIntervalMs)
                    await Task.Delay(_generalConfig.Value.GameServerWorkIntervalMs - millisecondsPassed, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure occurred during {ServiceName} execution loop: {ErrorMessage}",
                    nameof(GameServerWorker), ex.Message);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Stopping {ServiceName} service", nameof(GameServerWorker));

        await _gameServerService.Housekeeping();
        await SerializeWorkQueues();
        
        _logger.Debug("Stopped {ServiceName} service", nameof(GameServerWorker));
        await base.StopAsync(stoppingToken);
    }

    private async Task ProcessWorkQueue()
    {
        if (_inProgressWorkCount >= _generalConfig.Value.SimultaneousQueueWorkCountMax)
        {
            _logger.Verbose("In progress gameserver work [{InProgressWork}] is at max [{MaxWork}], moving on | Waiting: {WaitingWork}", _inProgressWorkCount,
                _generalConfig.Value.SimultaneousQueueWorkCountMax, _workQueue.Count);
            return;
        }
        
        if (_workQueue.IsEmpty)
        {
            _logger.Verbose("No work waiting, moving on");
            return;
        }
        
        // Work is waiting, and we have available queue space, adding next work to the thread pool
        var attemptCount = 0;
        while (_inProgressWorkCount < _generalConfig.Value.SimultaneousQueueWorkCountMax)
        {
            if (_workQueue.IsEmpty) break;
            
            if (attemptCount >= 5)
            {
                _logger.Error("Unable to dequeue next item in the gameserver work queue, quiting cycle queue processing");
                return;
            }
            
            if (!_workQueue.TryDequeue(out var work))
            {
                attemptCount++;
                continue;
            }
            
            _inProgressWorkCount++;
            work.Status = WeaverWorkState.InProgress;
            // ReSharper disable once AsyncVoidLambda
            ThreadHelper.QueueWork(async _ => await HandleWork(work));
        }

        await Task.CompletedTask;
    }

    private async Task BackupGameServers()
    {
        _lastBackupTime ??= _dateTimeService.NowDatabaseTime;

        if (_dateTimeService.NowDatabaseTime < _lastBackupTime.Value.AddMinutes(_generalConfig.Value.GameserverBackupIntervalMinutes)) return;

        var allGameServers = await _gameServerService.GetAll();
        
        foreach (var gameserver in allGameServers.Data)
            await _gameServerService.BackupGame(gameserver.Id);
        
        _lastBackupTime = _dateTimeService.NowDatabaseTime;
    }

    public static void AddWorkToQueue(WeaverWork work)
    {
        Log.Debug("Adding gameserver work to waiting queue: {Id} |{WorkType} | {Status}", work.Id, work.TargetType, work.Status);
        
        // TODO: Need to check against work in progress as well, currently we aren't tracking that work, we need to for reliability
        if (_workQueue.Any(x => x.Id == work.Id))
        {
            Log.Verbose("Work already exists in the queue, skipping duplicate: [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            return;
        }

        work.Status = WeaverWorkState.PickedUp;
        _workQueue.Enqueue(work);
        work.SendStatusUpdate(WeaverWorkState.PickedUp, "Work has been picked up");
        
        Log.Debug("Added gameserver work to queue:{Id} | {WorkType} | {Status}", work.Id, work.TargetType, work.Status);
    }

    private async Task SerializeWorkQueues()
    {
        var serializedWorkQueue = _serializerService.SerializeJson(_workQueue);
        await File.WriteAllTextAsync(GameServerConstants.WorkQueuePath, serializedWorkQueue);
        
        _logger.Information("Serialized gameserver work queue file: {FilePath}", GameServerConstants.WorkQueuePath);
    }

    private async Task DeserializeWorkQueues()
    {
        if (!File.Exists(GameServerConstants.WorkQueuePath))
        {
            _logger.Debug("Gameserver work queue file doesn't exist, creating... [{FilePath}]", GameServerConstants.WorkQueuePath);
            await SerializeWorkQueues();
        }

        var workQueue = await File.ReadAllTextAsync(GameServerConstants.WorkQueuePath);
        _workQueue = _serializerService.DeserializeJson<ConcurrentQueue<WeaverWork>>(workQueue);
        
        _logger.Information("Deserialized gameserver work queue");
    }

    private async Task HandleWork(WeaverWork work)
    {
        try
        {
            work.SendStatusUpdate(WeaverWorkState.InProgress, "Work has is now in progress");
            
            switch (work.TargetType)
            {
                case >= WeaverWorkTarget.Host and < WeaverWorkTarget.GameServer:
                    _logger.Error("Host work somehow got into the GameServer work queue: [{WorkId}]{WorkType}", work.Id, work.TargetType);
                    HostWorker.AddWorkToQueue(work);
                    _logger.Warning("Gave host work to host worker since it was in the wrong place: [{WorkId}]{WorkType}", work.Id, work.TargetType);
                    return;
                case >= WeaverWorkTarget.GameServer and < WeaverWorkTarget.CurrentEnd:
                    _logger.Debug("Starting gameserver work from queue: [{WorkId}]{WorkType}", work.Id, work.TargetType);
                    break;
                default:
                    _logger.Error("Invalid work type for work: [{WorkId}]{WorkType}", work.Id, work.TargetType);
                    work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver work data was invalid, please verify the payload");
                    return;
            }

            switch (work.TargetType)
            {
                case WeaverWorkTarget.GameServerInstall:
                    await InstallGameServer(work);
                    break;
                case WeaverWorkTarget.GameServerUpdate:
                    await UpdateGameServer(work);
                    break;
                case WeaverWorkTarget.GameServerUninstall:
                    await UninstallGameServer(work);
                    break;
                case WeaverWorkTarget.GameServerStart:
                    await StartGameServer(work);
                    break;
                case WeaverWorkTarget.GameServerStop:
                    await StopGameServer(work);
                    break;
                case WeaverWorkTarget.GameServerRestart:
                    await RestartGameServer(work);
                    break;
                case WeaverWorkTarget.GameServerStateUpdate:
                    await UpdateGameServerState(work);
                    break;
                case WeaverWorkTarget.StatusUpdate:
                case WeaverWorkTarget.Host:
                case WeaverWorkTarget.HostStatusUpdate:
                case WeaverWorkTarget.HostDetail:
                case WeaverWorkTarget.GameServer:
                case WeaverWorkTarget.CurrentEnd:
                default:
                    _logger.Error(
                        "An impossible event occurred, we hit a switch statement that shouldn't be possible for gameserver work: {WorkType}", work.TargetType);
                    work.SendStatusUpdate(WeaverWorkState.Failed,
                        $"An impossible event occurred, we hit a switch statement that shouldn't be possible for gameserver work: {work.TargetType}");
                    return;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred handling gameserver work: {Error}", ex.Message);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Failure occurred handling gameserver work: {ex.Message}");
        }
        finally
        {
            _inProgressWorkCount--;
        }
    }

    private async Task<IResult<GameServerLocal?>> GetGameServerFromIdWorkData(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver {WorkType} request has an empty work data payload: {WorkId}", work.TargetType, work.Id);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
            return await Result<GameServerLocal?>.FailAsync($"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
        }
        
        var gameServerId = _serializerService.DeserializeMemory<Guid>(work.WorkData);
        var gameServerRequest = await _gameServerService.GetById(gameServerId);
        if (gameServerRequest is {Succeeded: true, Data: not null})
            return await Result<GameServerLocal?>.SuccessAsync(gameServerRequest.Data);

        _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
        work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver id doesn't match an active gameserver this host manages");
        return await Result<GameServerLocal?>.FailAsync("Gameserver id doesn't match an active gameserver this host manages");
    }

    private async Task<IResult<GameServerLocal?>> GetGameServerFromGameServerWorkData(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver {WorkType} request has an empty work data payload: {WorkId}", work.TargetType, work.Id);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
            return await Result<GameServerLocal?>.FailAsync($"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
        }
        
        var deserializedGameServer = _serializerService.DeserializeMemory<GameServerToHost>(work.WorkData);
        if (deserializedGameServer is null)
        {
            _logger.Error("Gameserver wasn't deserializable from work data payload: {WorkId}", work.Id);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver wasn't deserializable from work data payload: {work.Id}");
            return await Result<GameServerLocal?>.FailAsync($"Gameserver wasn't deserializable from work data payload: {work.Id}");
        }

        var gameServerRequest = await _gameServerService.GetById(deserializedGameServer.Id);
        if (gameServerRequest is {Succeeded: true, Data: not null})
            return await Result<GameServerLocal?>.SuccessAsync(gameServerRequest.Data);
        
        _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
        work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver id doesn't match an active gameserver this host manages");
        return await Result<GameServerLocal?>.FailAsync("Gameserver id doesn't match an active gameserver this host manages");
    }

    private GameServerLocal? ExtractGameServerFromWorkData(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver {WorkType} request has an empty work data payload: {WorkId}", work.TargetType, work.Id);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
            return null;
        }
        var deserializedServer = _serializerService.DeserializeMemory<GameServerToHost>(work.WorkData);
        if (deserializedServer is not null) return deserializedServer.ToLocal();
        
        _logger.Error("Unable to deserialize work data payload: {WorkId}", work.Id);
        work.SendStatusUpdate(WeaverWorkState.Failed, $"Unable to deserialize work data payload: {work.Id}");
        return deserializedServer?.ToLocal();
    }

    private async Task UpdateGameServerState(WeaverWork work)
    {
        var gameServerLocal = await GetGameServerFromGameServerWorkData(work);
        if (gameServerLocal.Data is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            ServerProcessName = null,
            ServerState = gameServerLocal.Data.ServerState,
            Resources = null
        });

        var gameServerFromHost = ExtractGameServerFromWorkData(work);
        if (gameServerFromHost is null) return;

        var gameServerUpdate = gameServerFromHost.ToUpdate();
        var updateRequest = await _gameServerService.Update(gameServerUpdate);
        if (!updateRequest.Succeeded)
        {
            _logger.Error("Failed to update server locally with new state from server [{WorkId}]: {GameserverId}", work.Id, gameServerLocal.Data.Id);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Failed to update server locally with new state from server [{work.Id}]: {gameServerLocal.Data.Id}");
            return;
        }
        
        work.SendStatusUpdate(WeaverWorkState.Completed, "Local gameserver state updated");
        await Task.CompletedTask;
    }

    private async Task StartGameServer(WeaverWork work)
    {
        var gameServerLocal = await GetGameServerFromIdWorkData(work);
        if (gameServerLocal.Data is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            ServerProcessName = null,
            ServerState = ServerState.Unknown,
            Resources = null
        });
        
        var startResult = await _gameServerService.StartServer(gameServerLocal.Data.Id);
        if (!startResult.Succeeded)
        {
            work.SendStatusUpdate(WeaverWorkState.Failed, startResult.Messages);
            return;
        }
        
        _logger.Information("Started gameserver processes: [{GameserverId}]{GameserverName}", gameServerLocal.Data.Id, gameServerLocal.Data.ServerName);

        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.SpinningUp);
        
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            ServerProcessName = null,
            ServerState = ServerState.SpinningUp,
            Resources = null
        });
        
        // TODO: Add thread task to pool to indicate when server is up | Check for listening port of gameserver, once listening we know it's up w/ timeout
    }

    private async Task StopGameServer(WeaverWork work, bool sendCompletion = true)
    {
        var gameServerLocal = await GetGameServerFromIdWorkData(work);
        if (gameServerLocal.Data is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            ServerProcessName = null,
            ServerState = ServerState.ShuttingDown,
            Resources = null
        });
        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.ShuttingDown);
        
        var stopResult = await _gameServerService.StopServer(gameServerLocal.Data.Id);
        if (!stopResult.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = gameServerLocal.Data.Id,
                ServerProcessName = null,
                ServerState = ServerState.Unknown,
                Resources = null
            });
            await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Unknown);
            return;
        }
        
        _logger.Information("Stopped gameserver processes: [{GameserverId}]{GameserverName}", gameServerLocal.Data.Id, gameServerLocal.Data.ServerName);

        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Shutdown);

        var finalStatus = sendCompletion ? WeaverWorkState.Completed : WeaverWorkState.InProgress;
        work.SendGameServerUpdate(finalStatus, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            ServerProcessName = null,
            ServerState = ServerState.Shutdown,
            Resources = null
        });
        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Shutdown);
    }

    private async Task RestartGameServer(WeaverWork work)
    {
        await StopGameServer(work, false);
        await StartGameServer(work);
    }

    private async Task UninstallGameServer(WeaverWork work)
    {
        var gameServerLocal = await GetGameServerFromIdWorkData(work);
        if (gameServerLocal.Data is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            ServerProcessName = null,
            ServerState = ServerState.Uninstalling,
            Resources = null
        });
        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Uninstalling);
        
        await _gameServerService.UninstallGame(gameServerLocal.Data.Id);
        _logger.Information("Finished uninstalling gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Data.Id, gameServerLocal.Data.ServerName);
        
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            ServerProcessName = null,
            ServerState = ServerState.Uninstalled,
            Resources = null
        });
    }

    private async Task UpdateGameServer(WeaverWork work)
    {
        var gameServerLocal = await GetGameServerFromIdWorkData(work);
        if (gameServerLocal.Data is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            ServerProcessName = null,
            ServerState = ServerState.Updating,
            Resources = null
        });

        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Updating);
        
        await _gameServerService.InstallOrUpdateGame(gameServerLocal.Data.Id);
        _logger.Information("Finished updating gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Data.Id, gameServerLocal.Data.ServerName);
        
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            ServerProcessName = null,
            ServerState = ServerState.Shutdown,
            Resources = null
        });
        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Shutdown);
    }

    private async Task InstallGameServer(WeaverWork work)
    {
        var deserializedGameServer = ExtractGameServerFromWorkData(work);
        if (deserializedGameServer is null) return;

        var createServerRequest = await _gameServerService.Create(deserializedGameServer);
        if (!createServerRequest.Succeeded) return;
        
        var gameServerRequest = await _gameServerService.GetById(createServerRequest.Data);
        if (!gameServerRequest.Succeeded || gameServerRequest.Data is null) return;
        
        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerRequest.Data.Id,
            ServerProcessName = null,
            ServerState = ServerState.Installing,
            Resources = null
        });
        await _gameServerService.UpdateState(gameServerRequest.Data.Id, ServerState.Installing);

        var installRequest = await _gameServerService.InstallOrUpdateGame(gameServerRequest.Data.Id);
        if (!installRequest.Succeeded) return;
        
        _logger.Information("Finished installing gameserver: [{GameserverId}]{GameserverName}", gameServerRequest.Data.Id, gameServerRequest.Data.ServerName);

        await _gameServerService.UpdateState(gameServerRequest.Data.Id, ServerState.Shutdown);
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = deserializedGameServer.Id,
            ServerProcessName = null,
            ServerState = ServerState.Shutdown,
            Resources = null
        });
    }
}