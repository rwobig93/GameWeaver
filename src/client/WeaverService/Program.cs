using WeaverService;

var builder = Host.CreateDefaultBuilder(args);

builder.AddInfrastructure()
    .Build()
    .Run();
    