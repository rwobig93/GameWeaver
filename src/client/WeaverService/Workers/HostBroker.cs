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
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.CompletedTask;
            _logger.Debug("Placeholder");
            await Task.Delay(1000, stoppingToken);
        }
        
        _logger.Debug("Stopping {ServiceName} service", nameof(HostBroker));
    }
}