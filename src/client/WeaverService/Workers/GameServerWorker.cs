using System.Collections.Concurrent;
using Application.Constants;
using Application.Helpers;
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

    private static ConcurrentBag<GameServerLocal> GameServers { get; set; } = new();

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(GameServerWorker));

        // TODO: Implement Sqlite for game server state tracking, for now we'll serialize/deserialize a json file
        await DeserializeGameServerState();
        await DeserializeWorkQueues();
        
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
                _logger.Error(ex, "Failure occurred during {ServiceName} execution loop", nameof(GameServerWorker));
            }
        }

        await SerializeGameServerState();
        await SerializeWorkQueues();
        
        _logger.Debug("Stopping {ServiceName} service", nameof(GameServerWorker));
    }

    private async Task ProcessWorkQueue()
    {
        if (_inProgressWorkCount >= _generalConfig.Value.SimultaneousQueueWorkCountMax)
        {
            _logger.Verbose("In progress gameserver work [{InProgressWork}] is at max [{MaxWork}], moving on | Waiting: {WaitingWork}", _inProgressWorkCount,
                _generalConfig.Value.SimultaneousQueueWorkCountMax, _workQueue.Count);
            return;
        }
        
        if (_workQueue.Count <= 0)
        {
            _logger.Verbose("No work waiting, moving on");
            return;
        }
        
        // Work is waiting and we have available queue space, adding next work to the thread pool
        var attemptCount = 0;
        while (_inProgressWorkCount < _generalConfig.Value.SimultaneousQueueWorkCountMax)
        {
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
            ThreadHelper.QueueWork(_ => HandleWork(work).RunSynchronously());
        }

        await Task.CompletedTask;
    }

    private async Task BackupGameServers()
    {
        _lastBackupTime ??= _dateTimeService.NowDatabaseTime;

        if (_dateTimeService.NowDatabaseTime < _lastBackupTime.Value.AddMinutes(_generalConfig.Value.GameserverBackupIntervalMinutes)) return;

        foreach (var gameserver in GameServers)
            await _gameServerService.BackupGame(gameserver);
        
        _lastBackupTime = _dateTimeService.NowDatabaseTime;
    }

    public static void AddWorkToQueue(WeaverWork work)
    {
        Log.Debug("Adding gameserver work to waiting queue: {Id} |{WorkType} | {Status}", work.Id, work.Type, work.Status);
        
        // TODO: Need to check against work in progress as well, currently we aren't tracking that work, we need to for reliability
        if (_workQueue.Any(x => x.Id == work.Id))
        {
            Log.Verbose("Work already exists in the queue, skipping duplicate: [{WorkId}] of type {WorkType}", work.Id, work.Type);
            return;
        }

        work.Status = WeaverWorkState.PickedUp;
        _workQueue.Enqueue(work);
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            Type = WeaverWorkTarget.StatusUpdate,
            Status = WeaverWorkState.PickedUp,
            WorkData = MemoryPackSerializer.Serialize(new List<string> { "Work has been picked up" }),
            AttemptCount = 0
        });
        
        Log.Debug("Added gameserver work to queue:{Id} | {WorkType} | {Status}", work.Id, work.Type, work.Status);
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
        GameServers = _serializerService.DeserializeJson<ConcurrentBag<GameServerLocal>>(gameServerState);
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
                Type = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.InProgress,
                WorkData = MemoryPackSerializer.Serialize(new List<string> { "Work has is now in progress" }),
                AttemptCount = 0
            });
            
            switch (work.Type)
            {
                case >= WeaverWorkTarget.Host and < WeaverWorkTarget.GameServer:
                    _logger.Error("Host work somehow got into the GameServer work queue: [{WorkId}]{WorkType}", work.Id, work.Type);
                    HostWorker.AddWorkToQueue(work);
                    _logger.Warning("Gave host work to host worker since it was in the wrong place: [{WorkId}]{WorkType}", work.Id, work.Type);
                    return;
                case >= WeaverWorkTarget.GameServer and < WeaverWorkTarget.CurrentEnd:
                    _logger.Debug("Starting gameserver work from queue: [{WorkId}]{WorkType}", work.Id, work.Type);
                    break;
                default:
                    _logger.Error("Invalid work type for work: [{WorkId}]{WorkType}", work.Id, work.Type);
                    ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
                    {
                        Id = work.Id,
                        Type = WeaverWorkTarget.StatusUpdate,
                        Status = WeaverWorkState.Failed,
                        WorkData = _serializerService.SerializeMemory(new List<string> {"Gameserver work data was invalid, please verify the payload"}),
                        AttemptCount = 0
                    });
                    return;
            }

            switch (work.Type)
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
                case WeaverWorkTarget.StatusUpdate:
                case WeaverWorkTarget.Host:
                case WeaverWorkTarget.HostStatusUpdate:
                case WeaverWorkTarget.HostDetail:
                case WeaverWorkTarget.GameServer:
                case WeaverWorkTarget.CurrentEnd:
                default:
                    _logger.Error("An impossible event occurred, we hit a switch statement that shouldn't be possible for gameserver work: {WorkType}", work.Type);
                    ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
                    {
                        Id = work.Id,
                        Type = WeaverWorkTarget.StatusUpdate,
                        Status = WeaverWorkState.Failed,
                        WorkData = _serializerService.SerializeMemory(
                            new List<string> {$"An impossible event occurred, we hit a switch statement that shouldn't be possible for gameserver work: {work.Type}"}),
                        AttemptCount = 0
                    });
                    return;
            }
            
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Completed,
                WorkData = _serializerService.SerializeMemory(new List<string> { "Work completed" }),
                AttemptCount = 0
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred handling gameserver work: {Error}", ex.Message);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = WeaverWorkTarget.StatusUpdate,
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

    private async Task UninstallGameServer(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver uninstall request has an empty work data payload: {WorkId}", work.Id);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {$"Gameserver uninstall request has an empty work data payload: {work.Id}"}),
                AttemptCount = 0
            });
            return;
        }
        
        var uninstallWork = _serializerService.DeserializeMemory<GameServerUninstallWork>(work.WorkData);
        var gameServerLocal = GameServers.FirstOrDefault(x => x.Id == uninstallWork!.GameserverId);
        if (gameServerLocal is null)
        {
            _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.Type);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {"Gameserver id doesn't match an active gameserver this host manages"}),
                AttemptCount = 0
            });
            return;
        }

        await _gameServerService.UninstallGame(gameServerLocal);
        _logger.Information("Finished uninstalling gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
    }

    private async Task UpdateGameServer(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver update request has an empty work data payload: {WorkId}", work.Id);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {$"Gameserver update request has an empty work data payload: {work.Id}"}),
                AttemptCount = 0
            });
            return;
        }
        
        var updateWork = _serializerService.DeserializeMemory<GameServerUpdateWork>(work.WorkData);
        var gameServerLocal = GameServers.FirstOrDefault(x => x.Id == updateWork!.GameserverId);
        if (gameServerLocal is null)
        {
            _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.Type);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {"Gameserver id doesn't match an active gameserver this host manages"}),
                AttemptCount = 0
            });
            return;
        }

        await _gameServerService.InstallOrUpdateGame(gameServerLocal);
        _logger.Information("Finished updating gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
    }

    private async Task InstallGameServer(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver install request has an empty work data payload: {WorkId}", work.Id);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = WeaverWorkTarget.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = _serializerService.SerializeMemory(new List<string> {$"Gameserver install request has an empty work data payload: {work.Id}"}),
                AttemptCount = 0
            });
            return;
        }
        // TODO: Add converted gameserver to gameservers local list before install
        
        // TODO: Update placeholder to actual aggregated object
        var gameServer = _serializerService.DeserializeMemory<GameServerLocal>(work.WorkData);
        await _gameServerService.InstallOrUpdateGame(gameServer!);
        _logger.Information("Finished installing gameserver: [{GameserverId}]{GameserverName}", gameServer!.Id, gameServer.ServerName);
        throw new NotImplementedException();
    }
}