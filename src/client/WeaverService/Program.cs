using Infrastructure;
using WeaverService.Workers;

Host.CreateDefaultBuilder(args)
    .AddInfrastructure()
    .ConfigureServices(services =>
    {
        services.AddHostedService<HostWorker>();
        services.AddHostedService<ControlServerWorker>();
        services.AddHostedService<GameServerWorker>();
    })
    .Build()
    .Run();
    