
namespace WeaverService.Workers;

public class ServerBroker : BackgroundService
{
    private readonly ILogger _logger;

    public ServerBroker(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(ServerBroker));
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.Information("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
        
        _logger.Debug("Stopping {ServiceName} service", nameof(ServerBroker));
    }
}