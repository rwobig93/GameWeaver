﻿using Application.Helpers.GameServer;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Developers;
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
    private readonly IGameRepository _gameRepository;

    public JobManager(ILogger logger, IAppUserRepository userRepository, IAppAccountService accountService, IDateTimeService dateTime,
        IOptions<SecurityConfiguration> securityConfig, IAuditTrailService auditService, IOptions<LifecycleConfiguration> lifecycleConfig,
        IGameServerService gameServerService, IGameService gameService, ISteamApiService steamApiService, IRunningServerState serverState, IGameRepository gameRepository)
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
            {
                _logger.Error("Audit cleanup failed: {Error}", auditCleanup.Messages);
            }
            
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

    public async Task DailySteamSync()
    {
        var allSteamApps = await _steamApiService.GetAllApps();
        if (!allSteamApps.Succeeded)
        {
            _logger.Error("Failed to get all steam apps from the Steam API: {Error}", allSteamApps.Messages);
            return;
        }
        
        // TODO: Make filter terms configurable
        var serverApps = allSteamApps.Data.Where(x => x.Name.Contains("server", StringComparison.CurrentCultureIgnoreCase));
        foreach (var serverApp in serverApps)
        {
            var matchingGame = await _gameService.GetBySteamToolIdAsync(serverApp.AppId);
            if (matchingGame.Data is not null)
            {
                _logger.Verbose("Found matching game with tool id from steam: [{ToolId}]{GameName} => {GameId}",
                    matchingGame.Data.SteamToolId, matchingGame.Data.SteamName, matchingGame.Data.Id);
                continue;
            }

            var serverAppNameSanitized = serverApp.Name.SanitizeFromSteam();
            var baseGame = allSteamApps.Data.FirstOrDefault(x =>
                x.AppId != serverApp.AppId &&
                x.Name.Contains(serverAppNameSanitized));

            var gameCreate = await _gameService.CreateAsync(new GameCreateRequest
            {
                Name = serverAppNameSanitized,
                SteamGameId = baseGame?.AppId ?? 0,
                SteamToolId = serverApp.AppId
            }, _serverState.SystemUserId);
            if (!gameCreate.Succeeded)
            {
                _logger.Error("Failed to create server app game from steam: [{ToolId}]{Error}", serverApp.AppId, gameCreate.Messages);
                continue;
            }
            
            _logger.Information("Created missing server app from steam: [{ToolId}]{GameName} => {GameId}",
                serverApp.AppId, serverApp.Name, gameCreate.Data);

            if (baseGame is null)
            {
                _logger.Warning("Unable to find base game for server app from steam: [{ToolId}]{GameName} => {GameId}",
                    serverApp.AppId, serverApp.Name, gameCreate.Data);
                continue;
            }
            
            var baseGameDetails = await _steamApiService.GetAppDetail(baseGame.AppId);
            if (baseGameDetails.Data is null)
            {
                _logger.Error("Failed to get base game details from steam: [{ToolId}/{GameId}]{Error}",
                    serverApp.AppId, baseGame.AppId, gameCreate.Messages);
                continue;
            }

            var convertedGameUpdate = baseGameDetails.Data.ToUpdate(gameCreate.Data);
            convertedGameUpdate.LastModifiedBy = _serverState.SystemUserId;
            convertedGameUpdate.LastModifiedOn = _dateTime.NowDatabaseTime;
            
            var gameUpdate = await _gameRepository.UpdateAsync(convertedGameUpdate);
            if (!gameUpdate.Succeeded)
            {
                _logger.Error("Failed to get update game with details from steam: [{ToolId}/{GameId}]{Error}",
                    serverApp.AppId, baseGame.AppId, gameCreate.Messages);
                continue;
            }
            
            foreach (var publisher in baseGameDetails.Data.Publishers)
            {
                var createPublisher = await _gameService.CreatePublisherAsync(new PublisherCreate
                {
                    GameId = gameCreate.Data,
                    Name = publisher
                }, _serverState.SystemUserId);
                if (!createPublisher.Succeeded)
                {
                    _logger.Error("Failed to create publisher for game: [{ToolId}/{GameId}]{Error}",
                        serverApp.AppId, baseGame.AppId, gameCreate.Messages);
                }
            }

            foreach (var developer in baseGameDetails.Data.Developers)
            {
                var createDeveloper = await _gameService.CreateDeveloperAsync(new DeveloperCreate
                {
                    GameId = gameCreate.Data,
                    Name = developer
                }, _serverState.SystemUserId);
                if (!createDeveloper.Succeeded)
                {
                    _logger.Error("Failed to create developer for game: [{ToolId}/{GameId}]{Error}",
                        serverApp.AppId, baseGame.AppId, gameCreate.Messages);
                }
            }

            foreach (var genre in baseGameDetails.Data.Genres)
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
                        serverApp.AppId, baseGame.AppId, gameCreate.Messages);
                }
            }
            
            _logger.Information("Successfully synchronized game from steam: [{ToolId}/{GameId}]", serverApp.AppId, baseGame.AppId);
        }
    }
}