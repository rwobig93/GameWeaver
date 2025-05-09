
using System.Collections.Concurrent;
using Application.Requests.Host;
using Application.Services;
using Application.Settings;
using Domain.Enums;
using Domain.Models.ControlServer;
using Microsoft.Extensions.Options;
using Serilog;

namespace WeaverService.Workers;

public class ControlServerWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IControlServerService _serverService;
    private readonly IOptions<GeneralConfiguration> _generalConfig;
    private readonly IDateTimeService _dateTimeService;
    private readonly IWeaverWorkService _weaverWorkService;

    /// <summary>
    /// Handles control server communication
    /// </summary>
    public ControlServerWorker(ILogger logger, IControlServerService serverService, IOptions<GeneralConfiguration> generalConfig, IDateTimeService dateTimeService,
        IWeaverWorkService weaverWorkService)
    {
        _logger = logger;
        _serverService = serverService;
        _generalConfig = generalConfig;
        _dateTimeService = dateTimeService;
        _weaverWorkService = weaverWorkService;
    }

    private static DateTime _lastRuntime;
    private static readonly ConcurrentQueue<WeaverWorkUpdateRequest> WorkUpdateQueue = new();

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(ControlServerWorker));
        await Task.CompletedTask;
        await base.StartAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _lastRuntime = _dateTimeService.NowDatabaseTime;

                await ValidateServerStatus();
                await CheckInWithControlServer();
                await SendOutQueueCommunication();

                var millisecondsPassed = (_dateTimeService.NowDatabaseTime - _lastRuntime).Milliseconds;
                if (millisecondsPassed < _generalConfig.Value.ControlServerWorkIntervalMs)
                    await Task.Delay(_generalConfig.Value.ControlServerWorkIntervalMs - millisecondsPassed, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure occurred during {ServiceName} execution loop: {ErrorMessage}",
                    nameof(ControlServerWorker), ex.Message);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Stopped {ServiceName} service", nameof(ControlServerWorker));
        await Task.CompletedTask;
        await base.StopAsync(stoppingToken);
    }

    private async Task ValidateServerStatus()
    {
        var previousServerStatus = _serverService.ServerIsUp;
        var serverIsUp = await _serverService.CheckIfServerIsUp();

        if (previousServerStatus != serverIsUp)
            _logger.Information("Server connectivity status changed, server connectivity is now: {ServerStatus}", serverIsUp);

        if (_serverService is {ServerIsUp: true, RegisteredWithServer: false})
        {
            var registerResponse = await _serverService.RegistrationConfirm();

            // On a new registration we need to let the control server know about our host, same as on startup
            if (!string.IsNullOrWhiteSpace(registerResponse.Data?.Data.HostToken) && registerResponse.Data.Data.HostId != Guid.Empty)
            {
                // Registration complete, need to wait a bit for the other workers to catch up
                await Task.Delay(2000);
                await _weaverWorkService.CreateAsync(new WeaverWork
                {
                    Id = 0,
                    TargetType = WeaverWorkTarget.HostDetail,
                    Status = WeaverWorkState.WaitingToBePickedUp,
                    WorkData = null
                });
            }

            // Waiting on successful registration, we'll add a delay to not hammer the server waiting
            await Task.Delay(5000);
        }

        _logger.Verbose("Control Server is up: {ServerStatus}", serverIsUp);
    }

    private async Task CheckInWithControlServer()
    {
        if (!_serverService.ServerIsUp || !_serverService.RegisteredWithServer) { return; }

        var currentResourceUsage = HostWorker.CurrentHostResourceUsage;

        if (currentResourceUsage.CpuUsage == 0 || currentResourceUsage.RamUsage == 0 || currentResourceUsage.Uptime == 0)
            return;

        var checkInRequest = new HostCheckInRequest
        {
            SendTimestamp = _dateTimeService.NowDatabaseTime,
            CpuUsage = currentResourceUsage.CpuUsage,
            RamUsage = currentResourceUsage.RamUsage,
            Uptime = currentResourceUsage.Uptime,
            NetworkOutBytes = currentResourceUsage.NetworkOutBytes,
            NetworkInBytes = currentResourceUsage.NetworkInBytes
        };

        var checkInResponse = await _serverService.Checkin(checkInRequest);
        if (!checkInResponse.Succeeded)
        {
            _logger.Error("Failed to check in with the control server: {Error}", checkInResponse.Messages);
            return;
        }
        if (checkInResponse.Data is null) return;

        foreach (var work in checkInResponse.Data.ToList())
        {
            var matchingWorkRequest = await _weaverWorkService.GetByIdAsync(work.Id);
            if (matchingWorkRequest.Data is not null)
            {
                _logger.Verbose("Already picked up work was sent again: [{WeaverworkId}]", work.Id);
                continue;
            }

            var createRequest = await _weaverWorkService.CreateAsync(work);
            if (!createRequest.Succeeded)
            {
                _logger.Error("Failure occurred creating weaver work: {Error}", createRequest.Messages);
                continue;
            }

            _logger.Verbose("Successfully added weaver work: [{WeaverworkId}]{WeaverworkTarget}", work.Id, work.TargetType);
        }

        _logger.Verbose("Successfully checked in with the control server");
    }

    public static void AddWeaverWorkUpdate(WeaverWorkUpdateRequest request)
    {
        Log.Debug("Adding weaver work update: [{WorkId}]{WorkStatus}", request.Id, request.Status);
        WorkUpdateQueue.Enqueue(request);
    }

    private async Task SendOutQueueCommunication()
    {
        if (!_serverService.RegisteredWithServer) { return; }

        if (WorkUpdateQueue.IsEmpty)
        {
            _logger.Verbose("Outgoing communication queue is empty, skipping...");
            return;
        }

        if (!_serverService.ServerIsUp)
        {
            _logger.Warning("Server isn't up, skipping outgoing communication queue enumeration, current items waiting: {OutCommItemCount}", WorkUpdateQueue.Count);
            return;
        }

        var runAttemptsLeft = _generalConfig.Value.CommunicationQueueMaxPerRun;

        while (runAttemptsLeft > 0 && !WorkUpdateQueue.IsEmpty)
        {
            runAttemptsLeft -= 1;
            if (!WorkUpdateQueue.TryDequeue(out var work)) continue;

            _logger.Debug("Sending outgoing communication => {WorkId}", work.Id);

            var response = await _serverService.WorkStatusUpdate(work);
            if (response.Succeeded)
            {
                if (work.Id == 0)
                {
                    // GameServerStateUpdate from realtime status checks won't be bound to work, so we'll skip the internal update
                    continue;
                }

                var localUpdateRequest = await _weaverWorkService.UpdateStatusAsync(work.Id, work.Status);
                if (!localUpdateRequest.Succeeded)
                {
                    foreach (var message in localUpdateRequest.Messages)
                    {
                        _logger.Error("Failure updating local weaver work status occurred: [{Weaverworkid}]{Error}", work.Id, message);
                    }
                }

                _logger.Debug("Server successfully processed outgoing communication: {WorkId}", work.Id);
                continue;
            }

            if (work.AttemptCount >= _generalConfig.Value.MaxQueueAttempts)
            {
                _logger.Warning("Maximum attempts reached for outgoing communication, dropping: {AttemptCount} {WorkId}",
                    work.AttemptCount, work.Id);
                continue;
            }

            _logger.Error("Got a failure response from outgoing communication, re-queueing: [{WorkId}]", work.Id);
            work.AttemptCount += 1;
            AddWeaverWorkUpdate(work);
        }

        _logger.Debug("Finished parsing outgoing weaver communication queue, current items waiting: {OutCommItemCount}", WorkUpdateQueue.Count);
    }
}