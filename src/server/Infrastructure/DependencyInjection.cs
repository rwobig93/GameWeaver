using System.Net;
using System.Text.Json.Serialization;
using Application.Auth;
using Application.Constants.Communication;
using Application.Constants.Web;
using Application.Helpers.Auth;
using Application.Helpers.Runtime;
using Application.Repositories.GameServer;
using Application.Repositories.Identity;
using Application.Repositories.Integrations;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.External;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Services.Integrations;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Asp.Versioning;
using Blazored.LocalStorage;
using Domain.Contracts;
using Domain.Enums.Database;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.HealthChecks;
using Infrastructure.Repositories.MsSql.GameServer;
using Infrastructure.Repositories.MsSql.Identity;
using Infrastructure.Repositories.MsSql.Integrations;
using Infrastructure.Repositories.MsSql.Lifecycle;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Database;
using Infrastructure.Services.External;
using Infrastructure.Services.GameServer;
using Infrastructure.Services.Identity;
using Infrastructure.Services.Integrations;
using Infrastructure.Services.Lifecycle;
using Infrastructure.Services.System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using MudBlazor.Services;
using Newtonsoft.Json;

namespace Infrastructure;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        // Replace default logger w/ Serilog, configure via appsettings.json - uses the "Serilog" section
        builder.Host.UseSerilog((ctx, logConfig) => 
            logConfig.ReadFrom.Configuration(ctx.Configuration), preserveStaticLogger: false);
        
        builder.Services.AddBlazorServerCommon();
        builder.Services.AddSettingsConfiguration(builder.Configuration);
        builder.Services.AddSystemServices(builder.Configuration);

        builder.Services.AddApplicationServices();

        builder.Services.AddApiServices();
        builder.Services.AddAuthServices(builder.Configuration);
        builder.Services.AddDatabaseServices(builder.Configuration);

        return builder;
    }

    private static void AddBlazorServerCommon(this IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor();
    }

    private static void AddSettingsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AppConfiguration>()
            .Bind(configuration.GetRequiredSection(AppConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<DatabaseConfiguration>()
            .Bind(configuration.GetRequiredSection(DatabaseConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<MailConfiguration>()
            .Bind(configuration.GetSection(MailConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<LifecycleConfiguration>()
            .Bind(configuration.GetSection(LifecycleConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<SecurityConfiguration>()
            .Bind(configuration.GetSection(SecurityConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<OauthConfiguration>()
            .Bind(configuration.GetSection(OauthConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddSystemServices(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSettings = configuration.GetDatabaseSettings();
        
        services.AddHangfire(x =>
        {
            x.UseSerilogLogProvider();
            switch (databaseSettings.Provider)
            {
                case DatabaseProviderType.MsSql:
                    x.UseSqlServerStorage(databaseSettings.MsSql);
                    break;
                case DatabaseProviderType.Postgresql:
                    // Need to add database support for application before we can fully support Postgresql
                    x.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(databaseSettings.Postgres));
                    throw new Exception("Postgres Database Provider isn't supported, please enter a supported provider in appsettings.json!");
                case DatabaseProviderType.Sqlite:
                    // Need to add database support for application before we can fully support Sqlite
                    x.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(databaseSettings.Sqlite));
                    throw new Exception("Sqlite Database Provider isn't supported, please enter a supported provider in appsettings.json!");
                default:
                    throw new Exception("Configured Database Provider isn't supported, please enter a supported provider in appsettings.json!");
            }
        });
        services.AddHangfireServer();
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 6000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
        });
        
        services.AddBlazoredLocalStorage();
        services.AddHttpClient(ApiConstants.Clients.GameWeaverDefault, options =>
        {
            options.BaseAddress = new Uri(configuration.GetApplicationSettings().BaseUrl);
        }).ConfigureCertificateHandling(configuration);
        services.AddHttpClient(ApiConstants.Clients.GeneralWeb).ConfigureCertificateHandling(configuration);
        services.AddHttpClient(ApiConstants.Clients.SteamApiNetUnauthenticated, options =>
        {
            options.BaseAddress = new Uri(ApiConstants.Steam.BaseUrlApiNet);
        }).ConfigureCertificateHandling(configuration);
        services.AddHttpClient(ApiConstants.Clients.SteamApiPoweredComUnauthenticated, options =>
        {
            options.BaseAddress = new Uri(ApiConstants.Steam.BaseUrlApiPoweredCom);
        }).ConfigureCertificateHandling(configuration);
        services.AddHttpClient(ApiConstants.Clients.SteamStoreUnauthenticated, options =>
        {
            options.BaseAddress = new Uri(ApiConstants.Steam.BaseUrlStore);
        }).ConfigureCertificateHandling(configuration);

        var mailConfig = configuration.GetMailSettings();
        services.AddFluentEmail(mailConfig.From, mailConfig.DisplayName)
            .AddRazorRenderer().AddSmtpSender(mailConfig.Host, mailConfig.Port, mailConfig.UserName, mailConfig.Password);

        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IRunningServerState, RunningServerState>();
        services.AddSingleton<ISerializerService, SerializerService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddScoped<IWebClientService, WebClientService>();
    }

    private static void ConfigureCertificateHandling(this IHttpClientBuilder httpClientBuilder, IConfiguration configuration)
    {
        if (!configuration.GetSecuritySettings().TrustAllCertificates)
        {
            return;
        }
        
        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
        });
    }

    private static void AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        var securitySettings = configuration.GetSecuritySettings();

        services.AddHttpContextAccessor();
        services.AddSingleton<IAppUserService, AppUserService>();
        services.AddSingleton<IAppRoleService, AppRoleService>();
        services.AddSingleton<IAppPermissionService, AppPermissionService>();
        
        services.AddScoped<AuthStateProvider>();
        services.AddTransient<AuthenticationStateProvider, AuthStateProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAppAccountService, AppAccountService>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, DynamicAuthorizationHandler>();

        services.AddSingleton<IExternalAuthProviderService, ExternalAuthProviderService>();
        
        services.AddJwtAuthentication(configuration);
        services.AddAuthorization();
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromSeconds(securitySettings.SecurityStampValidationIntervalMinutes);
        });
    }

    private static void AddApplicationServices(this IServiceCollection services)
    {
        // Lifecycle Services
        services.AddSingleton<IAuditTrailService, AuditTrailService>();
        services.AddSingleton<ITroubleshootingRecordService, TroubleshootingRecordService>();
        services.AddSingleton<INotifyRecordService, NotifyRecordService>();

        // Integration Services
        services.AddSingleton<IExcelService, ExcelService>();
        services.AddTransient<IEmailService, EmailService>();
        
        // System Services
        services.AddSingleton<IFileStorageRecordService, FileStorageRecordService>();
        
        // Web Service Services
        services.AddTransient<IMfaService, MfaService>();
        services.AddTransient<IQrCodeService, QrCodeService>();
        services.AddTransient<IJobManager, JobManager>();

        // Game Server Orchestration Services
        services.AddSingleton<IHostService, HostService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IGameServerService, GameServerService>();
        services.AddSingleton<INetworkService, NetworkService>();
        
        // External Services
        services.AddSingleton<ISteamApiService, SteamApiService>();
    }

    private static void AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        
        services.AddApiVersioning(c =>
        {
            c.AssumeDefaultVersionWhenUnspecified = true;
            c.DefaultApiVersion = new ApiVersion(1);
            c.ReportApiVersions = true;
            c.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-Version"),
                new MediaTypeApiVersionReader("ver"),
                new UrlSegmentApiVersionReader());
        });
        
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database");
    }

    private static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add core SQL database service
        services.AddSingleton<ISqlDataService, SqlDataService>();
        
        // Add repositories based on the desired database provider
        var databaseProvider = configuration.GetDatabaseSettings().Provider;
        switch (databaseProvider)
        {
            case DatabaseProviderType.MsSql:
                // System Database Repositories
                services.AddSingleton<IAppUserRepository, AppUserRepositoryMsSql>();
                services.AddSingleton<IAppRoleRepository, AppRoleRepositoryMsSql>();
                services.AddSingleton<IAppPermissionRepository, AppPermissionRepositoryMsSql>();
                services.AddSingleton<IAuditTrailsRepository, AuditTrailsRepositoryMsSql>();
                services.AddSingleton<IServerStateRecordsRepository, ServerStateRecordsRepositoryMsSql>();
                services.AddSingleton<INotifyRecordRepository, NotifyRecordRepositoryMsSql>();
                services.AddSingleton<ITroubleshootingRecordsRepository, TroubleshootingRecordsRepositoryMsSql>();
                services.AddSingleton<IFileStorageRecordRepository, FileStorageRecordRepositoryMsSql>();
                // GameServer Database Repositories
                services.AddSingleton<IHostRepository, HostRepositoryMsSql>();
                services.AddSingleton<IGameRepository, GameRepositoryMsSql>();
                services.AddSingleton<IGameServerRepository, GameServerRepositoryMsSql>();
                break;
            case DatabaseProviderType.Postgresql:
                throw new Exception("Postgres Database Provider isn't supported, please enter a supported provider in appsettings.json!");
            case DatabaseProviderType.Sqlite:
                throw new Exception("Sqlite Database Provider isn't supported, please enter a supported provider in appsettings.json!");
            default:
                throw new Exception("Configured Database Provider isn't supported, please enter a supported provider in appsettings.json!");
        }

        // Seeds the targeted database using the indicated provider on startup
        services.AddHostedService<SqlDatabaseSeederService>();
    }
    
    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var securityConfig = configuration.GetSecuritySettings();
        var appConfig = configuration.GetApplicationSettings();
        
        services
            .AddAuthentication(authentication =>
            {
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(bearer =>
            {
                bearer.RequireHttpsMetadata = true;
                bearer.SaveToken = true;
                bearer.TokenValidationParameters = JwtHelpers.GetJwtValidationParameters(securityConfig, appConfig);
                bearer.AutomaticRefreshInterval = TimeSpan.FromMinutes(securityConfig.UserTokenExpirationMinutes - 1);
                bearer.RefreshInterval = TimeSpan.FromMinutes(securityConfig.UserTokenExpirationMinutes - 1);

                bearer.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = auth =>
                    {
                        var errorMessage = ErrorMessageConstants.Generic.ContactAdmin;
                        switch (auth.Exception)
                        {
                            case SecurityTokenInvalidLifetimeException:
                            case SecurityTokenExpiredException:
                                auth.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                                errorMessage = ErrorMessageConstants.Authentication.TokenExpiredError;
                                break;
                            case SecurityTokenMalformedException:
                            case SecurityTokenArgumentException:
                                auth.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                                errorMessage = ErrorMessageConstants.Authentication.TokenMalformedError;
                                break;
                            default:
                                auth.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                                Log.Warning("JWT authentication failed generically: {Error}", auth.Exception.Message);
                                break;
                        }

                        auth.Response.ContentType = "application/json";
                        var errorResult = JsonConvert.SerializeObject(Result.Fail(errorMessage));
                        return auth.Response.WriteAsync(errorResult);
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        if (context.Response.HasStarted) return Task.CompletedTask;
                        
                        context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";
                        var permissionFailure = JsonConvert.SerializeObject(Result.Fail(ErrorMessageConstants.Permissions.PermissionError));
                        return context.Response.WriteAsync(permissionFailure);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                        context.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(Result.Fail(ErrorMessageConstants.Permissions.Forbidden));
                        return context.Response.WriteAsync(result);
                    },
                };
            });
    }
}