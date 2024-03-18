using System.Collections.Concurrent;
using Application.Constants;
using Application.Helpers;
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

    public static ConcurrentBag<GameServerLocal> GameServers { get; private set; } = new();

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
        _workQueue.Enqueue(work);
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
        
        _logger.Information("Serialized in progress queue file: {FilePath}", GameServerConstants.WorkQueuePath);
    }

    private async Task DeserializeWorkQueues()
    {
        if (!File.Exists(GameServerConstants.WorkQueuePath))
        {
            _logger.Debug("Work queue file doesn't exist, creating... [{FilePath}]", GameServerConstants.WorkQueuePath);
            await SerializeWorkQueues();
        }

        var workQueue = await File.ReadAllTextAsync(GameServerConstants.WorkQueuePath);
        _workQueue = _serializerService.DeserializeJson<ConcurrentQueue<WeaverWorkClient>>(workQueue);
        
        _logger.Information("Deserialized gameserver work queue");
    }

    private async Task ProcessWorkQueue()
    {
        var inProgressWorkCount = _workQueue.Count(x => x.Status == WeaverWorkState.InProgress);
        var workWaitingCount = _workQueue.Count;

        if (inProgressWorkCount >= _generalConfig.Value.SimultaneousQueueWorkCountMax)
        {
            _logger.Verbose("In progress work [{InProgressWork}] is at max [{MaxWork}], moving on | Waiting: {WaitingWork}", inProgressWorkCount,
                _generalConfig.Value.SimultaneousQueueWorkCountMax, workWaitingCount);
            return;
        }
        
        if (workWaitingCount <= 0)
        {
            _logger.Verbose("No work waiting, moving on");
            return;
        }
        
        // Work is waiting and we have available queue space, adding next work to the thread pool
        foreach (var work in _workQueue.Where(x => x.Status == WeaverWorkState.PickedUp))
        {
            work.Status = WeaverWorkState.InProgress;
            ThreadHelper.QueueWork(_ => HandleWork(work).RunSynchronously());
        }
    }

    private async Task HandleWork(WeaverWorkClient work)
    {
        // TODO: Implement gameserver work and data for handling
        switch (work.TargetType)
        {
            case WeaverWorkTarget.Host:
                break;
            case WeaverWorkTarget.GameServer:
                break;
            default:
                _logger.Error("Invalid work type for work: [{WorkId}]{GamerServerId} of type {WorkType}", work.Id, work.GameServerId, work.TargetType);
                break;
        }
        await Task.CompletedTask;
    }
}