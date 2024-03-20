using System.Collections.Concurrent;
using Application.Constants;
using Application.Helpers;
using Application.Requests.Host;
using Application.Services;
using Application.Settings;
using Domain.Enums;
using Domain.Models.ControlServer;
using Domain.Models.GameServer;
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
    private static ConcurrentQueue<WeaverWorkClient> _workQueue = new();
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

    public static void AddWorkToQueue(WeaverWorkClient work)
    {
        Log.Debug("Adding gameserver work to waiting queue: {Id} | {GameServerId} | {WorkType} | {Status}", work.Id, work.GameServerId, work.TargetType, work.Status);
        
        if (_workQueue.Any(x => x.Id == work.Id))
        {
            Log.Verbose("Work already exists in the queue, skipping duplicate: [{WorkId}]{GamerServerId} of type {WorkType}",
                work.Id, work.GameServerId, work.TargetType);
            return;
        }

        work.Status = WeaverWorkState.PickedUp;
        _workQueue.Enqueue(work);
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            Type = HostWorkType.StatusUpdate,
            Status = WeaverWorkState.PickedUp,
            WorkData = null,
            AttemptCount = 0
        });
        
        Log.Debug("Added gameserver work to queue:{Id} | {GameServerId} | {WorkType} | {Status}", work.Id, work.GameServerId, work.TargetType, work.Status);
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
        _workQueue = _serializerService.DeserializeJson<ConcurrentQueue<WeaverWorkClient>>(workQueue);
        
        _logger.Information("Deserialized gameserver work queue");
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

    private async Task HandleWork(WeaverWorkClient work)
    {
        try
        {
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = HostWorkType.StatusUpdate,
                Status = WeaverWorkState.InProgress,
                WorkData = null,
                AttemptCount = 0
            });
            
            switch (work.TargetType)
            {
                case WeaverWorkTarget.Host:
                    _logger.Error("Host work somehow got into the GameServer work queue: [{WorkId}]{WorkType}", work.Id, work.TargetType);
                    HostWorker.AddWorkToQueue(work);
                    _logger.Warning("Gave host work to host worker since it was in the wrong place: [{WorkId}]{WorkType}", work.Id, work.TargetType);
                    return;
                case WeaverWorkTarget.GameServer:
                    _logger.Debug("Starting gameserver work from queue: [{WorkId}]{GamerServerId}", work.Id, work.GameServerId);
                    break;
                default:
                    _logger.Error("Invalid work type for work: [{WorkId}]{GamerServerId} of type {WorkType}", work.Id, work.GameServerId, work.TargetType);
                    ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
                    {
                        Id = work.Id,
                        Type = HostWorkType.StatusUpdate,
                        Status = WeaverWorkState.Failed,
                        WorkData = new GameServerWork { Messages = new List<string> {"Gameserver work data was invalid, please verify the payload"}},
                        AttemptCount = 0
                    });
                    return;
            }
            
            if (work.WorkData is null)
            {
                _logger.Error("Gameserver work data is empty, no work to do? [{WorkId}]{GamerServerId} of type {WorkType}", work.Id, work.GameServerId, work.TargetType);
                ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
                {
                    Id = work.Id,
                    Type = HostWorkType.StatusUpdate,
                    Status = WeaverWorkState.Failed,
                    WorkData = new GameServerWork { Messages = new List<string> {"Gameserver work data was empty, nothing to work with"}},
                    AttemptCount = 0
                });
                return;
            }

            var gameServerLocal = GameServers.FirstOrDefault(x => x.Id == work.GameServerId);
            if (gameServerLocal is null)
            {
                _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}]{GamerServerId} of type {WorkType}",
                    work.Id, work.GameServerId, work.TargetType);
                ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
                {
                    Id = work.Id,
                    Type = HostWorkType.StatusUpdate,
                    Status = WeaverWorkState.Failed,
                    WorkData = new GameServerWork { Messages = new List<string> {"Gameserver id doesn't match an active gameserver this host manages"}},
                    AttemptCount = 0
                });
                return;
            }
            
            _logger.Information("Starting work for gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
            var workData = work.WorkData as GameServerWork;
            switch (workData!.Type)
            {
                case GameServerWorkType.Install:
                    await _gameServerService.InstallOrUpdateGame(gameServerLocal);
                    _logger.Information("Finished installing gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
                    break;
                case GameServerWorkType.Update:
                    await _gameServerService.InstallOrUpdateGame(gameServerLocal);
                    _logger.Information("Finished updating gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
                    break;
                case GameServerWorkType.Uninstall:
                    await _gameServerService.UninstallGame(gameServerLocal);
                    _logger.Information("Finished updating gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Id, gameServerLocal.ServerName);
                    break;
                default:
                    _logger.Error("Unsupported gameserver work type asked for: Asked {WorkType} for [{GameserverId}]{GameserverName}",
                        workData.Type, gameServerLocal.Id, gameServerLocal.ServerName);
                    ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
                    {
                        Id = work.Id,
                        Type = HostWorkType.StatusUpdate,
                        Status = WeaverWorkState.Failed,
                        WorkData = new GameServerWork { Messages = new List<string> {$"Unsupported gameserver work type asked for: {workData.Type}"}},
                        AttemptCount = 0
                    });
                    return;
            }
            
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = HostWorkType.StatusUpdate,
                Status = WeaverWorkState.Completed,
                WorkData = null,
                AttemptCount = 0
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred handling gameserver work: {Error}", ex.Message);
            ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
            {
                Id = work.Id,
                Type = HostWorkType.StatusUpdate,
                Status = WeaverWorkState.Failed,
                WorkData = new GameServerWork { Messages = new List<string> {$"Failure occurred handling gameserver work: {ex.Message}"}},
                AttemptCount = 0
            });
        }
        finally
        {
            _inProgressWorkCount--;
        }
    }
}