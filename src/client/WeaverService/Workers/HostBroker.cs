using System.Collections.Concurrent;
using Application.Requests.Host;
using Domain.Models.ControlServer;

namespace WeaverService.Workers;

public class HostBroker : BackgroundService
{
    private readonly ILogger _logger;

    private static readonly ConcurrentQueue<WeaverWorkClient> WorkInProgressQueue = new();
    private static readonly ConcurrentQueue<WeaverWorkClient> WorkWaitingQueue = new();
    
    public static float CpuUsage { get; private set; } = 0;
    public static float RamUsage { get; private set; } = 0;
    public static float Uptime { get; private set; } = 0;
    public static int NetworkOutMb { get; private set; } = 0;
    public static int NetworkInMb { get; private set; } = 0;
    
    public HostBroker(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(HostBroker));
        
        // TODO: Add folder structure and dependency enforcement like SteamCMD before jumping into the execution loop
        
        // TODO: Handle WeaverToServer message when resource status changes occur
        ControlServerBroker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest());
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.CompletedTask;
            _logger.Debug("Placeholder");
            await Task.Delay(10000, stoppingToken);
        }
        
        _logger.Debug("Stopping {ServiceName} service", nameof(HostBroker));
    }
}