using System.Reflection;
using Application.Helpers.Web;
using Application.Services.Lifecycle;
using Application.Settings.AppSettings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Lifecycle;

public class RunningServerState : IRunningServerState
{
    private readonly IOptionsMonitor<AppConfiguration> _appConfig;
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public RunningServerState(IOptionsMonitor<AppConfiguration> appConfig, ILogger logger, IHttpClientFactory httpClientFactory)
    {
        _appConfig = appConfig;
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        InitializationUpdate();
    }

    public bool IsRunningInDebugMode { get; private set; }
    public string ApplicationName { get; private set; } = "";
    public Guid SystemUserId { get; private set; } = Guid.Empty;
    public Version ApplicationVersion { get; private set; } = new();

    public string PublicIp { get; private set; } = string.Empty;


    public void UpdateServerState()
    {
        UpdateApplicationConfiguration();
    }

    public void UpdateSystemUserId(Guid systemUserId)
    {
        // If the system user ID has already been updated by the database seeder we don't want it changing
        if (SystemUserId != Guid.Empty)
        {
            _logger.Warning("An attempt was made to update the SystemUserId => {CurrentId} {AttemptedId}",
                SystemUserId, systemUserId);
            return;
        }

        SystemUserId = systemUserId;
        _logger.Information("Running application system User Id Set: {SystemUserId}", SystemUserId);
    }

    public async Task UpdatePublicIp()
    {
        var publicIp = await _httpClientFactory.GetPublicIp();
        _logger.Information("Updated public ip address: {PublicIpAddress}", publicIp);
        PublicIp = publicIp;
    }

    private void InitializationUpdate()
    {
        UpdateRunningMode();
        UpdateApplicationVersion();
        UpdateServerState();
        Task.Run(UpdatePublicIp);
    }

    private void UpdateApplicationVersion()
    {
        ApplicationVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();
        _logger.Information("Running application version: {Version}", ApplicationVersion);
    }

    private void UpdateApplicationConfiguration()
    {
        ApplicationName = _appConfig.CurrentValue.ApplicationName;
        _logger.Information("Running application name: {AppName}", ApplicationName);
    }

    private void UpdateRunningMode()
    {
        #if DEBUG
                IsRunningInDebugMode = true;
        #else
                IsRunningInDebugMode = false;
        #endif
        _logger.Information("Running application is development: {RunningDebugMode}", IsRunningInDebugMode);
    }
}