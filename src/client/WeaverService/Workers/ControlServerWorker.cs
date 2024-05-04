
using System.Collections.Concurrent;
using Application.Requests.Host;
using Application.Services;
using Application.Settings;
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
            await _serverService.RegistrationConfirm();
        
        _logger.Verbose("Control Server is up: {ServerStatus}", serverIsUp);
    }

    private async Task CheckInWithControlServer()
    {
        if (!_serverService.ServerIsUp || !_serverService.RegisteredWithServer) { return; }
        
        var currentResourceUsage = HostWorker.CurrentHostResourceUsage;
        
        if (currentResourceUsage is {CpuUsage: 0, RamUsage: 0, Uptime: 0})
            return;
        
        var checkInRequest = new HostCheckInRequest
        {
            SendTimestamp = _dateTimeService.NowDatabaseTime,
            CpuUsage = currentResourceUsage.CpuUsage,
            RamUsage = currentResourceUsage.RamUsage,
            Uptime = currentResourceUsage.Uptime,
            NetworkOutMb = currentResourceUsage.NetworkOutBytes,
            NetworkInMb = currentResourceUsage.NetworkInBytes
        };
        
        var checkInResponse = await _serverService.Checkin(checkInRequest);
        if (!checkInResponse.Succeeded)
        {
            _logger.Error("Failed to check in with the control server: {Error}", checkInResponse.Messages);
            return;
        }
        if (checkInResponse.Data is null) return;

        foreach (var work in checkInResponse.Data)
        {
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
            if (!WorkUpdateQueue.TryDequeue(out var message)) continue;
            
            _logger.Debug("Sending outgoing communication => {WorkId}", message.Id);
            
            var response = await _serverService.WorkStatusUpdate(message);
            if (response.Succeeded)
            {
                _logger.Debug("Server successfully processed outgoing communication: {WorkId}", message.Id);
                continue;
            }

            if (message.AttemptCount >= _generalConfig.Value.MaxQueueAttempts)
            {
                _logger.Warning("Maximum attempts reached for outgoing communication, dropping: {AttemptCount} {WorkId}",
                    message.AttemptCount, message.Id);
                continue;
            }
            
            _logger.Error("Got a failure response from outgoing communication, re-queueing: [{WorkId}]", message.Id);
            message.AttemptCount += 1;
            AddWeaverWorkUpdate(message);
        }
        
        _logger.Debug("Finished parsing outgoing weaver communication queue, current items waiting: {OutCommItemCount}", WorkUpdateQueue.Count);
    }
}