using Serilog;
using WeaverService.Settings;

namespace WeaverService;

public static class ServiceConfiguration
{
    public static IHostBuilder AddInfrastructure(this IHostBuilder builder)
    {
        // Replace default logger w/ Serilog, configure via appsettings.json - uses the "Serilog" section
        builder.UseSerilog((ctx, logConfig) => 
            logConfig.ReadFrom.Configuration(ctx.Configuration), preserveStaticLogger: false);

        builder.ConfigureServices((context, services) =>
            {
                services.AddServiceSettings(context.Configuration);
                services.AddSystemServices();
            }
        );

        return builder;
    }

    private static void AddSystemServices(this IServiceCollection services)
    {
        services.AddHostedService<Worker>();
    }

    private static void AddServiceSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AppConfiguration>()
            .Bind(configuration.GetRequiredSection(AppConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}