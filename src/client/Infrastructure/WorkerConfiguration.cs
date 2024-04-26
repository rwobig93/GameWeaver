using Application.Constants;
using Application.Repositories;
using Application.Services;
using Application.Settings;
using Infrastructure.Handlers;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Infrastructure;

public static class WorkerConfiguration
{
    public static IHostBuilder AddInfrastructure(this IHostBuilder builder)
    {
        // Replace default logger w/ Serilog, configure via appsettings.json - uses the "Serilog" section
        builder.UseSerilog((ctx, logConfig) => 
            logConfig.ReadFrom.Configuration(ctx.Configuration), preserveStaticLogger: false);

        builder.ConfigureServices((context, services) =>
            {
                services.AddSettings(context.Configuration);
                services.AddClientServices(context.Configuration);
                services.AddHostedRepositories(context.Configuration);
                services.AddHostedServices(context.Configuration);
            }
        );

        return builder;
    }

    private static void AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GeneralConfiguration>()
            .Bind(configuration.GetRequiredSection(GeneralConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<AuthConfiguration>()
            .Bind(configuration.GetRequiredSection(AuthConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddHostedRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IGameServerRepository, GameServerRepositoryMemoryJson>();
        services.AddSingleton<IWeaverWorkRepository, WeaverWorkRepositoryMemoryJson>();
    }

    private static void AddHostedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IControlServerService, ControlServerService>();
        services.AddSingleton<IHostService, HostService>();
        services.AddSingleton<IGameServerService, GameServerService>();
        services.AddSingleton<IWeaverWorkService, WeaverWorkService>();
    }

    private static void AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        var generalConfig = configuration.GetRequiredSection(GeneralConfiguration.SectionName).Get<GeneralConfiguration>();
        services.AddTransient<AuthTokenDelegatingHandler>();
        
        services.AddHttpClient(HttpConstants.Unauthenticated, client => { client.BaseAddress = new Uri(generalConfig!.ServerUrl.Trim('/')); });
        services.AddHttpClient(HttpConstants.AuthenticatedServer, client => { client.BaseAddress = new Uri(generalConfig!.ServerUrl.Trim('/')); })
            .AddHttpMessageHandler<AuthTokenDelegatingHandler>();

        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<ISerializerService, SerializerService>();
    }
}