
using Application.Helpers;
using Application.Services;

namespace WeaverService.Workers;

public class ServerBroker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServerService _serverService;

    private static DateTime _lastRuntime;

    public ServerBroker(ILogger logger, IServerService serverService)
    {
        _logger = logger;
        _serverService = serverService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(ServerBroker));
        ThreadHelper.ConfigureThreadPool(Environment.ProcessorCount, Environment.ProcessorCount * 2);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _lastRuntime = DateTime.Now;

            await ValidateServerStatus();

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
}