using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameUpdate;
using Application.Repositories.Identity;
using Application.Requests.GameServer.Game;
using Application.Services.External;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Enums.GameServer;
using Domain.Enums.Identity;
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

    public JobManager(ILogger logger, IAppUserRepository userRepository, IAppAccountService accountService, IDateTimeService dateTime,
        IOptions<SecurityConfiguration> securityConfig, IAuditTrailService auditService, IOptions<LifecycleConfiguration> lifecycleConfig,
        IGameServerService gameServerService, IGameService gameService, ISteamApiService steamApiService, IRunningServerState serverState)
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
            var auditCleanup = await _auditService.DeleteOld(_lifecycleConfig.AuditLogLifetime);
            if (!auditCleanup.Succeeded)
                _logger.Error("Audit cleanup failed: {Error}", auditCleanup.Messages);
            
            // TODO: Cleanup host registrations and their hosts if not registered after 24 hours by default, should be configurable
            
            // TODO: Cleanup non-default game profiles that aren't bound to any game servers
            
            // TODO: Cleanup old weaver work using a configurable timeframe
            
            // TODO: Cleanup old host checkins using a configurable timeframe
            
            _logger.Debug("Finished daily cleanup");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Daily cleanup failed: {Error}", ex.Message);
        }
    }

    private async Task HandleLockedOutUsers()
    {
        // If account lockout threshold is 0 minutes then accounts are locked until unlocked by an administrator
        if (_securityConfig.AccountLockoutMinutes == 0)
            return;

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
                if (!foundGame.Succeeded)
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
}