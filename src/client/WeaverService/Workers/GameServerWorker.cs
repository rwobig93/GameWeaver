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

    private static ConcurrentDictionary<Guid, GameServerLocal> GameServers { get; set; } = new();

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
        await DeserializeGameServerState();
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

        await SerializeGameServerState();
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

        foreach (var gameserver in GameServers)
            await _gameServerService.BackupGame(gameserver.Value);
        
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

    public static GameServerLocal AddLocalGameserver(GameServerLocal gameServerLocal)
    {
        var gameServer = GameServers.GetOrAdd(gameServerLocal.Id, gameServerLocal);
        Log.Information("Added local gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
        return gameServer;
    }

    public static async Task<IResult> RemoveLocalGameserver(GameServerLocal gameServerLocal)
    {
        var removedGameServer = GameServers.TryRemove(new KeyValuePair<Guid, GameServerLocal>(gameServerLocal.Id, gameServerLocal));
        if (!removedGameServer)
        {
            Log.Error("Gameserver removal failure, local gameserver not found: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
            return await Result.FailAsync($"Gameserver removal failure, local gameserver not found: [{gameServerLocal.Id}]{gameServerLocal.ServerName}");
        }
        
        Log.Information("Removed local gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        return await Result.SuccessAsync();
    }

    public static async Task<IResult> UpdateLocalGameserver(GameServerLocal updatedGameServer, GameServerLocal gameServerLocal)
    {
        if (GameServers.TryUpdate(gameServerLocal.Id, updatedGameServer, gameServerLocal))
            return await Result.SuccessAsync();
        
        Log.Error("Failed to update server locally with new state from server: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        return await Result.FailAsync($"Failed to update server locally with new state from server: [{gameServerLocal.Id}]{gameServerLocal.ServerName}");
    }

    public static async Task<IResult> UpdateLocalGameserver(GameServerLocal updatedGameServer)
    {
        var gotGameServer = GameServers.TryGetValue(updatedGameServer.Id, out var gameServerLocal);
        if (!gotGameServer || gameServerLocal is null)
            return await Result.FailAsync($"Gameserver with Id {updatedGameServer.Id} wasn't found");

        return await UpdateLocalGameserver(updatedGameServer, gameServerLocal);
    }

    private async Task SerializeGameServerState()
    {
        var serializedGameserverState = _serializerService.SerializeJson(GameServers);
        await File.WriteAllTextAsync(GameServerConstants.GameServerStatePath, serializedGameserverState);
        _logger.Information("Serialized gameserver state file: {FilePath}", GameServerConstants.GameServerStatePath);
    }

    private async Task DeserializeGameServerState()
    {
        if (!File.Exists(GameServerConstants.GameServerStatePath))
        {
            _logger.Debug("Gameserver state file doesn't exist, creating... [{FilePath}]", GameServerConstants.GameServerStatePath);
            await SerializeGameServerState();
        }
        
        var gameServerState = await File.ReadAllTextAsync(GameServerConstants.GameServerStatePath);
        GameServers = _serializerService.DeserializeJson<ConcurrentDictionary<Guid, GameServerLocal>>(gameServerState);
        _logger.Information("Deserialized gameserver state");
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
                    await SerializeGameServerState();
                    break;
                case WeaverWorkTarget.GameServerUpdate:
                    await UpdateGameServer(work);
                    break;
                case WeaverWorkTarget.GameServerUninstall:
                    await UninstallGameServer(work);
                    await SerializeGameServerState();
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
                    await SerializeGameServerState();
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

    private GameServerLocal? GetGameServerFromIdWorkData(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver {WorkType} request has an empty work data payload: {WorkId}", work.TargetType, work.Id);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
            return null;
        }
        
        var gameServerId = _serializerService.DeserializeMemory<Guid>(work.WorkData);
        var gotGameServer = GameServers.TryGetValue(gameServerId, out var gameServerLocal);
        if (gotGameServer && gameServerLocal is not null) return gameServerLocal;
        
        _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
        work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver id doesn't match an active gameserver this host manages");
        return null;
    }

    private GameServerLocal? GetGameServerFromGameServerWorkData(WeaverWork work, out GameServerToHost gameServerFromWorkData)
    {
        gameServerFromWorkData = new GameServerToHost();
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver {WorkType} request has an empty work data payload: {WorkId}", work.TargetType, work.Id);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
            return null;
        }
        
        var deserializedGameServer = _serializerService.DeserializeMemory<GameServerToHost>(work.WorkData);
        if (deserializedGameServer is null)
        {
            _logger.Error("Gameserver wasn't deserializable from work data payload: {WorkId}", work.Id);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver wasn't deserializable from work data payload: {work.Id}");
            return null;
        }

        gameServerFromWorkData = deserializedGameServer;
        
        var gotGameServer = GameServers.TryGetValue(gameServerFromWorkData.Id, out var gameServerLocal);
        if (gotGameServer && gameServerLocal is not null) return gameServerLocal;
        
        _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
        work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver id doesn't match an active gameserver this host manages");
        return null;
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
        var gameServerLocal = GetGameServerFromGameServerWorkData(work, out var workDataGameserver);
        if (gameServerLocal is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = gameServerLocal.ServerState,
            Resources = null
        });

        var updatedGameServer = workDataGameserver.ToLocal();
        updatedGameServer.LastStateUpdate = _dateTimeService.NowDatabaseTime;
        updatedGameServer.ServerState = gameServerLocal.ServerState;

        if (!GameServers.TryUpdate(gameServerLocal.Id, updatedGameServer, gameServerLocal))
        {
            _logger.Error("Failed to update server locally with new state from server [{WorkId}]: {GameserverId}", work.Id, gameServerLocal.Id);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Failed to update server locally with new state from server [{work.Id}]: {gameServerLocal.Id}");
            return;
        }
        
        work.SendStatusUpdate(WeaverWorkState.Completed, "Local gameserver state updated");
        await Task.CompletedTask;
    }

    private async Task StartGameServer(WeaverWork work)
    {
        var gameServerLocal = GetGameServerFromIdWorkData(work);
        if (gameServerLocal is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.Unknown,
            Resources = null
        });
        
        var startResult = await _gameServerService.StartServer(gameServerLocal);
        if (!startResult.Succeeded)
        {
            work.SendStatusUpdate(WeaverWorkState.Failed, startResult.Messages);
            return;
        }
        
        _logger.Information("Started gameserver processes: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        
        // TODO: Update gameserver local status on each modification
        
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.SpinningUp,
            Resources = null
        });
        
        // TODO: Add thread task to pool to indicate when server is up | Check for listening port of gameserver, once listening we know it's up w/ timeout
    }

    private async Task StopGameServer(WeaverWork work, bool sendCompletion = true)
    {
        var gameServerLocal = GetGameServerFromIdWorkData(work);
        if (gameServerLocal is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.ShuttingDown,
            Resources = null
        });
        gameServerLocal.ServerState = ServerState.ShuttingDown;
        await UpdateLocalGameserver(gameServerLocal);
        
        var stopResult = await _gameServerService.StopServer(gameServerLocal);
        if (!stopResult.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = gameServerLocal.Id,
                ServerProcessName = null,
                ServerState = ServerState.Unknown,
                Resources = null
            });
            gameServerLocal.ServerState = ServerState.Unknown;
            await UpdateLocalGameserver(gameServerLocal);
            return;
        }
        
        _logger.Information("Stopped gameserver processes: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        
        // TODO: Update gameserver local status on each modification

        var finalStatus = sendCompletion ? WeaverWorkState.Completed : WeaverWorkState.InProgress;
        work.SendGameServerUpdate(finalStatus, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.Shutdown,
            Resources = null
        });
    }

    private async Task RestartGameServer(WeaverWork work)
    {
        await StopGameServer(work, false);
        await StartGameServer(work);
    }

    private async Task UninstallGameServer(WeaverWork work)
    {
        var gameServerLocal = GetGameServerFromIdWorkData(work);
        if (gameServerLocal is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.Uninstalling,
            Resources = null
        });
        gameServerLocal.ServerState = ServerState.Uninstalling;
        await UpdateLocalGameserver(gameServerLocal);
        
        await _gameServerService.UninstallGame(gameServerLocal);
        _logger.Information("Finished uninstalling gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.Uninstalled,
            Resources = null
        });

        var removedGameServer = await RemoveLocalGameserver(gameServerLocal);
        if (!removedGameServer.Succeeded)
            _logger.Error("Failed to remove local game server: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
    }

    private async Task UpdateGameServer(WeaverWork work)
    {
        var gameServerLocal = GetGameServerFromIdWorkData(work);
        if (gameServerLocal is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.Updating,
            Resources = null
        });
        
        gameServerLocal.ServerState = ServerState.Updating;
        await UpdateLocalGameserver(gameServerLocal);
        
        await _gameServerService.InstallOrUpdateGame(gameServerLocal);
        _logger.Information("Finished updating gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.Shutdown,
            Resources = null
        });
        gameServerLocal.ServerState = ServerState.Shutdown;
        await UpdateLocalGameserver(gameServerLocal);
    }

    private async Task InstallGameServer(WeaverWork work)
    {
        var gameServerLocal = ExtractGameServerFromWorkData(work);
        if (gameServerLocal is null) return;

        gameServerLocal.LastStateUpdate = _dateTimeService.NowDatabaseTime;
        gameServerLocal.ServerState = ServerState.Installing;
        var gameServer = AddLocalGameserver(gameServerLocal);
        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.Installing,
            Resources = null
        });

        await _gameServerService.InstallOrUpdateGame(gameServer);
        _logger.Information("Finished installing gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);

        gameServer.ServerState = ServerState.Shutdown;
        await UpdateLocalGameserver(gameServer);
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = gameServerLocal.Id,
            ServerProcessName = null,
            ServerState = ServerState.Shutdown,
            Resources = null
        });
    }
}