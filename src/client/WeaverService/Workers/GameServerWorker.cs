using System.Collections.Concurrent;
using Application.Constants;
using Application.Services;
using Application.Settings;
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
    private static ConcurrentQueue<WeaverWorkClient> _workInProgressQueue = new();
    private static ConcurrentQueue<WeaverWorkClient> _workWaitingQueue = new();

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
            
                // TODO: Handle moving work waiting into in progress queue
            
                // TODO: Handle in progress queue work

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
        _workWaitingQueue.Enqueue(work);
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
        var serializedInProgressQueue = _serializerService.SerializeJson(_workInProgressQueue);
        await File.WriteAllTextAsync(GameServerConstants.InProgressQueuePath, serializedInProgressQueue);
        
        _logger.Information("Serialized in progress queue file: {FilePath}", GameServerConstants.InProgressQueuePath);

        var serializedWaitingQueue = _serializerService.SerializeJson(_workWaitingQueue);
        await File.WriteAllTextAsync(GameServerConstants.WaitingQueuePath, serializedWaitingQueue);
        
        _logger.Information("Serialized in waiting queue file: {FilePath}", GameServerConstants.WaitingQueuePath);
    }

    private async Task DeserializeWorkQueues()
    {
        if (!File.Exists(GameServerConstants.InProgressQueuePath) || !File.Exists(GameServerConstants.WaitingQueuePath))
        {
            _logger.Debug("Work queue file(s) doesn't exist, creating...");
            await SerializeWorkQueues();
        }

        var inProgressQueue = await File.ReadAllTextAsync(GameServerConstants.InProgressQueuePath);
        _workInProgressQueue = _serializerService.DeserializeJson<ConcurrentQueue<WeaverWorkClient>>(inProgressQueue);
        
        _logger.Information("Deserialized in progress queue");
        
        var waitingQueue = await File.ReadAllTextAsync(GameServerConstants.WaitingQueuePath);
        _workInProgressQueue = _serializerService.DeserializeJson<ConcurrentQueue<WeaverWorkClient>>(waitingQueue);
        
        _logger.Information("Deserialized in waiting queue");
    }
}