﻿using System.Net;
using System.Text.Json.Serialization;
using Application.Filters;
using Application.Helpers.Auth;
using Application.Helpers.Identity;
using Application.Helpers.Runtime;
using Application.Models.Identity.Permission;
using Application.Models.Web;
using Application.Repositories.Identity;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.Example;
using Application.Services.Identity;
using Application.Services.Integrations;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Asp.Versioning;
using Blazored.LocalStorage;
using Domain.Enums.Database;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.HealthChecks;
using Infrastructure.Repositories.MsSql.Identity;
using Infrastructure.Repositories.MsSql.Lifecycle;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Database;
using Infrastructure.Services.Example;
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
using Microsoft.OpenApi.Models;
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
                    x.UseSqlServerStorage(databaseSettings.Core);
                    break;
                case DatabaseProviderType.Postgresql:
                    // Need to add database support for application before we can fully support Postgresql
                    x.UsePostgreSqlStorage(databaseSettings.Core);
                    throw new Exception("Postgres Database Provider isn't supported, please enter a supported provider in appsettings.json!");
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
        services.AddHttpClient("Default", options =>
        {
            options.BaseAddress = new Uri(configuration.GetApplicationSettings().BaseUrl);
        }).ConfigureCertificateHandling(configuration);

        var mailConfig = configuration.GetMailSettings();
        services.AddFluentEmail(mailConfig.From, mailConfig.DisplayName)
            .AddRazorRenderer().AddSmtpSender(mailConfig.Host, mailConfig.Port, mailConfig.UserName, mailConfig.Password);
        
        services.AddSingleton<IRunningServerState, RunningServerState>();
        services.AddSingleton<ISerializerService, JsonSerializerService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddScoped<IWebClientService, WebClientService>();
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static IHttpClientBuilder ConfigureCertificateHandling(this IHttpClientBuilder httpClientBuilder, IConfiguration configuration)
    {
        if (!configuration.GetSecuritySettings().TrustAllCertificates)
            return httpClientBuilder;
        
        return httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
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

        services.AddSingleton<IExternalAuthProviderService, ExternalAuthProviderService>();
        
        services.AddJwtAuthentication(configuration);
        services.AddAuthorization(options =>
        {
            // Enumerate permissions and create claim policies for them
            foreach (var permission in PermissionHelpers.GetAllBuiltInPermissions())
            {
                options.AddPolicy(permission, policy => policy.RequireClaim(
                    ApplicationClaimTypes.Permission, permission));
            }
        });
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromSeconds(securitySettings.PermissionValidationIntervalSeconds);
        });
    }

    private static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IAuditTrailService, AuditTrailService>();
        services.AddSingleton<IExcelService, ExcelService>();
        services.AddTransient<IEmailService, EmailService>();
        
        services.AddTransient<IMfaService, MfaService>();
        services.AddTransient<IQrCodeService, QrCodeService>();
        services.AddTransient<IJobManager, JobManager>();
        
        // Example services
        services.AddSingleton<IWeatherService, WeatherForecastService>();
    }

    private static void AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(x =>
        {
            // Gather swagger XML generated documentation from every assembly
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic) continue;
            
                var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assembly.GetName().Name}.xml");
                if (File.Exists(xmlPath))
                    x.IncludeXmlComments(xmlPath);
            }
            
            x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JSON Web Token Header Authorization Using Bearer Scheme",
            });
            x.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            x.UseInlineDefinitionsForEnums();
            x.SchemaFilter<EnumSchemaFilter>();
        });
        
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
                services.AddSingleton<IAppUserRepository, AppUserRepositoryMsSql>();
                services.AddSingleton<IAppRoleRepository, AppRoleRepositoryMsSql>();
                services.AddSingleton<IAppPermissionRepository, AppPermissionRepositoryMsSql>();
                services.AddSingleton<IAuditTrailsRepository, AuditTrailsRepositoryMsSql>();
                services.AddSingleton<IServerStateRecordsRepository, ServerStateRecordsRepositoryMsSql>();
                break;
            case DatabaseProviderType.Postgresql:
                throw new Exception("Postgres Database Provider isn't supported, please enter a supported provider in appsettings.json!");
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

                bearer.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = auth =>
                    {
                        if (auth.Exception is SecurityTokenExpiredException)
                        {
                            auth.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                            auth.Response.ContentType = "application/json";
                            var authExpired = JsonConvert.SerializeObject(Result.Fail("Authentication Token has expired, please login again"));
                            return auth.Response.WriteAsync(authExpired);
                        }
                        
                        auth.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                        auth.Response.ContentType = "application/json";
                        var generalError = JsonConvert.SerializeObject(Result.Fail("An unhandled error has occurred."));
                        return auth.Response.WriteAsync(generalError);
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        if (context.Response.HasStarted) return Task.CompletedTask;
                        
                        context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";
                        var permissionFailure = JsonConvert.SerializeObject(Result.Fail("You don't have permission to this resource"));
                        return context.Response.WriteAsync(permissionFailure);

                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                        context.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(
                            Result.Fail("You hath been forbidden, do thy bidding my masta, it's a disasta, skywalka we're afta!"));
                        return context.Response.WriteAsync(result);
                    },
                };
            });
    }
}