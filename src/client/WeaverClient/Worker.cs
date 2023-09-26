using WeaverClient.Interactivity;

namespace WeaverClient;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        WeaverConsole.StartConsole();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
            _logger.LogInformation("Last console message: {Message}", WeaverConsole.Reader?.ReadLine());
        }
        
        WeaverConsole.StopConsole();
    }
}