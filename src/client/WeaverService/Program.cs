using Infrastructure;
using WeaverService.Workers;

Host.CreateDefaultBuilder(args)
    .AddInfrastructure()
    .ConfigureServices(services =>
    {
        services.AddHostedService<ServerBroker>();
        services.AddHostedService<HostBroker>();
    })
    .Build()
    .Run();
    