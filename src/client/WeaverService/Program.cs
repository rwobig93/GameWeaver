using Infrastructure;
using Serilog;
using WeaverService.Workers;

var host = Host.CreateDefaultBuilder(args)
    .AddInfrastructure()
    .ConfigureServices(services =>
    {
        services.AddHostedService<HostWorker>();
        services.AddHostedService<ControlServerWorker>();
        services.AddHostedService<GameServerWorker>();
    })
    .Build();

    // Attempt to force worker cleanup if the service gracefully
    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
    {
        Log.Information("Weaver Service is exiting and attempting to cleanup workers");

        var hostWorker = host.Services.GetService<HostWorker>();
        hostWorker?.StopAsync(new CancellationToken(true));

        var gameServerWorker = host.Services.GetService<GameServerWorker>();
        gameServerWorker?.StopAsync(new CancellationToken(true));

        Log.Information("Finished Weaver Service cleanup, exiting");
    };

    host.Run();
    