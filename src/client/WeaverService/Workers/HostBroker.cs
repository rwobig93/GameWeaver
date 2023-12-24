using Domain.Models.ControlServer;

namespace WeaverService.Workers;

public class HostBroker : BackgroundService
{
    private readonly ILogger _logger;

    public HostBroker(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(HostBroker));
        
        // TODO: Add folder structure and dependency enforcement like SteamCMD before jumping into the execution loop
        
        // TODO: Handle WeaverToServer message when resource status changes occur
        ControlServerBroker.AddWeaverOutCommunication(new WeaverToServerMessage());
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.CompletedTask;
            _logger.Debug("Placeholder");
            await Task.Delay(10000, stoppingToken);
        }
        
        _logger.Debug("Stopping {ServiceName} service", nameof(HostBroker));
    }
}