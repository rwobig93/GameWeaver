
using System.Collections.Concurrent;
using Application.Helpers;
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

    private static DateTime _lastRuntime;
    private static readonly ConcurrentQueue<WeaverWorkUpdateRequest> WorkUpdateQueue = new();

    public ControlServerWorker(ILogger logger, IControlServerService serverService, IOptions<GeneralConfiguration> generalConfig)
    {
        _logger = logger;
        _serverService = serverService;
        _generalConfig = generalConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(ControlServerWorker));
        ThreadHelper.ConfigureThreadPool(Environment.ProcessorCount, Environment.ProcessorCount * 2);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _lastRuntime = DateTime.Now;

                await ValidateServerStatus();
                // TODO: Get host and resource telemetry and finish full checkin
                await _serverService.Checkin(new HostCheckInRequest());
                await SendOutQueueCommunication();

                var millisecondsPassed = (DateTime.Now - _lastRuntime).Milliseconds;
                if (millisecondsPassed < 1000)
                    await Task.Delay(1000 - millisecondsPassed, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure occurred during {ServiceName} execution loop", nameof(ControlServerWorker));
            }
        }
        
        _logger.Debug("Stopping {ServiceName} service", nameof(ControlServerWorker));
    }

    private async Task ValidateServerStatus()
    {
        var previousServerStatus = _serverService.ServerIsUp;
        var serverIsUp = await _serverService.CheckIfServerIsUp();
        
        if (previousServerStatus != serverIsUp)
            _logger.Information("Server connectivity status changed, server connectivity is now: {ServerStatus}", serverIsUp);

        if (_serverService is {ServerIsUp: true, RegisteredWithServer: false})
            await _serverService.RegistrationConfirm();
        
        _logger.Debug("Control Server is up: {ServerStatus}", serverIsUp);
    }

    public static void AddWeaverWorkUpdate(WeaverWorkUpdateRequest request)
    {
        Log.Debug("Adding weaver work update: [{WorkId}]{WorkStatus}", request.Id, request.Status);
        WorkUpdateQueue.Enqueue(request);
    }

    private async Task SendOutQueueCommunication()
    {
        if (!_serverService.RegisteredWithServer) { return; }
        
        if (!_serverService.ServerIsUp)
        {
            _logger.Warning("Server isn't up, skipping outgoing communication queue enumeration, current items waiting: {OutCommItemCount}", WorkUpdateQueue.Count);
            return;
        }
        
        if (WorkUpdateQueue.IsEmpty)
        {
            _logger.Verbose("Outgoing communication queue is empty, skipping...");
            return;
        }

        var runAttemptsLeft = _generalConfig.Value.QueueMaxPerRun;

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