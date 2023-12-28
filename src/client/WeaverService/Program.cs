using Infrastructure;
using WeaverService.Workers;

Host.CreateDefaultBuilder(args)
    .AddInfrastructure()
    .ConfigureServices(services =>
    {
        services.AddHostedService<ControlServerWorker>();
        services.AddHostedService<HostWorker>();
    })
    .Build()
    .Run();
    