using Infrastructure;
using WeaverService.Workers;

Host.CreateDefaultBuilder(args)
    .AddInfrastructure()
    .ConfigureServices(services =>
    {
        services.AddHostedService<ControlServerBroker>();
        services.AddHostedService<HostBroker>();
    })
    .Build()
    .Run();
    