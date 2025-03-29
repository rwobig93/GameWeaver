using Application.Constants.Communication;
using Application.Helpers.GameServer;
using Application.Mappers.GameServer;
using Application.Models.External.Steam;
using Application.Models.GameServer.Developers;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.GameUpdate;
using Application.Models.GameServer.Publishers;
using Application.Repositories.GameServer;
using Application.Repositories.Identity;
using Application.Requests.GameServer.Game;
using Application.Requests.GameServer.Host;
using Application.Services.External;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Domain.Enums.GameServer;
using Domain.Enums.Identity;
using Hangfire;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Lifecycle;

public class JobManager : IJobManager
{
    private readonly ILogger _logger;
    private readonly IAppUserRepository _userRepository;
    private readonly IAppAccountService _accountService;
    private readonly IDateTimeService _dateTime;
    private readonly IOptions<SecurityConfiguration> _securityConfig;
    private readonly IAuditTrailService _auditService;
    private readonly IOptions<LifecycleConfiguration> _lifecycleConfig;
    private readonly IGameServerRepository _gameServerRepository;
    private readonly IGameService _gameService;
    private readonly ISteamApiService _steamApiService;
    private readonly IRunningServerState _serverState;
    private readonly IGameRepository _gameRepository;
    private readonly IHostService _hostService;
    private readonly IOptionsMonitor<AppConfiguration> _appConfig;

    public JobManager(ILogger logger, IAppUserRepository userRepository, IAppAccountService accountService, IDateTimeService dateTime,
        IOptions<SecurityConfiguration> securityConfig, IAuditTrailService auditService, IOptions<LifecycleConfiguration> lifecycleConfig,
        IGameService gameService, ISteamApiService steamApiService, IRunningServerState serverState, IGameRepository gameRepository,
        IHostService hostService, IGameServerRepository gameServerRepository, IOptionsMonitor<AppConfiguration> appConfig)
    {
        _logger = logger;
        _userRepository = userRepository;
        _accountService = accountService;
        _dateTime = dateTime;
        _auditService = auditService;
        _gameService = gameService;
        _steamApiService = steamApiService;
        _serverState = serverState;
        _gameRepository = gameRepository;
        _hostService = hostService;
        _gameServerRepository = gameServerRepository;
        _appConfig = appConfig;
        _lifecycleConfig = lifecycleConfig;
        _securityConfig = securityConfig;
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task UserHousekeeping()
    {
        try
        {
            await HandleLockedOutUsers();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "User housekeeping failed: {Error}", ex.Message);
        }
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task DailyCleanup()
    {
        try
        {
            await AuditTrailCleanup();

            await HostRegistrationCleanup();

            await GameProfileCleanup();

            await WeaverWorkCleanup();

            await HostCheckInCleanup();

            _logger.Debug("Finished daily cleanup");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{LogPrefix} Daily cleanup failed: {Error}", DataConstants.Logging.JobDailyCleanup, ex.Message);
        }
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    private async Task HostCheckInCleanup()
    {
        var hostCheckInCleanupTimestamp = _dateTime.NowDatabaseTime.AddDays(-_lifecycleConfig.Value.HostCheckInCleanupAfterDays);
        var hostCheckInCleanup = await _hostService.DeleteAllOldCheckInsAsync(hostCheckInCleanupTimestamp, _serverState.SystemUserId);
        if (!hostCheckInCleanup.Succeeded)
        {
            _logger.Error("{LogPrefix} Host checkin cleanup failed: {Error}", DataConstants.Logging.JobDailyCleanup, hostCheckInCleanup.Messages);
        }
        _logger.Information("{LogPrefix} Cleaned {RecordCount} host checkin records", DataConstants.Logging.JobDailyCleanup, hostCheckInCleanup.Data);
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    private async Task WeaverWorkCleanup()
    {
        var workCleanupTimestamp = _dateTime.NowDatabaseTime.AddDays(-_lifecycleConfig.Value.WeaverWorkCleanupAfterDays);
        var weaverWorkCleanup = await _hostService.DeleteWeaverWorkOlderThanAsync(workCleanupTimestamp, _serverState.SystemUserId);
        if (!weaverWorkCleanup.Succeeded)
        {
            _logger.Error("{LogPrefix} Weaver work cleanup failed: {Error}", DataConstants.Logging.JobDailyCleanup, weaverWorkCleanup.Messages);
        }
        _logger.Information("{LogPrefix} Cleaned {RecordCount} weaver work records", DataConstants.Logging.JobDailyCleanup, weaverWorkCleanup.Data);
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    private async Task GameProfileCleanup()
    {
        var allGameProfiles = await _gameServerRepository.GetAllGameProfilesAsync();
        var allGameServers = await _gameServerRepository.GetAllAsync();
        var allGames = await _gameRepository.GetAllAsync();
        foreach (var gameProfile in allGameProfiles.Result?.ToList() ?? [])
        {
            var assignedServer = allGameServers.Result?.FirstOrDefault(x => x.GameProfileId == gameProfile.Id || x.ParentGameProfileId == gameProfile.Id);
            var assignedGame = allGames.Result?.FirstOrDefault(x => x.DefaultGameProfileId == gameProfile.Id);
            if (assignedServer is not null || assignedGame is not null)
            {
                continue;
            }
                
            var profileDelete = await _gameServerRepository.DeleteGameProfileAsync(gameProfile.Id, _serverState.SystemUserId);
            if (!profileDelete.Succeeded)
            {
                _logger.Error("{LogPrefix} Failed to delete unassigned game profile [{GameProfileId}]: {Error}",
                    DataConstants.Logging.JobDailyCleanup, gameProfile.Id, profileDelete.ErrorMessage);
                continue;
            }
                    
            _logger.Information("{LogPrefix} Deleted unassigned game profile: [{GameProfileId}]{GameProfileName}",
                DataConstants.Logging.JobDailyCleanup, gameProfile.Id, gameProfile.FriendlyName);
        }
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    private async Task HostRegistrationCleanup()
    {
        var hostRegistrationCleanup = await _hostService.DeleteRegistrationsOlderThanAsync(_serverState.SystemUserId, _lifecycleConfig.Value.HostRegistrationCleanupHours);
        if (!hostRegistrationCleanup.Succeeded)
        {
            _logger.Error("{LogPrefix} Host registration cleanup failed: {Error}", DataConstants.Logging.JobDailyCleanup, hostRegistrationCleanup.Messages);
        }
        _logger.Information("{LogPrefix} Cleaned {RecordCount} host registration records", DataConstants.Logging.JobDailyCleanup, hostRegistrationCleanup.Data);
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    private async Task AuditTrailCleanup()
    {
        var auditCleanup = await _auditService.DeleteOld(_lifecycleConfig.Value.AuditLogLifetime);
        if (!auditCleanup.Succeeded)
        {
            _logger.Error("{LogPrefix} Audit cleanup failed: {Error}", DataConstants.Logging.JobDailyCleanup, auditCleanup.Messages);
        }
        _logger.Information("{LogPrefix} Cleaned {RecordCount} audit records", DataConstants.Logging.JobDailyCleanup, auditCleanup.Data);
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    private async Task HandleLockedOutUsers()
    {
        // If account lockout threshold is 0 minutes then accounts are locked until unlocked by an administrator
        if (_securityConfig.Value.AccountLockoutMinutes == 0)
        {
            return;
        }

        var allLockedOutUsers = await _userRepository.GetAllLockedOutAsync();
        if (!allLockedOutUsers.Succeeded || allLockedOutUsers.Result is null)
        {
            _logger.Error("Failed to get locked out users: {Error}", allLockedOutUsers.ErrorMessage);
            return;
        }

        if (!allLockedOutUsers.Result.Any())
        {
            _logger.Debug("Currently no locked out users found during user housekeeping");
            return;
        }

        foreach (var user in allLockedOutUsers.Result)
        {
            try
            {
                // Account hasn't reached lockout threshold, skipping for now
                if (user.AuthStateTimestamp!.Value.AddMinutes(_securityConfig.Value.AccountLockoutMinutes) < _dateTime.NowDatabaseTime)
                {
                    continue;
                }
                
                // Account has passed locked threshold, so it's ready to be unlocked
                var unlockResult = await _accountService.SetAuthState(user.Id, AuthState.Enabled);
                if (!unlockResult.Succeeded)
                {
                    _logger.Error("Failed to unlock user during housekeeping: [{Id}]{Username}", user.Id, user.Username);
                    continue;
                }
                
                _logger.Debug("Successfully unlocked locked account that passed the lockout threshold[{Id}]{Username}", 
                    user.Id, user.Username);
            }
            catch (Exception innerException)
            {
                _logger.Error(innerException, 
                    "Failed to parse locked out user for housekeeping: [{Id}]{Username}", user.Id, user.Username);
            }
        }
        
        _logger.Debug("Finished handling locked out users during housekeeping job");
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task GameVersionCheck()
    {
        var allGameServers = await _gameServerRepository.GetAllAsync();
        var gamesFromGameServers = allGameServers.Result?.Select(x => x.GameId).Distinct().ToList() ?? [];
        _logger.Debug("Gathered {GameCount} games from active game servers to check for version updates", gamesFromGameServers.Count);

        foreach (var gameId in gamesFromGameServers)
        {
            try
            {
                var foundGame = await _gameService.GetByIdAsync(gameId);
                if (foundGame.Data is null)
                {
                    _logger.Error("Failed to find game pulled from game server list: {GameId}", gameId);
                    continue;
                }

                var latestVersionBuild = await _steamApiService.GetCurrentAppBuild(foundGame.Data.SteamToolId);
                if (!latestVersionBuild.Succeeded || latestVersionBuild.Data is null)
                {
                    _logger.Error("Failed to get latest game version build from steam: {Error}", latestVersionBuild.Messages);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(latestVersionBuild.Data.VersionBuild))
                {
                    _logger.Error("Retrieved latest game version build with an empty build: [{GameId}]{VersionBuild}",
                        gameId, latestVersionBuild.Data.VersionBuild);
                    continue;
                }

                if (foundGame.Data.LatestBuildVersion == latestVersionBuild.Data.VersionBuild)
                {
                    _logger.Debug("Game version matches latest: [{GameId}] {GameVersion} == {LatestVersion}",
                        gameId, foundGame.Data.LatestBuildVersion, latestVersionBuild.Data.VersionBuild);
                    continue;
                }
                
                // Latest version is newer than the game's current version, so we'll update the game and add an update record
                var gameUpdate = await _gameService.UpdateAsync(new GameUpdateRequest
                {
                    Id = foundGame.Data.Id,
                    LatestBuildVersion = latestVersionBuild.Data.VersionBuild
                }, _serverState.SystemUserId);
                if (!gameUpdate.Succeeded)
                {
                    _logger.Error("Failed to update game with latest version: [{GameId}]{Error}", gameId, gameUpdate.Messages);
                    continue;
                }

                var updateRecordCreate = await _gameService.CreateGameUpdateAsync(new GameUpdateCreate
                {
                    GameId = foundGame.Data.Id,
                    SupportsWindows = latestVersionBuild.Data.OsSupport.Contains(OsType.Windows),
                    SupportsLinux = latestVersionBuild.Data.OsSupport.Contains(OsType.Linux),
                    SupportsMac = latestVersionBuild.Data.OsSupport.Contains(OsType.Mac),
                    BuildVersion = latestVersionBuild.Data.VersionBuild,
                    BuildVersionReleased = latestVersionBuild.Data.LastUpdatedUtc
                });
                if (!updateRecordCreate.Succeeded)
                {
                    _logger.Error("Failed to create game update record: [{GameId}]{Error}", gameId, updateRecordCreate.Messages);
                    continue;
                }
                
                _logger.Information("Game version updated! [{GameId}]{PreviousVersion} => {LatestVersion}",
                    foundGame.Data.Id, foundGame.Data.LatestBuildVersion, latestVersionBuild.Data.VersionBuild);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure occurred attempting to check version for game [{GameId}]{Error}", gameId, ex.Message);
            }
        }
        
        _logger.Debug("Finished game version check job");
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task DailySteamSync()
    {
        var allSteamApps = await _steamApiService.GetAllApps();
        if (!allSteamApps.Succeeded)
        {
            _logger.Error("Failed to get all steam apps from the Steam API: {Error}", allSteamApps.Messages);
            return;
        }
        
        var serverApps = allSteamApps.Data.Where(x =>
            x.Name.Contains(_appConfig.CurrentValue.SteamAppNameFilter, StringComparison.CurrentCultureIgnoreCase) ||
            x.Name.Contains(_appConfig.CurrentValue.SteamAppNameFilter.Replace(" ", ""), StringComparison.CurrentCultureIgnoreCase));
        foreach (var serverApp in serverApps)
        {
            var appSanitizedName = serverApp.Name.SanitizeFromSteam();
            if (string.IsNullOrWhiteSpace(appSanitizedName) || appSanitizedName.Length < 3)
            {
                _logger.Verbose("Steam app sanitized name is empty, skipping: [{ToolId}]{GameName}", serverApp.AppId, serverApp.Name);
                continue;
            }
            
            var matchingGame = await _gameService.GetBySteamToolIdAsync(serverApp.AppId);
            if (matchingGame.Data is not null
                && matchingGame.Data.SourceType == GameSource.Steam
                && matchingGame.Data.SteamToolId != 0
                && matchingGame.Data.SteamGameId != 0)
            {
                _logger.Verbose("Found matching fully synced game with tool id from steam: [{ToolId}]{GameName} => {GameId}",
                    matchingGame.Data.SteamToolId, matchingGame.Data.SteamName, matchingGame.Data.Id);
                continue;
            }

            var gameId = matchingGame.Data?.Id ?? Guid.Empty;

            var baseGame = await GetBaseGame(allSteamApps, serverApp);
            if (baseGame is null)
            {
                _logger.Information("Unable to find base game for server app from steam: [{ToolId}]{GameName}", serverApp.AppId, serverApp.Name);
                continue;
            }

            if (matchingGame.Data is null)
            {
                var gameCreate = await _gameService.CreateAsync(new GameCreateRequest
                {
                    Name = serverApp.Name,
                    SteamGameId = 0,
                    SteamToolId = serverApp.AppId
                }, _serverState.SystemUserId);
                if (!gameCreate.Succeeded)
                {
                    _logger.Error("Failed to create server app game from steam: [{ToolId}]{Error}", serverApp.AppId, gameCreate.Messages);
                    continue;
                }
                
                _logger.Information("Created missing server app from steam: [{ToolId}]{GameName} => {GameId}",
                    serverApp.AppId, serverApp.Name, gameCreate.Data);
                
                gameId = gameCreate.Data;
            }

            var convertedGameUpdate = baseGame.ToUpdate(gameId);
            convertedGameUpdate.SteamName = serverApp.Name;
            convertedGameUpdate.FriendlyName = baseGame.Name;
            convertedGameUpdate.LastModifiedBy = _serverState.SystemUserId;
            convertedGameUpdate.LastModifiedOn = _dateTime.NowDatabaseTime;
            
            var gameUpdate = await _gameRepository.UpdateAsync(convertedGameUpdate);
            if (!gameUpdate.Succeeded)
            {
                _logger.Error("Failed to get update game with details from steam: [{ToolId}/{GameId}]{Error}",
                    serverApp.AppId, baseGame.Steam_AppId, gameUpdate.ErrorMessage);
                continue;
            }
            
            foreach (var publisher in baseGame.Publishers)
            {
                var createPublisher = await _gameService.CreatePublisherAsync(new PublisherCreate
                {
                    GameId = gameId,
                    Name = publisher
                }, _serverState.SystemUserId);
                if (!createPublisher.Succeeded)
                {
                    _logger.Error("Failed to create publisher for game: [{ToolId}/{GameId}]{Error}",
                        serverApp.AppId, baseGame.Steam_AppId, createPublisher.Messages);
                }
            }

            foreach (var developer in baseGame.Developers)
            {
                var createDeveloper = await _gameService.CreateDeveloperAsync(new DeveloperCreate
                {
                    GameId = gameId,
                    Name = developer
                }, _serverState.SystemUserId);
                if (!createDeveloper.Succeeded)
                {
                    _logger.Error("Failed to create developer for game: [{ToolId}/{GameId}]{Error}",
                        serverApp.AppId, baseGame.Steam_AppId, createDeveloper.Messages);
                }
            }

            foreach (var genre in baseGame.Genres)
            {
                var createGenre = await _gameService.CreateGameGenreAsync(new GameGenreCreate
                {
                    GameId = gameId,
                    Name = genre.Description,
                    Description = $"[{genre.Id}]{genre.Description}"
                }, _serverState.SystemUserId);
                if (!createGenre.Succeeded)
                {
                    _logger.Error("Failed to create genre for game: [{ToolId}/{GameId}]{Error}",
                        serverApp.AppId, baseGame.Steam_AppId, createGenre.Messages);
                }
            }
            
            _logger.Information("Successfully synchronized game from steam: [{ToolId}/{GameId}] {GameLocalId}",
                serverApp.AppId, baseGame.Steam_AppId, gameId);
        }
    }

    private async Task<SteamAppDetailResponseJson?> GetBaseGame(IResult<IEnumerable<SteamApiAppResponseJson>> allSteamApps, SteamApiAppResponseJson serverApp)
    {
        var sanitizedServerAppName = serverApp.Name.SanitizeFromSteam();
        _logger.Debug("Attempting to find base game for server app: [{ToolId}]{GameName} => {AppName}", serverApp.AppId, serverApp.Name, sanitizedServerAppName);
        List<SteamAppDetailResponseJson> filteredGameMatches = [];
        
        var baseGameMatches = allSteamApps.Data.Where(x =>
            x.AppId != serverApp.AppId &&
            !x.Name.EndsWith(" beta", StringComparison.InvariantCultureIgnoreCase) &&
            !x.Name.EndsWith(" test", StringComparison.InvariantCultureIgnoreCase) &&
            !x.Name.EndsWith(" demo", StringComparison.InvariantCultureIgnoreCase) &&
            !x.Name.Contains("developer build", StringComparison.InvariantCultureIgnoreCase) &&
            x.Name.Contains(sanitizedServerAppName))
            .OrderBy(x => x.Name != sanitizedServerAppName)
            .ToArray();

        foreach (var game in baseGameMatches)
        {
            await Task.Delay(500);
            var appDetail = await _steamApiService.GetAppDetail(game.AppId);
            if (appDetail.Data is null)
            {
                _logger.Debug("Failed to get base game details from steam: [{ToolId}/{GameId}] => {Error}", serverApp.AppId, game.AppId, appDetail.Messages);
                continue;
            }

            if (appDetail.Data.Type != "game")
            {
                _logger.Debug("App detail found but isn't a game, skipping for base game match: [{ToolId}/{GameId}] {AppType}",
                    serverApp.AppId, game.AppId, appDetail.Data.Type);
                continue;
            }

            if (appDetail.Data.Name == sanitizedServerAppName)
            {
                _logger.Information("Selected and found game match for server: [{ToolId}/{GameId}] {GameName}",
                    serverApp.AppId, appDetail.Data.Steam_AppId, appDetail.Data.Name);
                return appDetail.Data;
            }

            filteredGameMatches.Add(appDetail.Data);
            _logger.Debug("Found game match from app detail: [{ToolId}/{GameId}] {GameName}", serverApp.AppId, game.AppId, appDetail.Data.Name);
        }

        if (filteredGameMatches.Count == 1)
        {
            var selectedGame = filteredGameMatches.First();
            _logger.Information("Selected and found game match for server: [{ToolId}/{GameId}] {GameName}",
                serverApp.AppId, selectedGame.Steam_AppId, selectedGame.Name);
            return selectedGame;
        }

        if (filteredGameMatches.Count > 1)
        {
            _logger.Information("Found {MatchCount} base game matches for {GameName}, will select the first but all matches were:", filteredGameMatches.Count, sanitizedServerAppName);
            foreach (var gameMatch in filteredGameMatches)
            {
                _logger.Information("  Matching Game: [{GameId}] {GameName}", gameMatch.Steam_AppId, gameMatch.Name);
            }
        }

        return filteredGameMatches.FirstOrDefault();
    }

    [DisableConcurrentExecution(10)]
    [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task HostStatusCheck()
    {
        var allHosts = await _hostService.GetAllAsync();
        _logger.Debug("Gathered {HostCount} active Hosts to check for offline status", allHosts.Data.Count());

        foreach (var host in allHosts.Data)
        {
            var latestCheckIn = await _hostService.GetCheckInsLatestByHostIdAsync(host.Id, 1);
            if (!latestCheckIn.Data.Any() && !host.CurrentState.IsRunning())
            {
                _logger.Verbose("Host is already offline and has no latest checkin, no status update necessary: {HostId}", host.Id);
                continue;
            }
            
            // Last checkin is within configured seconds so is considered online or not
            var secondsSinceLastCheckIn = Math.Round((_dateTime.NowDatabaseTime - latestCheckIn.Data.Last().ReceiveTimestamp).TotalSeconds, 0, MidpointRounding.ToZero);
            var hostIsOffline = secondsSinceLastCheckIn > _appConfig.CurrentValue.HostOfflineAfterSeconds;
            if (hostIsOffline && !host.CurrentState.IsRunning())
            {
                _logger.Verbose("Host is already offline and checked in {SecondsSinceCheckin}s ago, no status update necessary for {HostId}",
                    secondsSinceLastCheckIn, host.Id);
                continue;
            }
            
            _logger.Debug("Host was last known online but hasn't checked in for {SecondsSinceCheckin}s which is over the offline {OfflineSeconds} and is now considered offline," +
                          " updating status for host {HostId}", secondsSinceLastCheckIn, _appConfig.CurrentValue.HostOfflineAfterSeconds, host.Id);
            var updateHost = new HostUpdateRequest() {Id = host.Id, CurrentState = ConnectivityState.Unknown};
            var updateHostResponse = await _hostService.UpdateAsync(updateHost, _serverState.SystemUserId);
            if (!updateHostResponse.Succeeded)
            {
                _logger.Error("Failed to update host offline status for {HostId}: {Error}", host.Id, updateHostResponse.Messages);
                continue;
            }
            
            _logger.Information("Host {HostId} hasn't checked in and is now offline", host.Id);
        }
    }
}