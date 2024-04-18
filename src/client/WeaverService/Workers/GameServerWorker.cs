using System.Collections.Concurrent;
using Application.Constants;
using Application.Helpers;
using Application.Mappers;
using Application.Requests.Host;
using Application.Services;
using Application.Settings;
using Domain.Enums;
using Domain.Models.ControlServer;
using Domain.Models.GameServer;
using MemoryPack;
using Microsoft.Extensions.Options;
using Serilog;

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
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.StatusUpdate,
            Status = WeaverWorkState.PickedUp,
            WorkData = MemoryPackSerializer.Serialize(new List<string> { "Work has been picked up" }),
            AttemptCount = 0
        });
        
        Log.Debug("Added gameserver work to queue:{Id} | {WorkType} | {Status}", work.Id, work.TargetType, work.Status);
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
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.InProgress,
                WorkData = MemoryPackSerializer.Serialize(new List<string> { "Work has is now in progress" }),
                AttemptCount = 0
            });
            
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
                    ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
                    {
                        Id = work.Id,
                        TargetType = WeaverWorkTarget.StatusUpdate,
                        Status = WeaverWorkState.Failed,
                        WorkData = _serializerService.SerializeMemory(new List<string> {"Gameserver work data was invalid, please verify the payload"}),
                        AttemptCount = 0
                    });
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
                case WeaverWorkTarget.StatusUpdate:
                case WeaverWorkTarget.Host:
                case WeaverWorkTarget.HostStatusUpdate:
                case WeaverWorkTarget.HostDetail:
                case WeaverWorkTarget.GameServer:
                case WeaverWorkTarget.CurrentEnd:
                case WeaverWorkTarget.GameServerStateUpdate:
                default:
                    _logger.Error("An impossible event occurred, we hit a switch statement that shouldn't be possible for gameserver work: {WorkType}", work.TargetType);
                    ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
                    {
                        Id = work.Id,
                        TargetType = WeaverWorkTarget.StatusUpdate,
                        Status = WeaverWorkState.Failed,
                        WorkData = _serializerService.SerializeMemory(
                            new List<string> {$"An impossible event occurred, we hit a switch statement that shouldn't be possible for gameserver work: {work.TargetType}"}),
                        AttemptCount = 0
                    });
                    return;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred handling gameserver work: {Error}", ex.Message);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {$"Failure occurred handling gameserver work: {ex.Message}"}),
                AttemptCount = 0
            });
        }
        finally
        {
            _inProgressWorkCount--;
        }
    }

    private async Task StartGameServer(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver start request has an empty work data payload: {WorkId}", work.Id);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {$"Gameserver start request has an empty work data payload: {work.Id}"}),
                AttemptCount = 0
            });
            return;
        }
        
        var gameServerId = _serializerService.DeserializeMemory<Guid>(work.WorkData);
        var gotGameServer = GameServers.TryGetValue(gameServerId, out var gameServerLocal);
        if (!gotGameServer || gameServerLocal is null)
        {
            _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {"Gameserver id doesn't match an active gameserver this host manages"}),
                AttemptCount = 0
            });
            return;
        }

        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.InProgress,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = gameServerLocal.Id,
                ServerProcessName = null,
                ServerState = ServerState.Unknown,
                Resources = null
            }),
            AttemptCount = 0
        });
        
        var startResult = await _gameServerService.StartServer(gameServerLocal);
        if (!startResult.Succeeded)
        {
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(startResult.Messages),
                AttemptCount = 0
            });
            return;
        }
        
        _logger.Information("Started gameserver processes: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        
        // TODO: Update gameserver local status on each modification
        
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.Completed,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = gameServerLocal.Id,
                ServerProcessName = null,
                ServerState = ServerState.SpinningUp,
                Resources = null
            }),
            AttemptCount = 0
        });
        
        // TODO: Add thread task to pool to indicate when server is up
    }

    private async Task StopGameServer(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver stop request has an empty work data payload: {WorkId}", work.Id);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {$"Gameserver stop request has an empty work data payload: {work.Id}"}),
                AttemptCount = 0
            });
            return;
        }
        
        var gameServerId = _serializerService.DeserializeMemory<Guid>(work.WorkData);
        var gotGameServer = GameServers.TryGetValue(gameServerId, out var gameServerLocal);
        if (!gotGameServer || gameServerLocal is null)
        {
            _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {"Gameserver id doesn't match an active gameserver this host manages"}),
                AttemptCount = 0
            });
            return;
        }

        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.InProgress,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = gameServerLocal.Id,
                ServerProcessName = null,
                ServerState = ServerState.ShuttingDown,
                Resources = null
            }),
            AttemptCount = 0
        });
        
        var stopResult = await _gameServerService.StopServer(gameServerLocal);
        if (!stopResult.Succeeded)
        {
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(stopResult.Messages),
                AttemptCount = 0
            });
            return;
        }
        
        _logger.Information("Stopped gameserver processes: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        
        // TODO: Update gameserver local status on each modification
        
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.Completed,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = gameServerLocal.Id,
                ServerProcessName = null,
                ServerState = ServerState.Shutdown,
                Resources = null
            }),
            AttemptCount = 0
        });
    }

    private async Task UninstallGameServer(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver uninstall request has an empty work data payload: {WorkId}", work.Id);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {$"Gameserver uninstall request has an empty work data payload: {work.Id}"}),
                AttemptCount = 0
            });
            return;
        }
        
        var gameServerId = _serializerService.DeserializeMemory<Guid>(work.WorkData);
        var gotGameServer = GameServers.TryGetValue(gameServerId, out var gameServerLocal);
        if (!gotGameServer || gameServerLocal is null)
        {
            _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Completed,
                WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
                {
                    Id = gameServerId,
                    ServerProcessName = null,
                    ServerState = ServerState.Uninstalled,
                    Resources = null
                }),
                AttemptCount = 0
            });
            return;
        }

        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.InProgress,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = gameServerLocal.Id,
                ServerProcessName = null,
                ServerState = ServerState.Uninstalling,
                Resources = null
            }),
            AttemptCount = 0
        });
        
        await _gameServerService.UninstallGame(gameServerLocal);
        _logger.Information("Finished uninstalling gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.Completed,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = gameServerLocal.Id,
                ServerProcessName = null,
                ServerState = ServerState.Uninstalled,
                Resources = null
            }),
            AttemptCount = 0
        });

        var removedGameServer = GameServers.TryRemove(new KeyValuePair<Guid, GameServerLocal>(gameServerLocal.Id, gameServerLocal));
        if (!removedGameServer)
            _logger.Error("Failed to remove local game server: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
    }

    private async Task UpdateGameServer(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver update request has an empty work data payload: {WorkId}", work.Id);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {$"Gameserver update request has an empty work data payload: {work.Id}"}),
                AttemptCount = 0
            });
            return;
        }
        
        var updateWork = _serializerService.DeserializeMemory<GameServerUpdateWork>(work.WorkData);
        var gotGameServer = GameServers.TryGetValue(updateWork!.GameserverId, out var gameServerLocal);
        if (!gotGameServer || gameServerLocal is null)
        {
            _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {"Gameserver id doesn't match an active gameserver this host manages"}),
                AttemptCount = 0
            });
            return;
        }

        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.InProgress,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = gameServerLocal.Id,
                ServerProcessName = null,
                ServerState = ServerState.Updating,
                Resources = null
            }),
            AttemptCount = 0
        });
        
        await _gameServerService.InstallOrUpdateGame(gameServerLocal);
        _logger.Information("Finished updating gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
        
        
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.Completed,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = gameServerLocal.Id,
                ServerProcessName = null,
                ServerState = ServerState.Shutdown,
                Resources = null
            }),
            AttemptCount = 0
        });
    }

    private async Task InstallGameServer(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver install request has an empty work data payload: {WorkId}", work.Id);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                TargetType = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {$"Gameserver install request has an empty work data payload: {work.Id}"}),
                AttemptCount = 0
            });
            return;
        }
        
        var gameServerToHost = _serializerService.DeserializeMemory<GameServerToHost>(work.WorkData);
        var localGameServer = gameServerToHost!.ToLocal();
        localGameServer.LastStateUpdate = _dateTimeService.NowDatabaseTime;
        var gameServer = GameServers.GetOrAdd(localGameServer.Id, localGameServer);
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.InProgress,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = localGameServer.Id,
                ServerProcessName = null,
                ServerState = ServerState.Installing,
                Resources = null
            }),
            AttemptCount = 0
        });

        await _gameServerService.InstallOrUpdateGame(gameServer);
        _logger.Information("Finished installing gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
        
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = WeaverWorkState.Completed,
            WorkData = _serializerService.SerializeMemory(new GameServerStateUpdate
            {
                Id = localGameServer.Id,
                ServerProcessName = null,
                ServerState = ServerState.Shutdown,
                Resources = null
            }),
            AttemptCount = 0
        });
    }
}