using Application.Constants.Communication;
using Application.Constants.Lifecycle;
using Application.Helpers.GameServer;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Mappers.Lifecycle;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Application.Models.Identity.User;
using Application.Models.Lifecycle;
using Application.Repositories.GameServer;
using Application.Repositories.Identity;
using Application.Repositories.Lifecycle;
using Application.Requests.GameServer.GameProfile;
using Application.Requests.GameServer.LocalResource;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;
using Domain.Enums.Lifecycle;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.GameServer;

public class GameServerService : IGameServerService
{
    private readonly IGameServerRepository _gameServerRepository;
    private readonly IDateTimeService _dateTime;
    private readonly IHostRepository _hostRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IAuditTrailsRepository _auditRepository;
    private readonly IRunningServerState _serverState;
    private readonly ITroubleshootingRecordsRepository _tshootRepository;
    private readonly IOptions<AppConfiguration> _generalConfig;
    private readonly IAppUserRepository _userRepository;
    private readonly IEventService _eventService;
    private readonly INotifyRecordRepository _notifyRecordRepository;
    private readonly IAppPermissionRepository _permissionRepository;

    public GameServerService(IGameServerRepository gameServerRepository, IDateTimeService dateTime, IHostRepository hostRepository, IGameRepository gameRepository,
        IAuditTrailsRepository auditRepository, IRunningServerState serverState, ITroubleshootingRecordsRepository tshootRepository, IOptions<AppConfiguration> generalConfig,
        IAppUserRepository userRepository, IEventService eventService, INotifyRecordRepository notifyRecordRepository, IAppPermissionRepository permissionRepository)
    {
        _gameServerRepository = gameServerRepository;
        _dateTime = dateTime;
        _hostRepository = hostRepository;
        _gameRepository = gameRepository;
        _auditRepository = auditRepository;
        _serverState = serverState;
        _tshootRepository = tshootRepository;
        _generalConfig = generalConfig;
        _userRepository = userRepository;
        _eventService = eventService;
        _notifyRecordRepository = notifyRecordRepository;
        _permissionRepository = permissionRepository;
    }

    private async Task<GameServerDb> FilterNoAccessServer(GameServerDb gameServer, Guid requestUserId)
    {
        if (requestUserId == _serverState.SystemUserId)
        {
            return gameServer;
        }
        
        var userPermissions = (await _permissionRepository.GetAllIncludingRolesForUserAsync(requestUserId)).Result?.ToArray() ?? [];

        if (gameServer.OwnerId == requestUserId || !gameServer.Private || gameServer.PermissionsHaveAccess(userPermissions))
        {
            return gameServer;
        }

        return gameServer.ToNoAccess();
    }

    private async Task<IEnumerable<GameServerDb>> FilterNoAccessServers(IEnumerable<GameServerDb> gameServers, Guid requestUserId)
    {
        if (requestUserId == _serverState.SystemUserId)
        {
            return gameServers;
        }

        List<GameServerDb> filteredServers = [];
        
        var userPermissions = (await _permissionRepository.GetAllIncludingRolesForUserAsync(requestUserId)).Result?.ToArray() ?? [];

        foreach (var gameServer in gameServers)
        {
            if (gameServer.OwnerId == requestUserId || !gameServer.Private || gameServer.PermissionsHaveAccess(userPermissions))
            {
                filteredServers.Add(gameServer);
                continue;
            }
            
            filteredServers.Add(gameServer.ToNoAccess());
        }

        return filteredServers;
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetAllAsync(Guid requestUserId)
    {
        var response = await _gameServerRepository.GetAllAsync();
        if (!response.Succeeded)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(response.ErrorMessage);
        }

        var accessFilteredServers = await FilterNoAccessServers(response.Result ?? [], requestUserId);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(accessFilteredServers.ToSlims());
    }
    
    public async Task<PaginatedResult<IEnumerable<GameServerSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize, Guid requestUserId)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.GetAllPaginatedAsync(pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<GameServerSlim>>.FailAsync(response.ErrorMessage);
        }

        if (response.Result is null)
        {
            return await PaginatedResult<IEnumerable<GameServerSlim>>.SuccessAsync([]);
        }

        var accessFilteredServers = await FilterNoAccessServers(response.Result.Data, requestUserId);

        return await PaginatedResult<IEnumerable<GameServerSlim>>.SuccessAsync(
            accessFilteredServers.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        var request = await _gameServerRepository.GetCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<GameServerSlim?>> GetByIdAsync(Guid id, Guid requestUserId)
    {
        var response = await _gameServerRepository.GetByIdAsync(id);
        if (!response.Succeeded)
        {
            return await Result<GameServerSlim?>.FailAsync(response.ErrorMessage);
        }
        
        if (response.Result is null)
        {
            return await Result<GameServerSlim?>.FailAsync(ErrorMessageConstants.Generic.NotFound);
        }
        
        var permissionFilteredGameserver = await FilterNoAccessServer(response.Result, requestUserId);

        return await Result<GameServerSlim?>.SuccessAsync(permissionFilteredGameserver.ToSlim());
    }

    public async Task<IResult<GameServerSlim?>> GetByServerNameAsync(string serverName, Guid requestUserId)
    {
        var request = await _gameServerRepository.GetByServerNameAsync(serverName);
        if (!request.Succeeded)
        {
            return await Result<GameServerSlim?>.FailAsync(request.ErrorMessage);
        }
        
        if (request.Result is null)
        {
            return await Result<GameServerSlim?>.FailAsync(ErrorMessageConstants.Generic.NotFound);
        }
        
        var permissionFilteredGameserver = await FilterNoAccessServer(request.Result, requestUserId);

        return await Result<GameServerSlim?>.SuccessAsync(permissionFilteredGameserver.ToSlim());
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetByGameIdAsync(Guid id, Guid requestUserId)
    {
        var response = await _gameServerRepository.GetByGameIdAsync(id);
        if (!response.Succeeded)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(response.ErrorMessage);
        }

        var accessFilteredServers = await FilterNoAccessServers(response.Result ?? [], requestUserId);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(accessFilteredServers.ToSlims());
    }

    public async Task<IResult<GameServerSlim?>> GetByGameProfileIdAsync(Guid id, Guid requestUserId)
    {
        var request = await _gameServerRepository.GetByGameProfileIdAsync(id);
        if (!request.Succeeded)
        {
            return await Result<GameServerSlim?>.FailAsync(request.ErrorMessage);
        }
        
        if (request.Result is null)
        {
            return await Result<GameServerSlim?>.FailAsync(ErrorMessageConstants.Generic.NotFound);
        }
        
        var permissionFilteredGameserver = await FilterNoAccessServer(request.Result, requestUserId);

        return await Result<GameServerSlim?>.SuccessAsync(permissionFilteredGameserver.ToSlim());
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetByHostIdAsync(Guid id, Guid requestUserId)
    {
        var response = await _gameServerRepository.GetByHostIdAsync(id);
        if (!response.Succeeded)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(response.ErrorMessage);
        }

        var accessFilteredServers = await FilterNoAccessServers(response.Result ?? [], requestUserId);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(accessFilteredServers.ToSlims());
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetByOwnerIdAsync(Guid id, Guid requestUserId)
    {
        var response = await _gameServerRepository.GetByOwnerIdAsync(id);
        if (!response.Succeeded)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(response.ErrorMessage);
        }

        var accessFilteredServers = await FilterNoAccessServers(response.Result ?? [], requestUserId);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(accessFilteredServers.ToSlims());
    }

    public async Task<IResult<Guid>> CreateAsync(GameServerCreate request, Guid requestUserId)
    {
        var foundGame = await _gameRepository.GetByIdAsync(request.GameId);
        if (foundGame.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Games.NotFound);
        }

        var gameDefaultProfile = await _gameServerRepository.GetGameProfileByIdAsync(foundGame.Result.DefaultGameProfileId);
        if (!gameDefaultProfile.Succeeded || gameDefaultProfile.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.GameProfiles.DefaultProfileNotFound);
        }

        var profileResources = await _gameServerRepository.GetLocalResourcesByGameProfileIdAsync(gameDefaultProfile.Result.Id);
        var profileStartupResources = profileResources.Result?.Where(x => x.Startup).ToList() ?? [];
        if (profileStartupResources.Count == 0)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.GameProfiles.NoStartupResources);
        }

        var foundHost = await _hostRepository.GetByIdAsync(request.HostId);
        if (foundHost.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Hosts.NotFound);
        }
        
        var requestingUser = await _userRepository.GetByIdAsync(requestUserId);
        if (requestingUser.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);
        }

        if (_generalConfig.Value.UseCurrency && foundHost.Result.OwnerId != requestUserId && requestingUser.Result.Currency <= 0)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.GameServers.InsufficientCurrency(_generalConfig.Value.CurrencyName));
        }
        
        if (request.PortGame != 0 && request.PortPeer != 0 && request.PortQuery != 0 && request.PortRcon != 0)
        {
            // Ports were provided, so we'll validate the provided ports are usable
            var allowedHostPorts = NetworkHelpers.GetPortsFromRangeList(foundHost.Result?.ToSlim().AllowedPorts);
            var desiredPorts = request.GetUsedPorts();
            foreach (var port in desiredPorts.Where(port => !allowedHostPorts.Contains(port)))
            {
                return await Result<Guid>.FailAsync($"Port provided isn't allowed on the chosen host: {port}");
            }

            var hostGameServers = await _gameServerRepository.GetByHostIdAsync(foundHost.Result!.Id);
            if (hostGameServers.Result is not null)
            {
                var usedHostPorts = hostGameServers.Result.GetUsedPorts();
                foreach (var port in desiredPorts.Where(port => usedHostPorts.Contains(port)))
                {
                    return await Result<Guid>.FailAsync($"Port provided overlaps with another server on the chosen host: {port}");
                }
            }
        }
        else
        {
            // Ports weren't provided, so we'll attempt to grab the next available ports for the host
            var allowedHostPorts = NetworkHelpers.GetPortsFromRangeList(foundHost.Result?.ToSlim().AllowedPorts);
            List<int> usedPorts = [];

            var hostGameServers = await _gameServerRepository.GetByHostIdAsync(foundHost.Result!.Id);
            if (hostGameServers.Result is not null)
            {
                var usedHostPorts = hostGameServers.Result.GetUsedPorts();
                usedPorts.AddRange(usedHostPorts);
            }
            
            var availablePorts = allowedHostPorts.Except(usedPorts).ToList();
            if (availablePorts.Count < 4)
            {
                return await Result<Guid>.FailAsync("The selected host has run out of available / configured ports, please have the owner or a moderator configure more");
            }

            request.PortGame = availablePorts.ElementAt(0);
            request.PortPeer = availablePorts.ElementAt(1);
            request.PortQuery = availablePorts.ElementAt(2);
            request.PortRcon = availablePorts.ElementAt(3);
        }

        if (request.ParentGameProfileId == Guid.Empty)
        {
            request.ParentGameProfileId = null;
        }
        if (request.ParentGameProfileId is not null)
        {
            var parentGameProfileRequest = await _gameServerRepository.GetGameProfileByIdAsync((Guid)request.ParentGameProfileId);
            if (parentGameProfileRequest.Result is null)
            {
                return await Result<Guid>.FailAsync(ErrorMessageConstants.GameProfiles.ParentProfileNotFound);
            }
        }

        var createdGameProfile = await _gameServerRepository.CreateGameProfileAsync(new GameProfileCreate
        {
            FriendlyName = $"Server Profile - {request.ServerName}",
            OwnerId = request.OwnerId,
            GameId = foundGame.Result.Id,
            CreatedBy = requestUserId,
            CreatedOn = _dateTime.NowDatabaseTime
        });
        if (!createdGameProfile.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, Guid.Empty, requestUserId,
                "Failed to create game profile for server before game server creation", new Dictionary<string, string> {{"Error", createdGameProfile.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        request.GameProfileId = createdGameProfile.Result;
        request.ServerBuildVersion = foundGame.Result.LatestBuildVersion;
        request.CreatedBy = requestUserId;
        request.CreatedOn = _dateTime.NowDatabaseTime;
        
        var gameServerCreate = await _gameServerRepository.CreateAsync(request);
        if (!gameServerCreate.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, Guid.Empty, requestUserId,
            "Created server profile but failed to create game server", new Dictionary<string, string>
            {
                {"Error", gameServerCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var createdGameServer = await _gameServerRepository.GetByIdAsync(gameServerCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, gameServerCreate.Result, requestUserId, AuditAction.Create,
            null, createdGameServer.Result);

        if (_generalConfig.Value.UseCurrency && foundHost.Result.OwnerId != requestUserId)
        {
            var userUpdate = await _userRepository.UpdateAsync(new AppUserUpdate
            {
                Currency = requestingUser.Result.Currency - 1,
                LastModifiedBy = _serverState.SystemUserId,
                LastModifiedOn = _dateTime.NowDatabaseTime
            });
            if (!userUpdate.Succeeded)
            {
                var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, gameServerCreate.Result,
                    requestUserId, "Created game server and profile but failed to update user currency",new Dictionary<string, string>
                    {
                        {"UserId", requestingUser.Result.Id.ToString()},
                        {"Username", requestingUser.Result.Username},
                        {"BeforeCurrency", requestingUser.Result.Currency.ToString()},
                        {"IntendedCurrency", (requestingUser.Result.Currency - 1).ToString()},
                        {"Error", userUpdate.ErrorMessage}
                    });
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
            }
        }

        var gameServerHost = createdGameServer.Result!.ToHost();
        gameServerHost.SteamName = foundGame.Result.SteamName;
        gameServerHost.SteamGameId = foundGame.Result.SteamGameId;
        gameServerHost.SteamToolId = foundGame.Result.SteamToolId;
        gameServerHost.ManualRootUrl = foundGame.Result.ManualVersionUrlDownload;

        var gameServerResources = await GetLocalResourcesForGameServerIdAsync(gameServerHost.Id);
        gameServerHost.Resources.AddRange(gameServerResources.Data.ToHosts(gameServerCreate.Result, foundHost.Result.Os));
        
        var hostInstallRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerInstall, foundHost.Result.Id,
            gameServerHost, requestUserId, _dateTime.NowDatabaseTime);
        if (!hostInstallRequest.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, gameServerCreate.Result,
                requestUserId, "Created game server and profile but failed to send install request to the host",new Dictionary<string, string>
            {
                {"Error", hostInstallRequest.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_serverState, _dateTime, AuditTableName.WeaverWorks, gameServerHost.Id, AuditAction.GameServerAction,
            null, new Dictionary<string, string>
            {
                {"WorkId", hostInstallRequest.Result.ToString()},
                {"Detail", "Game Server Install work request sent"}
            });

        return await Result<Guid>.SuccessAsync(gameServerCreate.Result);
    }

    public async Task<IResult> UpdateAsync(GameServerUpdate request, Guid requestUserId)
    {
        var foundGameServer = await _gameServerRepository.GetByIdAsync(request.Id);
        if (foundGameServer.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        }

        if (request.ParentGameProfileId is not null)
        {
            var foundGame = await _gameRepository.GetByIdAsync(foundGameServer.Result.GameId);
            if (!foundGame.Succeeded)
            {
                return await Result.FailAsync(foundGame.ErrorMessage);
            }

            if (foundGame.Result?.DefaultGameProfileId == request.ParentGameProfileId)
            {
                return await Result.FailAsync(ErrorMessageConstants.GameServers.DefaultProfileAssignment);
            }
        }

        request.LastModifiedOn = _dateTime.NowDatabaseTime;
        request.LastModifiedBy = requestUserId;

        var gameServerUpdate = await _gameServerRepository.UpdateAsync(request);
        if (!gameServerUpdate.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundGameServer.Result.Id,
                requestUserId, "Failed to update game server", new Dictionary<string, string> {{"Error", gameServerUpdate.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var updatedGameServer = await _gameServerRepository.GetByIdAsync(foundGameServer.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, foundGameServer.Result.Id, requestUserId, AuditAction.Update,
            foundGameServer.Result, updatedGameServer.Result);

        return await Result.SuccessAsync();
    }

    /// <summary>
    /// Delete a game server
    /// </summary>
    /// <param name="id">Game server Id</param>
    /// <param name="requestUserId">User ID making the request</param>
    /// <param name="sendHostUninstall">Whether to send an uninstallation request to the game server host</param>
    /// <returns>Success or failure with context messages</returns>
    public async Task<IResult> DeleteAsync(Guid id, Guid requestUserId, bool sendHostUninstall = true)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(id);
        if (foundServer.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        }

        var profileServers = await _gameServerRepository.GetByGameProfileIdAsync(foundServer.Result.GameProfileId);
        if (profileServers.Result is null)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id,
                requestUserId, "Failed to get game server profile servers before deletion", new Dictionary<string, string>
            {
                {"Error", profileServers.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var updateStatusRequest = await _gameServerRepository.UpdateAsync(new GameServerUpdate
        {
            Id = foundServer.Result.Id,
            ServerState = ConnectivityState.Uninstalling,
            LastModifiedBy = requestUserId,
            LastModifiedOn = _dateTime.NowDatabaseTime
        });
        if (!updateStatusRequest.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id, requestUserId,
                "Failed to update server state to uninstalling before game server deletion", new Dictionary<string, string> {{"Error", updateStatusRequest.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        if (!sendHostUninstall)
        {
            var deleteRequest = await _gameServerRepository.DeleteAsync(foundServer.Result.Id, requestUserId);
            if (!deleteRequest.Succeeded)
            {
                var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id, requestUserId,
                    "Failed to delete game server", new Dictionary<string, string> {{"Error", deleteRequest.ErrorMessage}});
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
            }
            
            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, foundServer.Result.Id, requestUserId, AuditAction.Delete,
                foundServer.Result);

            var serverOwner = await _userRepository.GetByIdAsync(foundServer.Result.OwnerId);
            if (serverOwner.Result is null)
            {
                var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id, requestUserId,
                    "Failed to find owner account during game server delete", new Dictionary<string, string> {{"UserID", foundServer.Result.OwnerId.ToString()}});
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
            }

            if (!_generalConfig.Value.UseCurrency) return await Result.SuccessAsync();
            {
                var serverHost = await _hostRepository.GetByIdAsync(foundServer.Result.HostId);
                if (serverHost.Result?.OwnerId != foundServer.Result.OwnerId)
                {
                    var userUpdate = await _userRepository.UpdateAsync(new AppUserUpdate
                    {
                        Currency = serverOwner.Result.Currency + 1,
                        LastModifiedBy = _serverState.SystemUserId,
                        LastModifiedOn = _dateTime.NowDatabaseTime
                    });
                    if (userUpdate.Succeeded) return await Result.SuccessAsync();
                
                    var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id,
                        requestUserId, "Deleted game server and profile but failed to update user currency",new Dictionary<string, string>
                        {
                            {"UserId", serverOwner.Result.Id.ToString()},
                            {"Username", serverOwner.Result.Username},
                            {"BeforeCurrency", serverOwner.Result.Currency.ToString()},
                            {"IntendedCurrency", (serverOwner.Result.Currency + 1).ToString()},
                            {"Error", userUpdate.ErrorMessage}
                        });
                    return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
                }
            }
        }

        var hostDeleteRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerUninstall, foundServer.Result.HostId,
            foundServer.Result.Id, requestUserId, _dateTime.NowDatabaseTime);
        if (hostDeleteRequest.Succeeded) return await Result.SuccessAsync();
        
        var trailId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id, requestUserId,
            "Failed to send host uninstall request for game server deletion", new Dictionary<string, string> {{"Error", hostDeleteRequest.ErrorMessage}});
        return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, $"Please mention this record Id: {trailId.Data}"]);
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> SearchAsync(string searchText)
    {
        var request = await _gameServerRepository.SearchAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameServerSlim>());
    }

    public async Task<PaginatedResult<IEnumerable<GameServerSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize, Guid requestUserId)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.SearchPaginatedAsync(searchText, pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<GameServerSlim>>.FailAsync(response.ErrorMessage);
        }

        if (response.Result is null)
        {
            return await PaginatedResult<IEnumerable<GameServerSlim>>.SuccessAsync([]);
        }

        var accessFilteredServers = await FilterNoAccessServers(response.Result.Data, requestUserId);

        return await PaginatedResult<IEnumerable<GameServerSlim>>.SuccessAsync(
            accessFilteredServers.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetAllConfigurationItemsAsync()
    {
        var request = await _gameServerRepository.GetAllConfigurationItemsAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ConfigurationItemSlim>());
    }

    public async Task<PaginatedResult<IEnumerable<ConfigurationItemSlim>>> GetAllConfigurationItemsPaginatedAsync(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.GetAllConfigurationItemsPaginatedAsync(pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<ConfigurationItemSlim>>.FailAsync(response.ErrorMessage);
        }
        
        if (response.Result?.Data is null)
        {
            return await PaginatedResult<IEnumerable<ConfigurationItemSlim>>.SuccessAsync([]);
        }

        return await PaginatedResult<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(
            response.Result.Data.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult<int>> GetConfigurationItemsCountAsync()
    {
        var request = await _gameServerRepository.GetConfigurationItemsCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<ConfigurationItemSlim>> GetConfigurationItemByIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetConfigurationItemByIdAsync(id);
        if (!request.Succeeded)
            return await Result<ConfigurationItemSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<ConfigurationItemSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<ConfigurationItemSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetConfigurationItemsByLocalResourceIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetConfigurationItemsByLocalResourceIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<Guid>> CreateConfigurationItemAsync(ConfigurationItemCreate request, Guid requestUserId)
    {
        // Default friendly name to key if a short or empty friendly name is provided
        request.FriendlyName = request.FriendlyName.Length <= 3 ? request.Key : request.FriendlyName;
        
        var foundResource = await _gameServerRepository.GetLocalResourceByIdAsync(request.LocalResourceId);
        if (foundResource.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.LocalResources.NotFound);
        }

        var resourceConfig = await _gameServerRepository.GetConfigurationItemsByLocalResourceIdAsync(foundResource.Result.Id);
        var matchingConfig = resourceConfig.Result?.Where(x =>
            x.Category == request.Category &&
            x.Key == request.Key &&
            x.Path == request.Path).ToList() ?? [];
        foreach (var config in matchingConfig)
        {
            if (!config.DuplicateKey)
            {
                return await Result<Guid>.FailAsync(ErrorMessageConstants.ConfigItems.DuplicateConfig);
            }
            
            // Is a duplicate key, so the only thing to verify against is the value since we can't have 2 items with the same key and value
            if (config.Value == request.Value)
            {
                return await Result<Guid>.FailAsync(ErrorMessageConstants.ConfigItems.DuplicateConfig);
            }
        }
        
        var configItemCreate = await _gameServerRepository.CreateConfigurationItemAsync(request);
        if (!configItemCreate.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.ConfigItems, Guid.Empty, requestUserId,
                "Failed to create a configuration item", new Dictionary<string, string>
            {
                {"LocalResourceId", foundResource.Result.Id.ToString()},
                {"Error", configItemCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var createdConfigItem = await _gameServerRepository.GetConfigurationItemByIdAsync(configItemCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.LocalResources, foundResource.Result.Id, requestUserId, AuditAction.Update,
            null, createdConfigItem.Result);

        return await Result<Guid>.SuccessAsync(configItemCreate.Result);
    }

    public async Task<IResult> UpdateConfigurationItemAsync(ConfigurationItemUpdate updateObject, Guid requestUserId)
    {
        var foundConfig = await _gameServerRepository.GetConfigurationItemByIdAsync(updateObject.Id);
        if (foundConfig.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.ConfigItems.NotFound);
        }
        
        var configUpdate = await _gameServerRepository.UpdateConfigurationItemAsync(updateObject);
        if (!configUpdate.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.ConfigItems, foundConfig.Result.Id, requestUserId,
                "Failed to update a configuration item", new Dictionary<string, string>
            {
                {"LocalResourceId", foundConfig.Result.LocalResourceId.ToString()},
                {"Error", configUpdate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var updatedConfigItem = await _gameServerRepository.GetConfigurationItemByIdAsync(foundConfig.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.LocalResources, foundConfig.Result.LocalResourceId, requestUserId,
            AuditAction.Update, foundConfig.Result, updatedConfigItem.Result);
        
        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteConfigurationItemAsync(Guid id, Guid requestUserId)
    {
        var foundConfig = await _gameServerRepository.GetConfigurationItemByIdAsync(id);
        if (foundConfig.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.ConfigItems.NotFound);
        }
        
        var configDelete = await _gameServerRepository.DeleteConfigurationItemAsync(id);
        if (!configDelete.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.ConfigItems, foundConfig.Result.Id, requestUserId,
                "Failed to delete a configuration item", new Dictionary<string, string>
            {
                {"LocalResourceId", foundConfig.Result.LocalResourceId.ToString()},
                {"Error", configDelete.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.LocalResources, foundConfig.Result.LocalResourceId, requestUserId,
            AuditAction.Update, foundConfig.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> SearchConfigurationItemsAsync(string searchText)
    {
        var request = await _gameServerRepository.SearchConfigurationItemsAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ConfigurationItemSlim>());
    }

    public async Task<PaginatedResult<IEnumerable<ConfigurationItemSlim>>> SearchConfigurationItemsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.SearchConfigurationItemsPaginatedAsync(searchText, pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<ConfigurationItemSlim>>.FailAsync(response.ErrorMessage);
        }
        
        if (response.Result?.Data is null)
        {
            return await PaginatedResult<IEnumerable<ConfigurationItemSlim>>.SuccessAsync([]);
        }

        return await PaginatedResult<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(
            response.Result.Data.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetAllLocalResourcesAsync()
    {
        var request = await _gameServerRepository.GetAllLocalResourcesAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<LocalResourceSlim>());
    }

    public async Task<PaginatedResult<IEnumerable<LocalResourceSlim>>> GetAllLocalResourcesPaginatedAsync(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.GetAllLocalResourcesPaginatedAsync(pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<LocalResourceSlim>>.FailAsync(response.ErrorMessage);
        }
        
        if (response.Result?.Data is null)
        {
            return await PaginatedResult<IEnumerable<LocalResourceSlim>>.SuccessAsync([]);
        }

        return await PaginatedResult<IEnumerable<LocalResourceSlim>>.SuccessAsync(
            response.Result.Data.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult<int>> GetLocalResourcesCountAsync()
    {
        var request = await _gameServerRepository.GetLocalResourcesCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    private async Task<IEnumerable<ConfigurationItemSlim>> GetLocalResourceConfigurationItems(LocalResourceSlim resource)
    {
        var configItemsRequest = await _gameServerRepository.GetConfigurationItemsByLocalResourceIdAsync(resource.Id);
        return configItemsRequest.Result is null ? [] : configItemsRequest.Result.ToSlims();
    }

    public async Task<IResult<LocalResourceSlim>> GetLocalResourceByIdAsync(Guid id)
    {
        var localResourceRequest = await _gameServerRepository.GetLocalResourceByIdAsync(id);
        if (!localResourceRequest.Succeeded)
            return await Result<LocalResourceSlim>.FailAsync(localResourceRequest.ErrorMessage);
        if (localResourceRequest.Result is null)
            return await Result<LocalResourceSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var convertedLocalResource = localResourceRequest.Result.ToSlim();
        convertedLocalResource.ConfigSets = await GetLocalResourceConfigurationItems(convertedLocalResource);

        return await Result<LocalResourceSlim>.SuccessAsync(convertedLocalResource);
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesByGameProfileIdAsync(Guid id)
    {
        var localResourcesRequest = await _gameServerRepository.GetLocalResourcesByGameProfileIdAsync(id);
        if (!localResourcesRequest.Succeeded)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(localResourcesRequest.ErrorMessage);
        if (localResourcesRequest.Result is null)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var convertedLocalResource = localResourcesRequest.Result.ToSlims().ToList();
        
        foreach (var resource in convertedLocalResource)
        {
            resource.ConfigSets = await GetLocalResourceConfigurationItems(resource);
        }

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(convertedLocalResource);
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesForGameServerIdAsync(Guid id)
    {
        var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
        if (!gameServerRequest.Succeeded || gameServerRequest.Result is null)
        {
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);
        }

        var gameRequest = await _gameRepository.GetByIdAsync(gameServerRequest.Result.GameId);
        if (!gameRequest.Succeeded || gameRequest.Result is null)
        {
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);
        }
        
        var defaultProfileRequest = await _gameServerRepository.GetGameProfileByIdAsync(gameRequest.Result.DefaultGameProfileId);
        if (!defaultProfileRequest.Succeeded || defaultProfileRequest.Result is null)
        {
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);
        }

        List<LocalResourceSlim> finalResourceList = [];

        var defaultProfileResourcesRequest = await _gameServerRepository.GetLocalResourcesByGameProfileIdAsync(defaultProfileRequest.Result.Id);
        if (defaultProfileResourcesRequest.Result is not null)
        {
            var convertedResources = defaultProfileResourcesRequest.Result.ToSlims().ToList();
            foreach (var resource in convertedResources)
            {
                resource.ConfigSets = await GetLocalResourceConfigurationItems(resource);
            }
            
            finalResourceList.AddRange(convertedResources);
        }

        if (gameServerRequest.Result.ParentGameProfileId is not null)
        {
            var parentProfileResourcesRequest = await _gameServerRepository.GetLocalResourcesByGameProfileIdAsync((Guid)gameServerRequest.Result.ParentGameProfileId);
            if (parentProfileResourcesRequest.Result is not null)
            {
                var convertedResources = parentProfileResourcesRequest.Result.ToSlims().ToList();
                foreach (var resource in convertedResources)
                {
                    resource.ConfigSets = await GetLocalResourceConfigurationItems(resource);
                }
            
                finalResourceList.MergeResources(convertedResources);
            }
        }

        // Set resource and config id's to empty so any resource or config changes will require a newly created resource as an override on the server profile
        foreach (var resource in finalResourceList)
        {
            resource.Id = Guid.Empty;
            resource.GameProfileId = Guid.Empty;

            foreach (var configSet in resource.ConfigSets)
            {
                configSet.Id = Guid.Empty;
                configSet.LocalResourceId = Guid.Empty;
            }
        }

        var serverProfileResourcesRequest = await _gameServerRepository.GetLocalResourcesByGameProfileIdAsync(gameServerRequest.Result.GameProfileId);
        if (serverProfileResourcesRequest.Result is not null)
        {
            var convertedResources = serverProfileResourcesRequest.Result.ToSlims().ToList();
            foreach (var resource in convertedResources)
            {
                resource.ConfigSets = await GetLocalResourceConfigurationItems(resource);
            }
            
            finalResourceList.MergeResources(convertedResources);
        }

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(finalResourceList);
    }

    public async Task<IResult<Guid>> CreateLocalResourceAsync(LocalResourceCreate request, Guid requestUserId)
    {
        var foundProfile = await _gameServerRepository.GetGameProfileByIdAsync(request.GameProfileId);
        if (foundProfile.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.GameProfiles.NotFound);
        }
        
        var profileCurrentResources = await _gameServerRepository.GetLocalResourcesByGameProfileIdAsync(foundProfile.Result.Id);
        profileCurrentResources.Result ??= new List<LocalResourceDb>();
        
        // Ensure we aren't creating a duplicate resource
        var duplicateResources = profileCurrentResources.Result.Where(x =>
            x.Type == request.Type &&
            x.Args == request.Args &&
            ((x.PathWindows.Length > 0 && x.PathWindows == request.PathWindows) ||
             (x.PathLinux.Length > 0 && x.PathLinux == request.PathLinux) ||
             (x.PathMac.Length > 0 && x.PathMac == request.PathMac)));
        if (duplicateResources.Any())
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.LocalResources.DuplicateResource);
        }

        request.CreatedBy = requestUserId;
        request.CreatedOn = _dateTime.NowDatabaseTime;
        
        var resourceCreate = await _gameServerRepository.CreateLocalResourceAsync(request);
        if (!resourceCreate.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.LocalResources, Guid.Empty, requestUserId,
                "Failed to create a local resource", new Dictionary<string, string>
            {
                {"GameProfileId", foundProfile.Result.Id.ToString()},
                {"Error", resourceCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }
        
        var createdResource = await _gameServerRepository.GetLocalResourceByIdAsync(resourceCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.LocalResources, resourceCreate.Result, requestUserId, AuditAction.Create,
            null, createdResource.Result);

        return await Result<Guid>.SuccessAsync(resourceCreate.Result);
    }

    public async Task<IResult> UpdateLocalResourceAsync(LocalResourceUpdateRequest request, Guid requestUserId)
    {
        if (request.Id == Guid.Empty)
        {
            return await CreateLocalResourceAsync(request.ToCreate(), requestUserId);
        }
        
        var foundResource = await _gameServerRepository.GetLocalResourceByIdAsync(request.Id);
        if (foundResource.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.LocalResources.NotFound);
        }
        
        var foundProfile = await _gameServerRepository.GetGameProfileByIdAsync(foundResource.Result.GameProfileId);
        if (foundProfile.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.GameProfiles.NotFound);
        }
        
        var profileCurrentResources = await _gameServerRepository.GetLocalResourcesByGameProfileIdAsync(foundProfile.Result.Id);
        profileCurrentResources.Result ??= new List<LocalResourceDb>();
        
        // Ensure we aren't updating the resource to become a duplicate resource
        var duplicateResources = profileCurrentResources.Result.Where(x =>
            x.Id != request.Id &&
            x.Type == request.Type &&
            x.Args == request.Args &&
            ((x.PathWindows.Length > 0 && x.PathWindows == request.PathWindows) ||
             (x.PathLinux.Length > 0 && x.PathLinux == request.PathLinux) ||
             (x.PathMac.Length > 0 && x.PathMac == request.PathMac)));
        if (duplicateResources.Any())
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.LocalResources.DuplicateResource);
        }

        var convertedRequest = request.ToUpdate();
        convertedRequest.LastModifiedBy = requestUserId;
        convertedRequest.LastModifiedOn = _dateTime.NowDatabaseTime;

        var resourceUpdate = await _gameServerRepository.UpdateLocalResourceAsync(convertedRequest);
        if (!resourceUpdate.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.LocalResources, foundResource.Result.Id, requestUserId,
                "Failed to update a local resource", new Dictionary<string, string>
            {
                {"GameProfileId", foundProfile.Result.Id.ToString()},
                {"Error", resourceUpdate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }
        
        var updatedResource = await _gameServerRepository.GetLocalResourceByIdAsync(foundResource.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.LocalResources, foundResource.Result.Id, requestUserId, AuditAction.Update,
            foundResource.Result, updatedResource.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteLocalResourceAsync(Guid id, Guid requestUserId, bool sendUpdateToHost = true)
    {
        var foundResource = await _gameServerRepository.GetLocalResourceByIdAsync(id);
        if (foundResource.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.LocalResources.NotFound);
        }
        
        var resourceDelete = await _gameServerRepository.DeleteLocalResourceAsync(id);
        if (!resourceDelete.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.LocalResources, foundResource.Result.Id, requestUserId,
                "Failed to delete a local resource", new Dictionary<string, string> {{"Error", resourceDelete.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.LocalResources, foundResource.Result.Id, requestUserId, AuditAction.Delete,
            foundResource.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> UpdateLocalResourceOnGameServerAsync(Guid serverId, Guid resourceId, Guid requestUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(serverId);
        if (foundServer.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        }

        var foundHost = await _hostRepository.GetByIdAsync(foundServer.Result.HostId);
        if (foundHost.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Hosts.NotFound);
        }

        var foundResource = await GetLocalResourceByIdAsync(resourceId);
        if (!foundResource.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id,
                requestUserId, "Failed to get local resource for game server", new Dictionary<string, string>
                {
                    {"Error", foundResource.Messages.ToString() ?? ""}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var gameServerHost = new GameServerToHost
        {
            Id = foundServer.Result.Id,
            Resources = new SerializableList<LocalResourceHost>([foundResource.Data.ToHost(foundServer.Result.Id, foundHost.Result.Os)])
        };
        
        var configUpdateRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerConfigUpdate,
            foundServer.Result.HostId, gameServerHost, requestUserId, _dateTime.NowDatabaseTime);
        if (!configUpdateRequest.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id,
                requestUserId, "Failed to send local resource update request to host for game server", new Dictionary<string, string>
                {
                    {"HostId", foundHost.Result.Id.ToString()},
                    {"Error", configUpdateRequest.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var requester = await _userRepository.GetByIdAsync(requestUserId);
        var notifyRecord = new NotifyRecordCreate
        {
            EntityId = serverId,
            Timestamp = _dateTime.NowDatabaseTime,
            Message = NotifyRecordConstants.GameServerConfigChanged,
            Detail = $"{requester.Result?.Username ?? "Someone"} requested a server reconfiguration change for file {foundResource.Data.Name}"
        };
        await _notifyRecordRepository.CreateAsync(notifyRecord);
        _eventService.TriggerNotify("GameServerServiceLocalResourceUpdate", notifyRecord.ToEvent());
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, AuditAction.GameServerAction,
            null, new Dictionary<string, string>
            {
                {"WorkId", configUpdateRequest.Result.ToString()},
                {"Detail", "Game Server single local resource update work request sent"}
            });

        return await Result.SuccessAsync();
    }

    public async Task<IResult> UpdateAllLocalResourcesOnGameServerAsync(Guid serverId, Guid requestUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(serverId);
        if (foundServer.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        }

        var foundHost = await _hostRepository.GetByIdAsync(foundServer.Result.HostId);
        if (foundHost.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Hosts.NotFound);
        }
        
        var foundResources = await GetLocalResourcesForGameServerIdAsync(foundServer.Result.Id);
        if (!foundResources.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id,
                requestUserId, "Failed to get local resources for game server", new Dictionary<string, string>
                {
                    {"Error", foundResources.Messages.ToString() ?? ""}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var gameServerHost = new GameServerToHost
        {
            Id = foundServer.Result.Id,
            Resources = new SerializableList<LocalResourceHost>(foundResources.Data.ToHosts(foundServer.Result.Id, foundHost.Result.Os))
        };
        
        var configUpdateRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerConfigUpdateFull,
            foundHost.Result.Id, gameServerHost, requestUserId, _dateTime.NowDatabaseTime);
        if (!configUpdateRequest.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id,
                requestUserId, "Failed to send local resource update request to host for game server", new Dictionary<string, string>
                {
                    {"HostId", foundHost.Result.Id.ToString()},
                    {"Error", configUpdateRequest.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var requester = await _userRepository.GetByIdAsync(requestUserId);
        var notifyRecord = new NotifyRecordCreate
        {
            EntityId = serverId,
            Timestamp = _dateTime.NowDatabaseTime,
            Message = NotifyRecordConstants.GameServerConfigChanged,
            Detail = $"{requester.Result?.Username ?? "Someone"} requested a server reconfiguration change for all files"
        };
        await _notifyRecordRepository.CreateAsync(notifyRecord);
        _eventService.TriggerNotify("GameServerServiceAllLocalResourceUpdate", notifyRecord.ToEvent());
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, AuditAction.GameServerAction,
            null, new Dictionary<string, string>
            {
                {"WorkId", configUpdateRequest.Result.ToString()},
                {"Detail", "Game Server all local resource update work request sent"}
            });

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> SearchLocalResourceAsync(string searchText)
    {
        var localResourceRequest = await _gameServerRepository.SearchLocalResourceAsync(searchText);
        if (!localResourceRequest.Succeeded || localResourceRequest.Result is null)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(localResourceRequest.ErrorMessage);

        var convertedLocalResources = localResourceRequest.Result.ToSlims().ToList();
        
        foreach (var resource in convertedLocalResources)
        {
            resource.ConfigSets = await GetLocalResourceConfigurationItems(resource);
        }

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(localResourceRequest.Result?.ToSlims() ?? new List<LocalResourceSlim>());
    }

    public async Task<PaginatedResult<IEnumerable<LocalResourceSlim>>> SearchLocalResourcePaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.SearchLocalResourcePaginatedAsync(searchText, pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<LocalResourceSlim>>.FailAsync(response.ErrorMessage);
        }
        
        if (response.Result?.Data is null)
        {
            return await PaginatedResult<IEnumerable<LocalResourceSlim>>.SuccessAsync([]);
        }

        var convertedLocalResources = response.Result.Data.ToSlims().ToList();
        
        foreach (var resource in convertedLocalResources)
        {
            resource.ConfigSets = await GetLocalResourceConfigurationItems(resource);
        }

        return await PaginatedResult<IEnumerable<LocalResourceSlim>>.SuccessAsync(
            response.Result.Data.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetAllGameProfilesAsync()
    {
        var request = await _gameServerRepository.GetAllGameProfilesAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameProfileSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameProfileSlim>());
    }

    public async Task<PaginatedResult<IEnumerable<GameProfileSlim>>> GetAllGameProfilesPaginatedAsync(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.GetAllGameProfilesPaginatedAsync(pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<GameProfileSlim>>.FailAsync(response.ErrorMessage);
        }
        
        if (response.Result?.Data is null)
        {
            return await PaginatedResult<IEnumerable<GameProfileSlim>>.SuccessAsync([]);
        }

        return await PaginatedResult<IEnumerable<GameProfileSlim>>.SuccessAsync(
            response.Result.Data.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult<int>> GetGameProfileCountAsync()
    {
        var request = await _gameServerRepository.GetGameProfileCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<GameProfileSlim>> GetGameProfileByIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetGameProfileByIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameProfileSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameProfileSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameProfileSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameProfileSlim>> GetGameProfileByFriendlyNameAsync(string friendlyName)
    {
        var request = await _gameServerRepository.GetGameProfileByFriendlyNameAsync(friendlyName);
        if (!request.Succeeded)
            return await Result<GameProfileSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameProfileSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameProfileSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByGameIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetGameProfilesByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<GameProfileSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByOwnerIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetGameProfilesByOwnerIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<GameProfileSlim>>.SuccessAsync(request.Result.ToSlims());
    }
    
    public async Task<IResult<Guid>> CreateGameProfileAsync(GameProfileCreateRequest request, Guid requestUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            request.Name = $"Profile - {Guid.NewGuid()}";
        }
        
        // Game profiles shouldn't have matching friendly names, so we'll enforce that 
        var matchingProfile = await _gameServerRepository.GetGameProfileByFriendlyNameAsync(request.Name);
        if (matchingProfile.Result is not null)
        {
            request.Name = $"{request.Name} - {Guid.NewGuid()}";
        }

        var convertedRequest = request.ToCreate();
        convertedRequest.CreatedBy = requestUserId;
        convertedRequest.CreatedOn = _dateTime.NowDatabaseTime;
        
        var profileCreate = await _gameServerRepository.CreateGameProfileAsync(convertedRequest);
        if (!profileCreate.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameProfiles, Guid.Empty, requestUserId,
                "Failed to create a game profile", new Dictionary<string, string>
            {
                {"ProfileName", convertedRequest.FriendlyName},
                {"Error", profileCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var createdProfile = await _gameServerRepository.GetGameProfileByIdAsync(profileCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameProfiles, profileCreate.Result, requestUserId, AuditAction.Create,
            null, createdProfile.Result);

        return await Result<Guid>.SuccessAsync(profileCreate.Result);
    }

    public async Task<IResult> UpdateGameProfileAsync(GameProfileUpdateRequest request, Guid requestUserId)
    {
        var foundProfile = await _gameServerRepository.GetGameProfileByIdAsync(request.Id);
        if (foundProfile.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameProfiles.NotFound);
        }

        if (request.Name is not null)
        {
            if (request.Name.Length == 0)
            {
                return await Result.FailAsync(ErrorMessageConstants.GameProfiles.EmptyName);
            }
            
            // Game profiles shouldn't have matching friendly names, so we'll enforce that 
            var matchingUsernameRequest = await _gameServerRepository.GetGameProfileByFriendlyNameAsync(request.Name);
            if (matchingUsernameRequest.Result is not null)
            {
                return await Result.FailAsync(ErrorMessageConstants.GameProfiles.MatchingName);
            }
        }

        var convertedRequest = request.ToUpdate();
        convertedRequest.LastModifiedOn = _dateTime.NowDatabaseTime;
        convertedRequest.LastModifiedBy = requestUserId;

        var profileUpdate = await _gameServerRepository.UpdateGameProfileAsync(convertedRequest);
        if (!profileUpdate.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameProfiles, foundProfile.Result.Id, requestUserId,
                "Failed to update game profile", new Dictionary<string, string> {{"Error", profileUpdate.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var updatedProfile = await _gameServerRepository.GetGameProfileByIdAsync(foundProfile.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameProfiles, foundProfile.Result.Id, requestUserId, AuditAction.Update,
            foundProfile.Result, updatedProfile.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteGameProfileAsync(Guid id, Guid requestUserId)
    {
        var foundProfile = await _gameServerRepository.GetGameProfileByIdAsync(id);
        if (foundProfile.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameProfiles.NotFound);
        }
        
        // Don't allow deletion if a default game profile
        var foundGame = await _gameRepository.GetByIdAsync(foundProfile.Result.GameId);
        if (!foundGame.Succeeded || foundGame.Result is null)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameProfiles, foundProfile.Result.Id, requestUserId,
                "Failed to find game before game profile deletion", new Dictionary<string, string> {{"Error", foundGame.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }
        
        if (foundGame.Result.DefaultGameProfileId == foundProfile.Result.Id)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameProfiles.DeleteDefaultProfile);
        }

        // Don't allow deletion if assigned to multiple game servers
        var assignedGameServer = await _gameServerRepository.GetByGameProfileIdAsync(foundProfile.Result.Id);
        if (assignedGameServer is {Succeeded: true, Result: not null})
        {
            List<string> errorMessages =
            [
                ErrorMessageConstants.GameProfiles.AssignedGameServers,
                $"Assigned GameServer: [id]{assignedGameServer.Result.Id} [name]{assignedGameServer.Result.ServerName}"
            ];
            return await Result.FailAsync(errorMessages);
        }

        // Delete all assigned local resources
        var assignedLocalResources = await _gameServerRepository.GetLocalResourcesByGameProfileIdAsync(foundProfile.Result.Id);
        if (assignedLocalResources.Succeeded)
        {
            List<string> errorMessages = [];
            foreach (var resource in assignedLocalResources.Result?.ToList() ?? [])
            {
                var resourceDeleteRequest = await DeleteLocalResourceAsync(resource.Id, requestUserId, false);
                if (!resourceDeleteRequest.Succeeded)
                {
                    errorMessages.AddRange(resourceDeleteRequest.Messages);
                }
            }

            if (errorMessages.Count > 0)
            {
                return await Result.FailAsync(errorMessages);
            }
        }

        var profileDelete = await _gameServerRepository.DeleteGameProfileAsync(id, requestUserId);
        if (!profileDelete.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameProfiles, foundProfile.Result.Id, requestUserId,
                "Failed to delete game profile after deleting assigned resources", new Dictionary<string, string> {{"Error", profileDelete.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameProfiles, foundProfile.Result.Id, requestUserId, AuditAction.Delete,
            foundProfile.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> SearchGameProfilesAsync(string searchText)
    {
        var request = await _gameServerRepository.SearchGameProfilesAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameProfileSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameProfileSlim>());
    }

    public async Task<PaginatedResult<IEnumerable<GameProfileSlim>>> SearchGameProfilesPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.SearchGameProfilesPaginatedAsync(searchText, pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<GameProfileSlim>>.FailAsync(response.ErrorMessage);
        }
        
        if (response.Result?.Data is null)
        {
            return await PaginatedResult<IEnumerable<GameProfileSlim>>.SuccessAsync([]);
        }

        return await PaginatedResult<IEnumerable<GameProfileSlim>>.SuccessAsync(
            response.Result.Data.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetAllModsAsync()
    {
        var request = await _gameServerRepository.GetAllModsAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ModSlim>());
    }

    public async Task<PaginatedResult<IEnumerable<ModSlim>>> GetAllModsPaginatedAsync(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.GetAllModsPaginatedAsync(pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<ModSlim>>.FailAsync(response.ErrorMessage);
        }
        
        if (response.Result?.Data is null)
        {
            return await PaginatedResult<IEnumerable<ModSlim>>.SuccessAsync([]);
        }

        return await PaginatedResult<IEnumerable<ModSlim>>.SuccessAsync(
            response.Result.Data.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult<int>> GetModCountAsync()
    {
        var request = await _gameServerRepository.GetModCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<ModSlim>> GetModByIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetModByIdAsync(id);
        if (!request.Succeeded)
            return await Result<ModSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<ModSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<ModSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<ModSlim>> GetModByCurrentHashAsync(string hash)
    {
        var request = await _gameServerRepository.GetModByCurrentHashAsync(hash);
        if (!request.Succeeded)
            return await Result<ModSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<ModSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<ModSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetModsByFriendlyNameAsync(string friendlyName)
    {
        var request = await _gameServerRepository.GetModsByFriendlyNameAsync(friendlyName);
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<ModSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetModsByGameIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetModsByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<ModSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetModsBySteamGameIdAsync(int id)
    {
        var request = await _gameServerRepository.GetModsBySteamGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<ModSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<ModSlim>> GetModBySteamIdAsync(string id)
    {
        var request = await _gameServerRepository.GetModBySteamIdAsync(id);
        if (!request.Succeeded)
            return await Result<ModSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<ModSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<ModSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetModsBySteamToolIdAsync(int id)
    {
        var request = await _gameServerRepository.GetModsBySteamToolIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<ModSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<Guid>> CreateModAsync(ModCreate request, Guid requestUserId)
    {
        var createMod = await _gameServerRepository.CreateModAsync(request);
        if (!createMod.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.Mods, Guid.Empty, requestUserId,
                "Failed to create a mod", new Dictionary<string, string> {{"Error", createMod.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var createdMod = await _gameServerRepository.GetModByIdAsync(createMod.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Mods, createMod.Result, requestUserId, AuditAction.Create,
            null, createdMod.Result);

        return await Result<Guid>.SuccessAsync(createMod.Result);
    }

    public async Task<IResult> UpdateModAsync(ModUpdate request, Guid requestUserId)
    {
        var foundMod = await _gameServerRepository.GetModByIdAsync(request.Id);
        if (foundMod.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.Mods.NotFound);
        }

        foundMod.Result.LastModifiedOn = _dateTime.NowDatabaseTime;
        foundMod.Result.LastModifiedBy = requestUserId;

        var modUpdate = await _gameServerRepository.UpdateModAsync(request);
        if (!modUpdate.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.Mods, foundMod.Result.Id, requestUserId,
                "Failed to update mod", new Dictionary<string, string> {{"Error", modUpdate.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var updatedMod = await _gameServerRepository.GetModByIdAsync(foundMod.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Mods, foundMod.Result.Id, requestUserId, AuditAction.Update,
            foundMod.Result, updatedMod.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteModAsync(Guid id, Guid requestUserId)
    {
        var foundMod = await _gameServerRepository.GetModByIdAsync(id);
        if (!foundMod.Succeeded || foundMod.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.Mods.NotFound);
        }

        var modDelete = await _gameServerRepository.DeleteModAsync(id, requestUserId);
        if (!modDelete.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.Mods, foundMod.Result.Id, requestUserId,
                "Failed to delete mod", new Dictionary<string, string> {{"Error", modDelete.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Mods, foundMod.Result.Id, requestUserId, AuditAction.Delete, foundMod.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> SearchModsAsync(string searchText)
    {
        var request = await _gameServerRepository.SearchModsAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ModSlim>());
    }

    public async Task<PaginatedResult<IEnumerable<ModSlim>>> SearchModsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var response = await _gameServerRepository.SearchModsPaginatedAsync(searchText, pageNumber, pageSize);
        if (!response.Succeeded)
        {
            return await PaginatedResult<IEnumerable<ModSlim>>.FailAsync(response.ErrorMessage);
        }
        
        if (response.Result?.Data is null)
        {
            return await PaginatedResult<IEnumerable<ModSlim>>.SuccessAsync([]);
        }

        return await PaginatedResult<IEnumerable<ModSlim>>.SuccessAsync(
            response.Result.Data.ToSlims(),
            response.Result.StartPage,
            response.Result.CurrentPage,
            response.Result.EndPage,
            response.Result.TotalCount,
            response.Result.PageSize);
    }

    public async Task<IResult> StartServerAsync(Guid id, Guid requestUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(id);
        if (foundServer.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        }

        var startRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerStart, foundServer.Result.HostId, foundServer.Result.Id,
            requestUserId, _dateTime.NowDatabaseTime);
        if (!startRequest.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id, requestUserId,
                "Failed to send game server start work", new Dictionary<string, string> {{"Error", startRequest.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var requester = await _userRepository.GetByIdAsync(requestUserId);
        var notifyRecord = new NotifyRecordCreate
        {
            EntityId = id,
            Timestamp = _dateTime.NowDatabaseTime,
            Message = NotifyRecordConstants.GameServerStart,
            Detail = $"{requester.Result?.Username ?? "Someone"} requested a server start from state {foundServer.Result.ServerState}"
        };
        await _notifyRecordRepository.CreateAsync(notifyRecord);
        _eventService.TriggerNotify("GameServerServiceStartServer", notifyRecord.ToEvent());
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, AuditAction.GameServerAction,
            null, new Dictionary<string, string>
            {
                {"WorkId", startRequest.Result.ToString()},
                {"Detail", "Game Server Version Update work request sent"}
            });

        return await Result<Guid>.SuccessAsync();
    }

    public async Task<IResult> StopServerAsync(Guid id, Guid requestUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(id);
        if (foundServer.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        }

        var stopRequest =
            await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerStop, foundServer.Result.HostId, foundServer.Result.Id,
                requestUserId, _dateTime.NowDatabaseTime);
        if (!stopRequest.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id, requestUserId,
                "Failed to send game server stop work", new Dictionary<string, string> {{"Error", stopRequest.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var requester = await _userRepository.GetByIdAsync(requestUserId);
        var notifyRecord = new NotifyRecordCreate
        {
            EntityId = id,
            Timestamp = _dateTime.NowDatabaseTime,
            Message = NotifyRecordConstants.GameServerStop,
            Detail = $"{requester.Result?.Username ?? "Someone"} requested a server stop from state {foundServer.Result.ServerState}"
        };
        await _notifyRecordRepository.CreateAsync(notifyRecord);
        _eventService.TriggerNotify("GameServerServiceStopServer", notifyRecord.ToEvent());
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, AuditAction.GameServerAction,
            null, new Dictionary<string, string>
            {
                {"WorkId", stopRequest.Result.ToString()},
                {"Detail", "Game Server Version Update work request sent"}
            });

        return await Result<Guid>.SuccessAsync();
    }

    public async Task<IResult> RestartServerAsync(Guid id, Guid requestUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(id);
        if (foundServer.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        }

        var restartRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerRestart, foundServer.Result.HostId,
            foundServer.Result.Id, requestUserId, _dateTime.NowDatabaseTime);
        if (!restartRequest.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id, requestUserId,
                "Failed to send game server restart work", new Dictionary<string, string> {{"Error", restartRequest.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var requester = await _userRepository.GetByIdAsync(requestUserId);
        var notifyRecord = new NotifyRecordCreate
        {
            EntityId = id,
            Timestamp = _dateTime.NowDatabaseTime,
            Message = NotifyRecordConstants.GameServerRestart,
            Detail = $"{requester.Result?.Username ?? "Someone"} requested a server restart from state {foundServer.Result.ServerState}"
        };
        await _notifyRecordRepository.CreateAsync(notifyRecord);
        _eventService.TriggerNotify("GameServerServiceRestartServer", notifyRecord.ToEvent());
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, AuditAction.GameServerAction,
            null, new Dictionary<string, string>
            {
                {"WorkId", restartRequest.Result.ToString()},
                {"Detail", "Game Server Version Update work request sent"}
            });

        return await Result<Guid>.SuccessAsync();
    }

    public async Task<IResult> UpdateServerAsync(Guid id, Guid requestUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(id);
        if (foundServer.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        }

        var updateRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerUpdate, foundServer.Result.HostId,
            foundServer.Result.Id, requestUserId, _dateTime.NowDatabaseTime);
        if (!updateRequest.Succeeded)
        {
            var tshootId = await _tshootRepository.CreateTroubleshootRecord(_dateTime, TroubleshootEntityType.GameServers, foundServer.Result.Id, requestUserId,
                "Failed to send game server version update work", new Dictionary<string, string> {{"Error", updateRequest.ErrorMessage}});
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data)]);
        }

        var requester = await _userRepository.GetByIdAsync(requestUserId);
        var notifyRecord = new NotifyRecordCreate
        {
            EntityId = id,
            Timestamp = _dateTime.NowDatabaseTime,
            Message = NotifyRecordConstants.GameServerUpdate,
            Detail = $"{requester.Result?.Username ?? "Someone"} requested a server update from state {foundServer.Result.ServerState}"
        };
        await _notifyRecordRepository.CreateAsync(notifyRecord);
        _eventService.TriggerNotify("GameServerServiceUpdateServer", notifyRecord.ToEvent());
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, AuditAction.GameServerAction,
            null, new Dictionary<string, string>
            {
                {"WorkId", updateRequest.Result.ToString()},
                {"Detail", "Game Server Version Update work request sent"}
            });

        return await Result<Guid>.SuccessAsync();
    }
}