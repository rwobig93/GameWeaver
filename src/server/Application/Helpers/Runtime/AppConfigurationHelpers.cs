using Application.Settings.AppSettings;
using Microsoft.Extensions.Configuration;

namespace Application.Helpers.Runtime;

public static class AppConfigurationHelpers
{
    // Configuring a class to bind to settings allows you to call it through dependency injection
    //   doing something like this: IOptions<AppConfiguration> appConfig.Value or AppConfiguration like a service
    //
    // These are just helpers for Dependency Injection usage, after app.Run() is called on startup configuration sections
    // should be retrieved using IOptions<TypeConfiguration> as a service injection

    public static AppConfiguration GetApplicationSettings(this IConfiguration configuration)
    {
        return configuration.GetSection(AppConfiguration.SectionName).Get<AppConfiguration>()!;
    }

    public static MailConfiguration GetMailSettings(this IConfiguration configuration)
    {
        return configuration.GetSection(MailConfiguration.SectionName).Get<MailConfiguration>()!;
    }

    public static DatabaseConfiguration GetDatabaseSettings(this IConfiguration configuration)
    {
        return configuration.GetSection(DatabaseConfiguration.SectionName).Get<DatabaseConfiguration>()!;
    }

    public static LifecycleConfiguration GetLifecycleSettings(this IConfiguration configuration)
    {
        return configuration.GetSection(LifecycleConfiguration.SectionName).Get<LifecycleConfiguration>()!;
    }

    public static SecurityConfiguration GetSecuritySettings(this IConfiguration configuration)
    {
        return configuration.GetSection(SecurityConfiguration.SectionName).Get<SecurityConfiguration>()!;
    }

    public static OauthConfiguration GetOauthSettings(this IConfiguration configuration)
    {
        return configuration.GetSection(OauthConfiguration.SectionName).Get<OauthConfiguration>()!;
    }
}