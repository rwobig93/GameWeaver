using System.Text;
using Application.Helpers;
using Application.Mappers;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models.ControlServer;
using Domain.Models.GameServer;
using Microsoft.Extensions.Options;
using WeaverService.Helpers;

namespace WeaverService.Workers;

public class GameServerWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IGameServerService _gameServerService;
    private readonly IOptions<GeneralConfiguration> _generalConfig;
    private readonly ISerializerService _serializerService;
    private readonly IWeaverWorkService _weaverWorkService;

    /// <summary>
    /// Handles Gameserver work including updates, configuration and per server IO
    /// </summary>
    public GameServerWorker(ILogger logger, IDateTimeService dateTimeService, IGameServerService gameServerService, IOptions<GeneralConfiguration> generalConfig,
        ISerializerService serializerService, IWeaverWorkService weaverWorkService)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
        _gameServerService = gameServerService;
        _generalConfig = generalConfig;
        _serializerService = serializerService;
        _weaverWorkService = weaverWorkService;
    }

    private static DateTime _lastRuntime;
    private static DateTime? _lastBackupTime;
    private static DateTime? _lastStateCheck;

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(GameServerWorker));

        await CheckGameServersRealtimeState();

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
                await CheckGameServersRealtimeState();

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

        _logger.Debug("Stopped {ServiceName} service", nameof(GameServerWorker));
        await base.StopAsync(stoppingToken);
    }

    private async Task ProcessWorkQueue()
    {
        var inProgressCount = await _weaverWorkService.GetCountInProgressGameserverAsync();
        var waitingCount = await _weaverWorkService.GetCountWaitingGameserverAsync();

        if (inProgressCount.Data >= _generalConfig.Value.SimultaneousQueueWorkCountMax)
        {
            _logger.Verbose("In progress gameserver work [{InProgressWork}] is at max [{MaxWork}], moving on | Waiting: {WaitingWork}",
                inProgressCount.Data, _generalConfig.Value.SimultaneousQueueWorkCountMax, waitingCount.Data);
            return;
        }

        if (waitingCount.Data < 1)
        {
            _logger.Verbose("No gameserver work waiting, moving on");
            return;
        }

        // Work is waiting, and we have available queue space, adding next work to the thread pool
        var attemptCount = 0;
        while (inProgressCount.Data < _generalConfig.Value.SimultaneousQueueWorkCountMax)
        {
            inProgressCount = await _weaverWorkService.GetCountInProgressGameserverAsync();
            waitingCount = await _weaverWorkService.GetCountWaitingGameserverAsync();
            if (waitingCount.Data < 1) break;

            if (attemptCount >= 5)
            {
                _logger.Error("Reached max attempt count in the gameserver work queue, quiting cycle queue processing");
                return;
            }

            var nextWork = await _weaverWorkService.GetNextWaitingGameserverAsync();
            if (!nextWork.Succeeded || nextWork.Data is null)
            {
                _logger.Error("Failure occurred getting next gameserver work from service: {Error}", nextWork.Messages);
                attemptCount++;
                continue;
            }

            var updateRequest = await _weaverWorkService.UpdateStatusAsync(nextWork.Data.Id, WeaverWorkState.InProgress);
            if (!updateRequest.Succeeded)
            {
                _logger.Error("Failure occurred updating gameserver work from service: [{WeaverworkId}]{Error}", nextWork.Data.Id, updateRequest.Messages);
                attemptCount++;
                continue;
            }
            nextWork.Data.SendStatusUpdate(WeaverWorkState.InProgress, "Work is now in progress");

            // ReSharper disable once AsyncVoidLambda
            ThreadHelper.QueueWork(async _ => await HandleWork(nextWork.Data));
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

    private async Task CheckGameServersRealtimeState()
    {
        _lastStateCheck ??= _dateTimeService.NowDatabaseTime;

        if (_dateTimeService.NowDatabaseTime < _lastStateCheck.Value.AddSeconds(_generalConfig.Value.GameServerStatusCheckIntervalSeconds))
        {
            return;
        }

        var allGameServers = await _gameServerService.GetAll();

        foreach (var gameServer in allGameServers.Data)
        {
            var realtimeState = (await _gameServerService.GetCurrentRealtimeState(gameServer.Id)).Data;

            if (gameServer.ServerState == realtimeState)
            {
                continue;
            }
            if (gameServer.ServerState == ServerState.Connectable && (realtimeState is ServerState.InternallyConnectable or ServerState.Unreachable))
            {
                continue;
            }

            switch (realtimeState)
            {
                case ServerState.Shutdown when
                    gameServer.ServerState is ServerState.Installing or
                        ServerState.Restarting or
                        ServerState.Uninstalling or
                        ServerState.ShuttingDown:
                case ServerState.Stalled when
                    gameServer.ServerState is ServerState.Installing or
                        ServerState.Restarting or
                        ServerState.Uninstalling or
                        ServerState.ShuttingDown or
                        ServerState.SpinningUp or
                        ServerState.Discovering:
                    continue;
            }

            _logger.Information("Realtime server state has changed from expected: [{GameserverId}]{GameserverName} [Expected]{ExpectedState} [Current]{CurrentState}",
                gameServer.Id, gameServer.ServerName, gameServer.ServerState, realtimeState);
            await _gameServerService.UpdateState(gameServer.Id, realtimeState);
            new WeaverWork().SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
            {
                Id = gameServer.Id,
                ServerState = realtimeState
            });
        }

        _lastStateCheck = _dateTimeService.NowDatabaseTime;
    }

    private async Task HandleWork(WeaverWork work)
    {
        try
        {
            switch (work.TargetType)
            {
                case >= WeaverWorkTarget.GameServer and < WeaverWorkTarget.CurrentEnd:
                    _logger.Debug("Starting gameserver work from queue: [{WorkId}]{WorkType}", work.Id, work.TargetType);
                    break;
                default:
                    _logger.Error("Invalid work type for work: [{WorkId}]{WorkType}", work.Id, work.TargetType);
                    await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
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
                case WeaverWorkTarget.GameServerConfigNew:
                case WeaverWorkTarget.GameServerConfigUpdate:
                    await ConfigureGameServerUpdate(work);
                    break;
                case WeaverWorkTarget.GameServerConfigDelete:
                    await ConfigureGameServerDelete(work);
                    break;
                case WeaverWorkTarget.GameServerConfigUpdateFull:
                    await ConfigureGameServerUpdateFull(work);
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
                    await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
                    work.SendStatusUpdate(WeaverWorkState.Failed,
                        $"An impossible event occurred, we hit a switch statement that shouldn't be possible for gameserver work: {work.TargetType}");
                    return;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred handling gameserver work: {Error}", ex.Message);
            await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Failure occurred handling gameserver work: {ex.Message}");
        }
    }

    private async Task<IResult<GameServerLocal?>> GetGameServerFromIdWorkData(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver {WorkType} request has an empty work data payload: {WorkId}", work.TargetType, work.Id);
            await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
            return await Result<GameServerLocal?>.FailAsync($"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
        }

        var gameServerId = _serializerService.DeserializeMemory<Guid>(work.WorkData);
        var gameServerRequest = await _gameServerService.GetById(gameServerId);
        if (gameServerRequest is {Succeeded: true, Data: not null})
            return await Result<GameServerLocal?>.SuccessAsync(gameServerRequest.Data);

        _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
        await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
        work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver id doesn't match an active gameserver this host manages");
        return await Result<GameServerLocal?>.FailAsync("Gameserver id doesn't match an active gameserver this host manages");
    }

    private async Task<IResult<GameServerLocal?>> GetGameServerFromGameServerWorkData(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver {WorkType} request has an empty work data payload: {WorkId}", work.TargetType, work.Id);
            await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
            return await Result<GameServerLocal?>.FailAsync($"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
        }

        var deserializedGameServer = _serializerService.DeserializeMemory<GameServerToHost>(work.WorkData);
        if (deserializedGameServer is null)
        {
            _logger.Error("Gameserver wasn't deserializable from work data payload: {WorkId}", work.Id);
            await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver wasn't deserializable from work data payload: {work.Id}");
            return await Result<GameServerLocal?>.FailAsync($"Gameserver wasn't deserializable from work data payload: {work.Id}");
        }

        var gameServerRequest = await _gameServerService.GetById(deserializedGameServer.Id);
        if (gameServerRequest is {Succeeded: true, Data: not null})
            return await Result<GameServerLocal?>.SuccessAsync(gameServerRequest.Data);

        _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
        await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
        work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver id doesn't match an active gameserver this host manages");
        return await Result<GameServerLocal?>.FailAsync("Gameserver id doesn't match an active gameserver this host manages");
    }

    private async Task<GameServerLocal?> ExtractGameServerFromWorkData(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver {WorkType} request has an empty work data payload: {WorkId}", work.TargetType, work.Id);
            await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
            return null;
        }
        var deserializedServer = _serializerService.DeserializeMemory<GameServerToHost>(work.WorkData);
        if (deserializedServer is not null) return deserializedServer.ToLocal();

        _logger.Error("Unable to deserialize work data payload: {WorkId}", work.Id);
        await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
        work.SendStatusUpdate(WeaverWorkState.Failed, $"Unable to deserialize work data payload: {work.Id}");
        return deserializedServer?.ToLocal();
    }

    private async Task<LocalResource?> ExtractLocalResourceFromWorkData(WeaverWork work)
    {
        if (work.WorkData is null)
        {
            _logger.Error("Gameserver {WorkType} request has an empty work data payload: {WorkId}", work.TargetType, work.Id);
            await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Gameserver {work.TargetType} request has an empty work data payload: {work.Id}");
            return null;
        }
        var deserializedLocation = _serializerService.DeserializeMemory<LocalResource>(work.WorkData);
        if (deserializedLocation is not null) return deserializedLocation;

        _logger.Error("Unable to deserialize work data payload: {WorkId}", work.Id);
        await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
        work.SendStatusUpdate(WeaverWorkState.Failed, $"Unable to deserialize work data payload: {work.Id}");
        return deserializedLocation;
    }

    private async Task UpdateGameServerState(WeaverWork work)
    {
        var gameServerLocal = await GetGameServerFromGameServerWorkData(work);
        if (gameServerLocal.Data is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            BuildVersionUpdated = false,
            ServerState = gameServerLocal.Data.ServerState,
            Resources = null
        });

        var gameServerFromHost = await ExtractGameServerFromWorkData(work);
        if (gameServerFromHost is null) return;

        var gameServerUpdate = gameServerFromHost.ToUpdate();
        var updateRequest = await _gameServerService.Update(gameServerUpdate);
        if (!updateRequest.Succeeded)
        {
            _logger.Error("Failed to update server locally with new state from server [{WorkId}]: {GameserverId}", work.Id, gameServerLocal.Data.Id);
            await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Failed to update server locally with new state from server [{work.Id}]: {gameServerLocal.Data.Id}");
            return;
        }

        await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Completed);
        work.SendStatusUpdate(WeaverWorkState.Completed, "Local gameserver state updated");
        await Task.CompletedTask;
    }

    private async Task<string> GetConfigurationIntegrityHash(IEnumerable<LocalResource> resources)
    {
        StringBuilder fullHashBuilder = new();

        foreach (var resource in resources)
        {
            if (!File.Exists(resource.GetFullPath()))
            {
                fullHashBuilder.Append(string.Empty);
                continue;
            }

            var fileContentHash = FileHelpers.ComputeFileContentSha256Hash(resource.GetFullPath());
            fullHashBuilder.Append(fileContentHash ?? string.Empty);
        }

        return await Task.FromResult(FileHelpers.GetIntegrityHash(fullHashBuilder.ToString()));
    }

    private async Task StartGameServer(WeaverWork work)
    {
        var gameServerLocal = await GetGameServerFromIdWorkData(work);
        if (gameServerLocal.Data is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            BuildVersionUpdated = false,
            ServerState = gameServerLocal.Data.ServerState,
            Resources = null
        });

        var startResult = await _gameServerService.StartServer(gameServerLocal.Data.Id);
        if (!startResult.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = gameServerLocal.Data.Id,
                BuildVersionUpdated = false,
                ServerState = ServerState.Shutdown,
                Messages = startResult.Messages
            });
            work.SendStatusUpdate(WeaverWorkState.Failed, startResult.Messages);
            return;
        }

        _logger.Information("Started gameserver processes: [{GameserverId}]{GameserverName}", gameServerLocal.Data.Id, gameServerLocal.Data.ServerName);

        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.SpinningUp);

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            BuildVersionUpdated = false,
            ServerState = ServerState.SpinningUp,
            Resources = null
        });

        var waitingStartTime = _dateTimeService.NowDatabaseTime;

        while (true)
        {
            if ((_dateTimeService.NowDatabaseTime - waitingStartTime).Minutes > 15)
            {
                await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Stalled);
                work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
                {
                    Id = gameServerLocal.Data.Id,
                    BuildVersionUpdated = false,
                    ServerState = ServerState.Stalled,
                    Resources = null
                });
                break;
            }

            var currentState = await _gameServerService.GetCurrentRealtimeState(gameServerLocal.Data.Id);
            if (!currentState.Succeeded)
            {
                await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Unknown);
                work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
                {
                    Id = gameServerLocal.Data.Id,
                    BuildVersionUpdated = false,
                    ServerState = ServerState.Unknown,
                    Resources = null
                });
                break;
            }

            if (currentState.Data == ServerState.InternallyConnectable)
            {
                var configurationHash = await GetConfigurationIntegrityHash(gameServerLocal.Data.Resources);
                await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.InternallyConnectable, configurationHash, configurationHash);
                work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
                {
                    Id = gameServerLocal.Data.Id,
                    BuildVersionUpdated = false,
                    ServerState = ServerState.InternallyConnectable,
                    Resources = null,
                    RunningConfigHash = configurationHash,
                    StorageConfigHash = configurationHash
                });
                break;
            }

            if (currentState.Data != ServerState.Shutdown)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                continue;
            }

            await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Shutdown);
            work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
            {
                Id = gameServerLocal.Data.Id,
                BuildVersionUpdated = false,
                ServerState = ServerState.Shutdown,
                Resources = null
            });
            break;
        }
    }

    private async Task StopGameServer(WeaverWork work, bool sendCompletion = true)
    {
        var gameServerLocal = await GetGameServerFromIdWorkData(work);
        if (gameServerLocal.Data is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            BuildVersionUpdated = false,
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
                BuildVersionUpdated = false,
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
            BuildVersionUpdated = false,
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
            BuildVersionUpdated = false,
            ServerState = ServerState.Uninstalling,
            Resources = null
        });
        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Uninstalling);

        await _gameServerService.UninstallGame(gameServerLocal.Data.Id);
        _logger.Information("Finished uninstalling gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Data.Id, gameServerLocal.Data.ServerName);

        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            BuildVersionUpdated = false,
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
            BuildVersionUpdated = false,
            ServerState = ServerState.Updating,
            Resources = null
        });

        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Updating);

        await _gameServerService.InstallOrUpdateGame(gameServerLocal.Data.Id, true);
        _logger.Information("Finished updating gameserver: [{GameserverId}]{GameserverName}", gameServerLocal.Data.Id, gameServerLocal.Data.ServerName);

        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = gameServerLocal.Data.Id,
            BuildVersionUpdated = true,
            ServerState = ServerState.Shutdown,
            Resources = null
        });
        await _gameServerService.UpdateState(gameServerLocal.Data.Id, ServerState.Shutdown);
    }

    private async Task InstallGameServer(WeaverWork work)
    {
        var deserializedGameServer = await ExtractGameServerFromWorkData(work);
        if (deserializedGameServer is null) return;

        var overlappingListeningSockets = deserializedGameServer.GetListeningSockets();
        if (overlappingListeningSockets.Count > 0)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = deserializedGameServer.Id,
                BuildVersionUpdated = false,
                ServerState = ServerState.OverlappingPort,
                Resources = null
            });
            return;
        }

        var createServerRequest = await _gameServerService.Create(deserializedGameServer);
        if (!createServerRequest.Succeeded) return;

        var gameServerRequest = await _gameServerService.GetById(createServerRequest.Data);
        if (!gameServerRequest.Succeeded || gameServerRequest.Data is null) return;

        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerRequest.Data.Id,
            BuildVersionUpdated = false,
            ServerState = ServerState.Installing,
            Resources = null
        });
        await _gameServerService.UpdateState(gameServerRequest.Data.Id, ServerState.Installing);

        var installRequest = await _gameServerService.InstallOrUpdateGame(gameServerRequest.Data.Id);
        if (!installRequest.Succeeded) return;

        _logger.Information("Finished installing gameserver: [{GameserverId}]{GameserverName}", gameServerRequest.Data.Id, gameServerRequest.Data.ServerName);

        // Start the server for a short time to generate any necessary config files then kill it to complete installation
        work.SendGameServerUpdate(WeaverWorkState.InProgress, new GameServerStateUpdate
        {
            Id = gameServerRequest.Data.Id,
            BuildVersionUpdated = false,
            ServerState = ServerState.Discovering,
            Resources = null
        });
        var startResult = await _gameServerService.StartServer(gameServerRequest.Data.Id);
        if (!startResult.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = gameServerRequest.Data.Id,
                BuildVersionUpdated = false,
                ServerState = ServerState.Shutdown,
                Messages = startResult.Messages
            });
            work.SendStatusUpdate(WeaverWorkState.Failed, startResult.Messages);
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(20));

        var stopResult = await _gameServerService.StopServer(gameServerRequest.Data.Id);
        if (!stopResult.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = gameServerRequest.Data.Id,
                BuildVersionUpdated = false,
                Messages = stopResult.Messages
            });
            work.SendStatusUpdate(WeaverWorkState.Failed, stopResult.Messages);
            return;
        }

        var configUpdateRequest = await _gameServerService.UpdateConfigItemFiles(gameServerRequest.Data.Id);
        if (!configUpdateRequest.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = gameServerRequest.Data.Id,
                BuildVersionUpdated = false,
                Messages = configUpdateRequest.Messages
            });
            work.SendStatusUpdate(WeaverWorkState.Failed, configUpdateRequest.Messages);
            return;
        }

        await _gameServerService.UpdateState(gameServerRequest.Data.Id, ServerState.Shutdown);
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate
        {
            Id = deserializedGameServer.Id,
            BuildVersionUpdated = true,
            ServerState = ServerState.Shutdown,
            Resources = null
        });
    }

    private async Task ConfigureGameServerUpdate(WeaverWork work)
    {
        var deserializedServer = await ExtractGameServerFromWorkData(work);
        if (deserializedServer is null) return;

        var updateGameServerResponse = await _gameServerService.Update(deserializedServer.ToUpdate());
        if (!updateGameServerResponse.Succeeded)
        {
            _logger.Error("Failed to update gameserver configuration from work [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            work.SendStatusUpdate(WeaverWorkState.Failed, "Failed to update gameserver configuration");
            return;
        }

        var foundGameServer = await _gameServerService.GetById(deserializedServer.Id);
        if (!foundGameServer.Succeeded || foundGameServer.Data is null)
        {
            _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver id doesn't match an active gameserver this host manages");
            return;
        }

        var gameServerUpdated = foundGameServer.Data;
        var updatedResource = deserializedServer.Resources.First();

        var matchingResource = gameServerUpdated.Resources.FirstOrDefault(x => x.Id == updatedResource.Id);
        if (matchingResource is not null)
        {
            gameServerUpdated.Resources.Remove(matchingResource);
        }

        gameServerUpdated.Resources.Add(updatedResource);
        var configUpdateRequest = await _gameServerService.UpdateConfigItemFiles(gameServerUpdated.Id);
        if (!configUpdateRequest.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = foundGameServer.Data.Id,
                BuildVersionUpdated = false,
                Messages = configUpdateRequest.Messages
            });
            work.SendStatusUpdate(WeaverWorkState.Failed, configUpdateRequest.Messages);
            return;
        }

        var configurationHash = await GetConfigurationIntegrityHash(gameServerUpdated.Resources);
        gameServerUpdated.StorageConfigHash = configurationHash;
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate { Id = gameServerUpdated.Id, StorageConfigHash = configurationHash });

        await _gameServerService.UpdateState(gameServerUpdated.Id, gameServerUpdated.ServerState, configurationHash);

        work.SendStatusUpdate(WeaverWorkState.Completed, $"Gameserver [{gameServerUpdated.Id}] configuration updated");
    }

    private async Task ConfigureGameServerDelete(WeaverWork work)
    {
        var deserializedResource = await ExtractLocalResourceFromWorkData(work);
        if (deserializedResource is null) return;

        var foundGameServer = await _gameServerService.GetById(deserializedResource.GameserverId);
        if (!foundGameServer.Succeeded || foundGameServer.Data is null)
        {
            _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver id doesn't match an active gameserver this host manages");
            return;
        }

        var gameServerUpdated = foundGameServer.Data;

        var matchingResource = gameServerUpdated.Resources.FirstOrDefault(x => x.Id == deserializedResource.Id);
        if (matchingResource is not null)
        {
            gameServerUpdated.Resources.Remove(matchingResource);
        }

        var gameServerLocalUpdateRequest = await _gameServerService.Update(gameServerUpdated.ToUpdate());
        if (!gameServerLocalUpdateRequest.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = foundGameServer.Data.Id,
                BuildVersionUpdated = false,
                Messages = gameServerLocalUpdateRequest.Messages
            });
            work.SendStatusUpdate(WeaverWorkState.Failed, gameServerLocalUpdateRequest.Messages);
            return;
        }

        var configUpdateRequest = await _gameServerService.UpdateConfigItemFiles(gameServerUpdated.Id);
        if (!configUpdateRequest.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = foundGameServer.Data.Id,
                BuildVersionUpdated = false,
                Messages = configUpdateRequest.Messages
            });
            work.SendStatusUpdate(WeaverWorkState.Failed, configUpdateRequest.Messages);
            return;
        }

        var configurationHash = await GetConfigurationIntegrityHash(gameServerUpdated.Resources);
        gameServerUpdated.StorageConfigHash = configurationHash;
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate { Id = gameServerUpdated.Id, StorageConfigHash = configurationHash });

        await _gameServerService.UpdateState(gameServerUpdated.Id, gameServerUpdated.ServerState, configurationHash);

        work.SendStatusUpdate(WeaverWorkState.Completed, $"Gameserver [{gameServerUpdated.Id}] configuration updated");
    }

    private async Task ConfigureGameServerUpdateFull(WeaverWork work)
    {
        var deserializedServer = await ExtractGameServerFromWorkData(work);
        if (deserializedServer is null) return;

        var updateGameServerResponse = await _gameServerService.Update(deserializedServer.ToUpdate());
        if (!updateGameServerResponse.Succeeded)
        {
            _logger.Error("Failed to update gameserver configuration from work [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            work.SendStatusUpdate(WeaverWorkState.Failed, "Failed to update gameserver configuration");
            return;
        }

        var foundGameServer = await _gameServerService.GetById(deserializedServer.Id);
        if (!foundGameServer.Succeeded || foundGameServer.Data is null)
        {
            _logger.Error("Gameserver id provided doesn't match an active Gameserver? [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
            work.SendStatusUpdate(WeaverWorkState.Failed, "Gameserver id doesn't match an active gameserver this host manages");
            return;
        }

        var gameServerUpdated = foundGameServer.Data;
        gameServerUpdated.Resources = deserializedServer.Resources;

        var gameServerLocalUpdateRequest = await _gameServerService.Update(gameServerUpdated.ToUpdate());
        if (!gameServerLocalUpdateRequest.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = foundGameServer.Data.Id,
                BuildVersionUpdated = false,
                Messages = gameServerLocalUpdateRequest.Messages
            });
            work.SendStatusUpdate(WeaverWorkState.Failed, gameServerLocalUpdateRequest.Messages);
            return;
        }

        var configUpdateRequest = await _gameServerService.UpdateConfigItemFiles(gameServerUpdated.Id);
        if (!configUpdateRequest.Succeeded)
        {
            work.SendGameServerUpdate(WeaverWorkState.Failed, new GameServerStateUpdate
            {
                Id = foundGameServer.Data.Id,
                BuildVersionUpdated = false,
                Messages = configUpdateRequest.Messages
            });
            work.SendStatusUpdate(WeaverWorkState.Failed, configUpdateRequest.Messages);
            return;
        }

        var configurationHash = await GetConfigurationIntegrityHash(gameServerUpdated.Resources);
        gameServerUpdated.StorageConfigHash = configurationHash;
        work.SendGameServerUpdate(WeaverWorkState.Completed, new GameServerStateUpdate { Id = gameServerUpdated.Id, StorageConfigHash = configurationHash });

        await _gameServerService.UpdateState(gameServerUpdated.Id, gameServerUpdated.ServerState, configurationHash);

        work.SendStatusUpdate(WeaverWorkState.Completed, $"Gameserver [{gameServerUpdated.Id}] configuration updated");
    }
}