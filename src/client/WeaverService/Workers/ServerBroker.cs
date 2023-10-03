
using System.Collections.Concurrent;
using Application.Constants;
using Application.Helpers;
using Application.Services;
using Domain.Models;
using Serilog;

namespace WeaverService.Workers;

public class ServerBroker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServerService _serverService;
    private readonly IHttpClientFactory _httpClientFactory;

    private static DateTime _lastRuntime;
    private static readonly ConcurrentQueue<WeaverCommunication> WeaverOutQueue = new();

    public ServerBroker(ILogger logger, IServerService serverService, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _serverService = serverService;
        _httpClientFactory = httpClientFactory;
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

    public static void AddWeaverOutCommunication(WeaverCommunication communication)
    {
        Log.Debug("Adding weaver outgoing communication: {WeaverCommAction}", communication.Action);
        WeaverOutQueue.Enqueue(communication);
    }

    private async Task SendOutQueueCommunication()
    {
        if (!_serverService.ServerIsUp)
        {
            _logger.Debug("Server isn't up, skipping outgoing communication queue enumeration, current items waiting: {OutCommItemCount}", WeaverOutQueue.Count);
            return;
        }
        
        if (WeaverOutQueue.IsEmpty)
            return;

        // TODO: Handle auth, renew token when old token expires
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        
        Parallel.For(0, 5, _ =>
        {
            if (!WeaverOutQueue.TryDequeue(out var communication)) return;
            
            // TODO: Handle properly, enumerate queue w/ a max (like 5) per run
            _logger.Information("Sending outgoing communication => {WeaverCommAction}", communication.Action);
            httpClient.GetAsync("/uri");
        });
        
        await Task.CompletedTask;
        _logger.Debug("Finished parsing outgoing weaver communication queue, current items waiting: {OutCommItemCount}", WeaverOutQueue.Count);
    }
}