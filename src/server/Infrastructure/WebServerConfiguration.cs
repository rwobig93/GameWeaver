using System.Diagnostics.CodeAnalysis;
using Application.Api.v1.Api;
using Application.Api.v1.GameServer;
using Application.Api.v1.Identity;
using Application.Api.v1.Lifecycle;
using Application.Constants.Web;
using Application.Helpers.Runtime;
using Application.Services.Database;
using Application.Services.Lifecycle;
using Application.Settings.AppSettings;
using Hangfire;
using HealthChecks.UI.Client;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

namespace Infrastructure;

public static class WebServerConfiguration
{
    // Certificate loading via appsettings.json =>
    //   https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0
    
    [SuppressMessage("ReSharper.DPA", "DPA0008: Large number of DB connections")]
    public static void ConfigureWebServices(this WebApplication app)
    {
        app.ConfigureForEnvironment();
        app.ConfigureBlazorServerCommons();
        
        app.ValidateDatabaseStructure();
        
        app.ConfigureCoreServices();
        app.ConfigureApiServices();
        app.ConfigureIdentityServices();
        
        app.MapApplicationApiEndpoints();
        app.AddScheduledJobs();
    }

    private static void ConfigureAppUrls(this WebApplication app)
    {
        using var scope = app.Services.CreateAsyncScope();
        var appConfig = scope.ServiceProvider.GetRequiredService<IOptions<AppConfiguration>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
        
        app.Urls.Add(appConfig.Value.BaseUrl);
        logger.Information("Successfully bound application to Url: {Url}", appConfig.Value.BaseUrl);
        
        foreach (var altUrl in appConfig.Value.AlternativeUrls)
        {
            try
            {
                app.Urls.Add(altUrl);
                logger.Information("Successfully bound alternate application to Url: {Url}", altUrl);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Failed to map to alternate url provided: [{Url}] {Error}", altUrl, ex.Message);
            }
        }
    }

    private static void ConfigureForEnvironment(this WebApplication app)
    {
        app.ConfigureAppUrls();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            return;
        }
        
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
        });
        
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    private static void ConfigureBlazorServerCommons(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
        
        app.UseSerilogRequestLogging();
        app.UseForwardedHeaders(new ForwardedHeadersOptions()
        {
            ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
        });
        app.UseMiddleware<ErrorHandlerMiddleware>();
    }
    
    private static void ValidateDatabaseStructure(this IHost app)
    {
        using var scope = app.Services.CreateAsyncScope();
        var sqlAccess = scope.ServiceProvider.GetRequiredService<ISqlDataService>();
        sqlAccess.EnforceDatabaseStructure();
    }

    private static void ConfigureCoreServices(this IApplicationBuilder app)
    {
        app.UseHangfireDashboard("/jobs", new DashboardOptions
        {
            DashboardTitle = "Jobs",
            // Authorization = new[] {new HangfireAuthorizationFilter()}
            DarkModeEnabled = true
        });
    }

    private static void ConfigureIdentityServices(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        ((IEndpointRouteBuilder) app).MapIdentityApiEndpoints();
    }

    private static void ConfigureApiServices(this WebApplication app)
    {
        using var scope = app.Services.CreateAsyncScope();
        var serverState = scope.ServiceProvider.GetRequiredService<IRunningServerState>();

        app.MapOpenApi();
        app.MapScalarApiReference("/api", options =>
        {
            options.Title = $"{serverState.ApplicationName} API";
            options.Layout = ScalarLayout.Modern;
            options.DarkMode = true;
            options.WithPreferredScheme("Bearer");
        });
        
        app.MapControllers();
        app.ConfigureApiVersions();
        
        app.MapHealthChecks("/_health", new HealthCheckOptions()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    }

    private static void ConfigureApiVersions(this IEndpointRouteBuilder app)
    {
        ApiConstants.SupportsVersionOne = app.NewApiVersionSet()
            .HasApiVersion(ApiConstants.Version1)
            .ReportApiVersions()
            .Build();
        ApiConstants.SupportsOneTwo = app.NewApiVersionSet()
            .HasApiVersion(ApiConstants.Version1)
            .HasApiVersion(ApiConstants.Version2)
            .ReportApiVersions()
            .Build();
    }

    private static void MapIdentityApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Map endpoints that require identity services
        app.MapEndpointsUsers();
        app.MapEndpointsRoles();
        app.MapEndpointsPermissions();
        app.MapEndpointsApi();
    }

    private static void MapApplicationApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Map all other endpoints for the application (not identity and not examples)
        app.MapEndpointsAudit();
        app.MapEndpointsHost();
        app.MapEndpointsHostRegistration();
        app.MapEndpointsHostCheckin();
        app.MapEndpointsWeaverWork();
        app.MapEndpointsGame();
        app.MapEndpointsGameGenre();
        app.MapEndpointsDeveloper();
        app.MapEndpointsPublisher();
        app.MapEndpointsGameserver();
        app.MapEndpointsConfigItem();
        app.MapEndpointsGameProfile();
        app.MapEndpointsLocalResource();
        app.MapEndpointsMod();
        app.MapEndpointsNetwork();
    }

    private static void AddScheduledJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateAsyncScope();
        var hangfireJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
        
        hangfireJobs.AddOrUpdate("UserHousekeeping", () => jobManager.UserHousekeeping(), JobHelpers.CronString.Minutely);
        hangfireJobs.AddOrUpdate("DailySystemCleanup", () => jobManager.DailyCleanup(), JobHelpers.CronString.Daily);
        hangfireJobs.AddOrUpdate("GameVersionCheck", () => jobManager.GameVersionCheck(), JobHelpers.CronString.MinuteInterval(5));
        hangfireJobs.AddOrUpdate("DailySteamSync", () => jobManager.DailySteamSync(), JobHelpers.CronString.Daily);
    }
}