
using System.Collections.Concurrent;
using Application.Constants;
using Application.Helpers;
using Application.Requests.Host;
using Application.Services;
using Application.Settings;
using Domain.Models;
using Microsoft.Extensions.Options;
using Serilog;

namespace WeaverService.Workers;

public class ControlServerBroker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IControlServerService _serverService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeneralConfiguration _generalConfig;

    private static DateTime _lastRuntime;
    private static readonly ConcurrentQueue<WeaverToServerMessage> WeaverOutQueue = new();

    public ControlServerBroker(ILogger logger, IControlServerService serverService, IHttpClientFactory httpClientFactory, IOptions<GeneralConfiguration> generalConfig)
    {
        _logger = logger;
        _serverService = serverService;
        _httpClientFactory = httpClientFactory;
        _generalConfig = generalConfig.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(ControlServerBroker));
        ThreadHelper.ConfigureThreadPool(Environment.ProcessorCount, Environment.ProcessorCount * 2);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _lastRuntime = DateTime.Now;

                await ValidateServerStatus();
                await _serverService.Checkin(new HostCheckInRequest());
                await SendOutQueueCommunication();

                var millisecondsPassed = (DateTime.Now - _lastRuntime).Milliseconds;
                if (millisecondsPassed < 1000)
                    await Task.Delay(1000 - millisecondsPassed, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure occurred during {ServiceName} execution loop", nameof(ControlServerBroker));
            }
        }
        
        _logger.Debug("Stopping {ServiceName} service", nameof(ControlServerBroker));
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

    public static void AddWeaverOutCommunication(WeaverToServerMessage message)
    {
        Log.Debug("Adding weaver outgoing communication: {MessageAction}", message.Action);
        WeaverOutQueue.Enqueue(message);
    }

    private async Task SendOutQueueCommunication()
    {
        if (!_serverService.RegisteredWithServer) { return; }
        
        if (!_serverService.ServerIsUp)
        {
            _logger.Warning("Server isn't up, skipping outgoing communication queue enumeration, current items waiting: {OutCommItemCount}", WeaverOutQueue.Count);
            return;
        }

        if (!_serverService.RegisteredWithServer)
        {
            _logger.Debug("Client isn't currently registered with the control server, skipping handling communication queue");
            return;
        }
        
        if (WeaverOutQueue.IsEmpty)
        {
            _logger.Verbose("Outgoing communication queue is empty, skipping...");
            return;
        }

        var runAttemptsLeft = _generalConfig.QueueMaxPerRun;

        while (runAttemptsLeft > 0 && !WeaverOutQueue.IsEmpty)
        {
            runAttemptsLeft -= 1;
            if (!WeaverOutQueue.TryDequeue(out var message)) continue;
            
            _logger.Debug("Sending outgoing communication => {MessageAction}", message.Action);

            // TODO: Add real endpoint from server service to send outgoing communications
            var response = await _serverService.CheckIfServerIsUp();
            if (response)
            {
                _logger.Debug("Server successfully processed outgoing communication: {MessageAction}", message.Action);
                continue;
            }

            if (message.AttemptCount >= _generalConfig.MaxQueueAttempts)
            {
                _logger.Warning("Maximum attempts reached for outgoing communication, dropping: {AttemptCount} {MessageAction}",
                    message.AttemptCount, message.Action);
                continue;
            }
            
            _logger.Error("Got a failure response from outgoing communication, re-queueing: [{MessageAction}]", message.Action);
            message.AttemptCount += 1;
            AddWeaverOutCommunication(message);
        }
        
        _logger.Debug("Finished parsing outgoing weaver communication queue, current items waiting: {OutCommItemCount}", WeaverOutQueue.Count);
    }
}