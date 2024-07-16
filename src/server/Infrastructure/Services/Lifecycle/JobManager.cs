using Application.Constants.Communication;
using Application.Helpers.GameServer;
using Application.Mappers.GameServer;
using Application.Models.External.Steam;
using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.GameUpdate;
using Application.Models.GameServer.Publishers;
using Application.Repositories.GameServer;
using Application.Repositories.Identity;
using Application.Requests.GameServer.Game;
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
    private readonly SecurityConfiguration _securityConfig;
    private readonly IAuditTrailService _auditService;
    private readonly LifecycleConfiguration _lifecycleConfig;
    private readonly IGameServerService _gameServerService;
    private readonly IGameService _gameService;
    private readonly ISteamApiService _steamApiService;
    private readonly IRunningServerState _serverState;
    private readonly IGameRepository _gameRepository;
    private readonly IHostService _hostService;
    private readonly IGameServerRepository _gameServerRepository;

    public JobManager(ILogger logger, IAppUserRepository userRepository, IAppAccountService accountService, IDateTimeService dateTime,
        IOptions<SecurityConfiguration> securityConfig, IAuditTrailService auditService, IOptions<LifecycleConfiguration> lifecycleConfig,
        IGameServerService gameServerService, IGameService gameService, ISteamApiService steamApiService, IRunningServerState serverState, IGameRepository gameRepository,
        IHostService hostService, IGameServerRepository gameServerRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _accountService = accountService;
        _dateTime = dateTime;
        _auditService = auditService;
        _gameServerService = gameServerService;
        _gameService = gameService;
        _steamApiService = steamApiService;
        _serverState = serverState;
        _gameRepository = gameRepository;
        _hostService = hostService;
        _gameServerRepository = gameServerRepository;
        _lifecycleConfig = lifecycleConfig.Value;
        _securityConfig = securityConfig.Value;
    }

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

    private async Task HostCheckInCleanup()
    {
        var hostCheckInCleanupTimestamp = _dateTime.NowDatabaseTime.AddDays(-_lifecycleConfig.HostCheckInCleanupAfterDays);
        var hostCheckInCleanup = await _hostService.DeleteAllOldCheckInsAsync(hostCheckInCleanupTimestamp, _serverState.SystemUserId);
        if (!hostCheckInCleanup.Succeeded)
        {
            _logger.Error("{LogPrefix} Host checkin cleanup failed: {Error}", DataConstants.Logging.JobDailyCleanup, hostCheckInCleanup.Messages);
        }
        _logger.Information("{LogPrefix} Cleaned {RecordCount} host checkin records", DataConstants.Logging.JobDailyCleanup, hostCheckInCleanup.Data);
    }

    private async Task WeaverWorkCleanup()
    {
        var workCleanupTimestamp = _dateTime.NowDatabaseTime.AddDays(-_lifecycleConfig.WeaverWorkCleanupAfterDays);
        var weaverWorkCleanup = await _hostService.DeleteWeaverWorkOlderThanAsync(workCleanupTimestamp, _serverState.SystemUserId);
        if (!weaverWorkCleanup.Succeeded)
        {
            _logger.Error("{LogPrefix} Weaver work cleanup failed: {Error}", DataConstants.Logging.JobDailyCleanup, weaverWorkCleanup.Messages);
        }
        _logger.Information("{LogPrefix} Cleaned {RecordCount} weaver work records", DataConstants.Logging.JobDailyCleanup, weaverWorkCleanup.Data);
    }

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

    private async Task HostRegistrationCleanup()
    {
        var hostRegistrationCleanup = await _hostService.DeleteRegistrationsOlderThanAsync(_serverState.SystemUserId, _lifecycleConfig.HostRegistrationCleanupHours);
        if (!hostRegistrationCleanup.Succeeded)
        {
            _logger.Error("{LogPrefix} Host registration cleanup failed: {Error}", DataConstants.Logging.JobDailyCleanup, hostRegistrationCleanup.Messages);
        }
        _logger.Information("{LogPrefix} Cleaned {RecordCount} host registration records", DataConstants.Logging.JobDailyCleanup, hostRegistrationCleanup.Data);
    }

    private async Task AuditTrailCleanup()
    {
        var auditCleanup = await _auditService.DeleteOld(_lifecycleConfig.AuditLogLifetime);
        if (!auditCleanup.Succeeded)
        {
            _logger.Error("{LogPrefix} Audit cleanup failed: {Error}", DataConstants.Logging.JobDailyCleanup, auditCleanup.Messages);
        }
        _logger.Information("{LogPrefix} Cleaned {RecordCount} audit records", DataConstants.Logging.JobDailyCleanup, auditCleanup.Data);
    }

    private async Task HandleLockedOutUsers()
    {
        // If account lockout threshold is 0 minutes then accounts are locked until unlocked by an administrator
        if (_securityConfig.AccountLockoutMinutes == 0)
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
                if (user.AuthStateTimestamp!.Value.AddMinutes(_securityConfig.AccountLockoutMinutes) < _dateTime.NowDatabaseTime)
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

    public async Task GameVersionCheck()
    {
        var allGameServers = await _gameServerService.GetAllAsync();
        var gamesFromGameServers = allGameServers.Data.Select(x => x.GameId).Distinct().ToList();
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
        
        var serverApps = allSteamApps.Data.Where(x => x.Name.Contains("server", StringComparison.CurrentCultureIgnoreCase));
        foreach (var serverApp in serverApps)
        {
            var appSanitizedName = serverApp.Name.SanitizeFromSteam();
            if (string.IsNullOrWhiteSpace(appSanitizedName) || appSanitizedName.Length < 3)
            {
                _logger.Verbose("Steam app sanitized name is empty, skipping: [{ToolId}]{GameName}", serverApp.AppId, serverApp.Name);
                continue;
            }
            
            var matchingGame = await _gameService.GetBySteamToolIdAsync(serverApp.AppId);
            if (matchingGame.Data is not null)
            {
                // TODO: If ToolId isn't 0 but GameId is we should do an update if we can find game details
                _logger.Verbose("Found matching game with tool id from steam: [{ToolId}]{GameName} => {GameId}",
                    matchingGame.Data.SteamToolId, matchingGame.Data.SteamName, matchingGame.Data.Id);
                continue;
            }

            var baseGame = await GetBaseGame(allSteamApps, serverApp);
            if (baseGame is null)
            {
                _logger.Information("Unable to find base game for server app from steam: [{ToolId}]{GameName}", serverApp.AppId, serverApp.Name);
                continue;
            }
            
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

            var convertedGameUpdate = baseGame.ToUpdate(gameCreate.Data);
            convertedGameUpdate.SteamName = serverApp.Name;
            convertedGameUpdate.FriendlyName = baseGame?.Name;
            convertedGameUpdate.LastModifiedBy = _serverState.SystemUserId;
            convertedGameUpdate.LastModifiedOn = _dateTime.NowDatabaseTime;
            
            var gameUpdate = await _gameRepository.UpdateAsync(convertedGameUpdate);
            if (!gameUpdate.Succeeded)
            {
                _logger.Error("Failed to get update game with details from steam: [{ToolId}/{GameId}]{Error}",
                    serverApp.AppId, baseGame?.Steam_AppId ?? 0, gameCreate.Messages);
                continue;
            }
            
            foreach (var publisher in baseGame!.Publishers)
            {
                var createPublisher = await _gameService.CreatePublisherAsync(new PublisherCreate
                {
                    GameId = gameCreate.Data,
                    Name = publisher
                }, _serverState.SystemUserId);
                if (!createPublisher.Succeeded)
                {
                    _logger.Error("Failed to create publisher for game: [{ToolId}/{GameId}]{Error}",
                        serverApp.AppId, baseGame.Steam_AppId, gameCreate.Messages);
                }
            }

            foreach (var developer in baseGame.Developers)
            {
                var createDeveloper = await _gameService.CreateDeveloperAsync(new DeveloperCreate
                {
                    GameId = gameCreate.Data,
                    Name = developer
                }, _serverState.SystemUserId);
                if (!createDeveloper.Succeeded)
                {
                    _logger.Error("Failed to create developer for game: [{ToolId}/{GameId}]{Error}",
                        serverApp.AppId, baseGame.Steam_AppId, gameCreate.Messages);
                }
            }

            foreach (var genre in baseGame.Genres)
            {
                var createGenre = await _gameService.CreateGameGenreAsync(new GameGenreCreate
                {
                    GameId = gameCreate.Data,
                    Name = genre.Description,
                    Description = $"[{genre.Id}]{genre.Description}"
                }, _serverState.SystemUserId);
                if (!createGenre.Succeeded)
                {
                    _logger.Error("Failed to create genre for game: [{ToolId}/{GameId}]{Error}",
                        serverApp.AppId, baseGame.Steam_AppId, gameCreate.Messages);
                }
            }
            
            _logger.Information("Successfully synchronized game from steam: [{ToolId}/{GameId}] {GameLocalId}",
                serverApp.AppId, baseGame.Steam_AppId, gameCreate.Data);
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
            x.Name.Contains(sanitizedServerAppName));

        foreach (var game in baseGameMatches)
        {
            await Task.Delay(1500);
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
}