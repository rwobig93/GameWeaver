
using System.Collections.Concurrent;
using Application.Constants;
using Application.Helpers;
using Application.Services;
using Application.Settings;
using Domain.Models;
using Microsoft.Extensions.Options;
using Serilog;

namespace WeaverService.Workers;

public class ServerBroker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServerService _serverService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeneralConfiguration _generalConfig;

    private static DateTime _lastRuntime;
    private static readonly ConcurrentQueue<WeaverToServerMessage> WeaverOutQueue = new();

    public ServerBroker(ILogger logger, IServerService serverService, IHttpClientFactory httpClientFactory, IOptions<GeneralConfiguration> generalConfig)
    {
        _logger = logger;
        _serverService = serverService;
        _httpClientFactory = httpClientFactory;
        _generalConfig = generalConfig.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(ServerBroker));
        ThreadHelper.ConfigureThreadPool(Environment.ProcessorCount, Environment.ProcessorCount * 2);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _lastRuntime = DateTime.Now;

            await ValidateServerStatus();
            await SendOutQueueCommunication();

            var millisecondsPassed = (DateTime.Now - _lastRuntime).Milliseconds;
            if (millisecondsPassed < 1000)
                await Task.Delay(1000 - millisecondsPassed, stoppingToken);
        }
        
        _logger.Debug("Stopping {ServiceName} service", nameof(ServerBroker));
    }

    private async Task ValidateServerStatus()
    {
        var previousServerStatus = _serverService.ServerIsUp;
        var serverIsUp = await _serverService.CheckIfServerIsUp();
        
        if (previousServerStatus != serverIsUp)
            _logger.Warning("Server connectivity status changed, server connectivity is now: {ServerStatus}", serverIsUp);
        
        _logger.Debug("Server status is: {ServerStatus}", serverIsUp);
    }

    public static void AddWeaverOutCommunication(WeaverToServerMessage message)
    {
        Log.Debug("Adding weaver outgoing communication: {MessageAction}", message.Action);
        WeaverOutQueue.Enqueue(message);
    }

    private async Task SendOutQueueCommunication()
    {
        if (!_serverService.ServerIsUp)
        {
            _logger.Information("Server isn't up, skipping outgoing communication queue enumeration, current items waiting: {OutCommItemCount}", WeaverOutQueue.Count);
            return;
        }
        
        if (WeaverOutQueue.IsEmpty)
        {
            _logger.Verbose("Outgoing communication queue is empty, skipping...");
            return;
        }

        // TODO: Handle auth, renew token when old token expires | If expiration is within 5 seconds or more then get a new token
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);

        var runAttemptsLeft = _generalConfig.QueueMaxPerRun;

        while (runAttemptsLeft > 0 && !WeaverOutQueue.IsEmpty)
        {
            runAttemptsLeft -= 1;
            if (!WeaverOutQueue.TryDequeue(out var message)) continue;
            
            _logger.Debug("Sending outgoing communication => {MessageAction}", message.Action);
            
            var response = await httpClient.GetAsync("/uri");
            if (response.IsSuccessStatusCode)
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
            
            _logger.Error("Got a failure response from outgoing communication, re-queueing: [{MessageAction}] => {StatusCode} {ErrorMessage}", 
                message.Action, response.StatusCode, await response.Content.ReadAsStringAsync());
            message.AttemptCount += 1;
            AddWeaverOutCommunication(message);
        }
        
        _logger.Debug("Finished parsing outgoing weaver communication queue, current items waiting: {OutCommItemCount}", WeaverOutQueue.Count);
    }
}