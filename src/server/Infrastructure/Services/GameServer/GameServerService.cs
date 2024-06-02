﻿using Application.Constants.Communication;
using Application.Helpers.GameServer;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Application.Repositories.GameServer;
using Application.Repositories.Lifecycle;
using Application.Requests.GameServer.GameProfile;
using Application.Requests.GameServer.GameServer;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Application.Services.System;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.Database;
using Domain.Enums.GameServer;
using Domain.Enums.Lifecycle;

namespace Infrastructure.Services.GameServer;

public class GameServerService : IGameServerService
{
    private readonly IGameServerRepository _gameServerRepository;
    private readonly IDateTimeService _dateTime;
    private readonly IHostRepository _hostRepository;
    private readonly ISerializerService _serializerService;
    private readonly IGameRepository _gameRepository;
    private readonly IEventService _eventService;
    private readonly IAuditTrailsRepository _auditRepository;
    private readonly IRunningServerState _serverState;

    public GameServerService(IGameServerRepository gameServerRepository, IDateTimeService dateTime, IHostRepository hostRepository,
        ISerializerService serializerService, IGameRepository gameRepository, IEventService eventService, IAuditTrailsRepository auditRepository,
        IRunningServerState serverState)
    {
        _gameServerRepository = gameServerRepository;
        _dateTime = dateTime;
        _hostRepository = hostRepository;
        _serializerService = serializerService;
        _gameRepository = gameRepository;
        _eventService = eventService;
        _auditRepository = auditRepository;
        _serverState = serverState;
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetAllAsync()
    {
        var request = await _gameServerRepository.GetAllAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameServerSlim>());
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.GetAllPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameServerSlim>());
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        var request = await _gameServerRepository.GetCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<GameServerSlim>> GetByIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetByIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameServerSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameServerSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameServerSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameServerSlim>> GetByServerNameAsync(string serverName)
    {
        var request = await _gameServerRepository.GetByServerNameAsync(serverName);
        if (!request.Succeeded)
            return await Result<GameServerSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameServerSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameServerSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetByGameIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetByGameProfileIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetByGameProfileIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetByHostIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetByHostIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<GameServerSlim>> GetByOwnerIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetByOwnerIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameServerSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameServerSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameServerSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<Guid>> CreateAsync(GameServerCreateRequest request, Guid requestUserId)
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

        // TODO: Remove ServerProcessName, we look at processes based on directory so we have no need to know the process name anymore
        var createdGameProfile = await _gameServerRepository.CreateGameProfileAsync(new GameProfileCreate
        {
            FriendlyName = $"Server Profile - {request.Name}",
            OwnerId = request.OwnerId,
            GameId = foundGame.Result.Id,
            CreatedBy = requestUserId,
            CreatedOn = _dateTime.NowDatabaseTime
        });
        if (!createdGameProfile.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to create game profile for server before game server creation"},
                {"Error", createdGameProfile.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var convertedRequest = request.ToCreate();
        convertedRequest.GameProfileId = createdGameProfile.Result;
        convertedRequest.ServerBuildVersion = foundGame.Result.LatestBuildVersion;
        convertedRequest.CreatedBy = requestUserId;
        convertedRequest.CreatedOn = _dateTime.NowDatabaseTime;
        
        var gameServerCreate = await _gameServerRepository.CreateAsync(convertedRequest);
        if (!gameServerCreate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Created server profile but failed to create game server"},
                {"Error", gameServerCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdGameServer = await _gameServerRepository.GetByIdAsync(gameServerCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, gameServerCreate.Result, requestUserId, DatabaseActionType.Create,
            null, createdGameServer.Result);

        var gameServerHost = createdGameServer.Result!.ToHost();
        gameServerHost.SteamName = foundGame.Result.SteamName;
        gameServerHost.SteamGameId = foundGame.Result.SteamGameId;
        gameServerHost.SteamToolId = foundGame.Result.SteamToolId;

        var gameServerResources = await GetLocalResourcesForGameServerIdAsync(gameServerHost.Id);
        gameServerHost.Resources.AddRange(gameServerResources.Data.ToHosts(gameServerCreate.Result, foundHost.Result.Os));
        
        var hostInstallRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerInstall, foundHost.Result.Id,
            gameServerHost, requestUserId, _dateTime.NowDatabaseTime);
        if (!hostInstallRequest.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, gameServerCreate.Result, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Created game server and profile but failed to send install request to the host"},
                {"Error", hostInstallRequest.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_serverState, _dateTime, AuditTableName.WeaverWorks, gameServerHost.Id, DatabaseActionType.GameServerAction,
            null, new Dictionary<string, string>
            {
                {"WorkId", hostInstallRequest.Result.ToString()},
                {"Detail", "Game Server Install work request sent"}
            });

        return await Result<Guid>.SuccessAsync(gameServerCreate.Result);
    }

    public async Task<IResult> UpdateAsync(GameServerUpdateRequest request, Guid requestUserId)
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

        var convertedRequest = request.ToUpdate();
        convertedRequest.LastModifiedOn = _dateTime.NowDatabaseTime;
        convertedRequest.LastModifiedBy = requestUserId;

        var gameServerUpdate = await _gameServerRepository.UpdateAsync(convertedRequest);
        if (!gameServerUpdate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundGameServer.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to update game server"},
                {"Error", gameServerUpdate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var updatedGameServer = await _gameServerRepository.GetByIdAsync(foundGameServer.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, foundGameServer.Result.Id, requestUserId, DatabaseActionType.Update,
            foundGameServer.Result, updatedGameServer.Result);

        return await Result.SuccessAsync();
    }

    /// <summary>
    /// Delete a game server
    /// </summary>
    /// <param name="id">Game server Id</param>
    /// <param name="requestUserId">User Id making the request</param>
    /// <param name="sendHostUninstall">Whether to send an uninstall request to the game server host</param>
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to get game server profile servers before deletion"},
                {"Error", profileServers.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        // Delete the assigned game profile if the only assignment is this game server
        if (profileServers.Result.ToList().Count == 1)
        {
            var profileDeleteRequest = await DeleteGameProfileAsync(foundServer.Result.GameProfileId, requestUserId);
            if (!profileDeleteRequest.Succeeded)
            {
                var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id, requestUserId, new Dictionary<string, string>
                {
                    {"Detail", "Failed to delete server profile before game server deletion"},
                    {"Error", string.Join(Environment.NewLine , profileDeleteRequest.Messages)}
                    // TODO: ToString doesn't display, do a join instead {"Error", profileDeleteRequest.Messages.ToString() ?? ""}
                });
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
            }
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to update server state to uninstalling before game server deletion"},
                {"Error", updateStatusRequest.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        if (!sendHostUninstall)
        {
            var deleteRequest = await _gameServerRepository.DeleteAsync(foundServer.Result.Id, requestUserId);
            if (!deleteRequest.Succeeded)
            {
                var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id, requestUserId, new Dictionary<string, string>
                {
                    {"Detail", "Failed to delete game server"},
                    {"Error", deleteRequest.ErrorMessage}
                });
                return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
            }
            
            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, foundServer.Result.Id, requestUserId, DatabaseActionType.Delete,
                foundServer.Result);

            return await Result.SuccessAsync();
        }

        var hostDeleteRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerUninstall, foundServer.Result.HostId,
            foundServer.Result.Id, requestUserId, _dateTime.NowDatabaseTime);
        if (hostDeleteRequest.Succeeded) return await Result.SuccessAsync();
        
        var trailId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id, requestUserId, new Dictionary<string, string>
        {
            {"Detail", "Failed to send host uninstall request for game server deletion"},
            {"Error", hostDeleteRequest.ErrorMessage}
        });
        return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, $"Please mention this record Id: {trailId.Data}"]);
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> SearchAsync(string searchText)
    {
        var request = await _gameServerRepository.SearchAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameServerSlim>());
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.SearchPaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameServerSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameServerSlim>());
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetAllConfigurationItemsAsync()
    {
        var request = await _gameServerRepository.GetAllConfigurationItemsAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ConfigurationItemSlim>());
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetAllConfigurationItemsPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.GetAllConfigurationItemsPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ConfigurationItemSlim>());
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
        // Default friendly name to category/key if a short or empty friendly name is provided
        request.FriendlyName = request.FriendlyName.Length <= 3 ? $"{request.Category}/{request.Key}" : request.FriendlyName;
        
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootConfigItems, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"LocalResourceId", foundResource.Result.Id.ToString()},
                {"Detail", "Failed to create a configuration item"},
                {"Error", configItemCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        // TODO: Add audit properties to LocalResource then add an audit trail for local resource update for the config item's bound local resource
        // TODO: Diff will be config before and after for the local resource

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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootConfigItems, foundConfig.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"LocalResourceId", foundConfig.Result.LocalResourceId.ToString()},
                {"Detail", "Failed to update a configuration item"},
                {"Error", configUpdate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        // TODO: Add audit properties to LocalResource then add an audit trail for local resource update for the config item's bound local resource
        // TODO: Diff will be config before and after for the local resource
        
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootConfigItems, foundConfig.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"LocalResourceId", foundConfig.Result.LocalResourceId.ToString()},
                {"Detail", "Failed to delete a configuration item"},
                {"Error", configDelete.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        // TODO: Add audit properties to LocalResource then add an audit trail for local resource update for the config item's bound local resource
        // TODO: Diff will be config before and after for the local resource

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> SearchConfigurationItemsAsync(string searchText)
    {
        var request = await _gameServerRepository.SearchConfigurationItemsAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ConfigurationItemSlim>());
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> SearchConfigurationItemsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.SearchConfigurationItemsPaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ConfigurationItemSlim>());
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetAllLocalResourcesAsync()
    {
        var request = await _gameServerRepository.GetAllLocalResourcesAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<LocalResourceSlim>());
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetAllLocalResourcesPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.GetAllLocalResourcesPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<LocalResourceSlim>());
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
            
                finalResourceList.AddRange(convertedResources);
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
            // TODO: Add ignore key to ConfigurationItem for priority during inheritance checking
            // TODO: Remove GameServerId from LocalResource since we want all profile config on the profile and not the server
            
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
            (x.PathWindows.Length > 0 && x.PathWindows == request.PathWindows) &&
            (x.PathLinux.Length > 0 && x.PathLinux == request.PathLinux) &&
            (x.PathMac.Length > 0 && x.PathMac == request.PathMac) &&
            x.Args == request.Args);
        if (duplicateResources.Any())
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.LocalResources.DuplicateResource);
        }
        
        // TODO: Decide on data validation framework / method
        request.Extension = request.Extension.Replace(".", "");
        
        var resourceCreate = await _gameServerRepository.CreateLocalResourceAsync(request);
        if (!resourceCreate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootLocalResources, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"GameProfileId", foundProfile.Result.Id.ToString()},
                {"Detail", "Failed to create a local resource"},
                {"Error", resourceCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        // TODO: Add audit properties to LocalResource then add an audit trail for local resource update for the config item's bound local resource

        return await Result<Guid>.SuccessAsync(resourceCreate.Result);
    }

    public async Task<IResult> UpdateLocalResourceAsync(LocalResourceUpdate request, Guid requestUserId)
    {
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
            (x.PathWindows.Length > 0 && x.PathWindows == request.PathWindows) &&
            (x.PathLinux.Length > 0 && x.PathLinux == request.PathLinux) &&
            (x.PathMac.Length > 0 && x.PathMac == request.PathMac) &&
            x.Args == request.Args);
        if (duplicateResources.Any())
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.LocalResources.DuplicateResource);
        }

        var resourceUpdate = await _gameServerRepository.UpdateLocalResourceAsync(request);
        if (!resourceUpdate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootLocalResources, foundResource.Result.Id, requestUserId, new Dictionary<string, 
                string>
            {
                {"GameProfileId", foundProfile.Result.Id.ToString()},
                {"Detail", "Failed to update a local resource"},
                {"Error", resourceUpdate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        // TODO: Add audit properties to LocalResource then add an audit trail for local resource update for the config item's bound local resource

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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootLocalResources, foundResource.Result.Id, requestUserId, new Dictionary<string, 
                string>
            {
                {"Detail", "Failed to delete a local resource"},
                {"Error", resourceDelete.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        // TODO: Add audit properties to LocalResource then add an audit trail for local resource update for the config item's bound local resource

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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id,
                requestUserId, new Dictionary<string, string>
                {
                    {"Detail", "Failed to get local resource for game server"},
                    {"Error", foundResource.Messages.ToString() ?? ""}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id,
                requestUserId, new Dictionary<string, string>
                {
                    {"HostId", foundHost.Result.Id.ToString()},
                    {"Detail", "Failed to send local resource update request to host for game server"},
                    {"Error", configUpdateRequest.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, DatabaseActionType.GameServerAction,
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id,
                requestUserId, new Dictionary<string, string>
                {
                    {"Detail", "Failed to get local resources for game server"},
                    {"Error", foundResources.Messages.ToString() ?? ""}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id,
                requestUserId, new Dictionary<string, string>
                {
                    {"HostId", foundHost.Result.Id.ToString()},
                    {"Detail", "Failed to send local resource update request to host for game server"},
                    {"Error", configUpdateRequest.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, DatabaseActionType.GameServerAction,
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

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> SearchLocalResourcePaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var localResourcesRequest = await _gameServerRepository.SearchLocalResourcePaginatedAsync(searchText, pageNumber, pageSize);
        if (!localResourcesRequest.Succeeded || localResourcesRequest.Result is null)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(localResourcesRequest.ErrorMessage);

        var convertedLocalResources = localResourcesRequest.Result.ToSlims().ToList();
        
        foreach (var resource in convertedLocalResources)
        {
            resource.ConfigSets = await GetLocalResourceConfigurationItems(resource);
        }

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(localResourcesRequest.Result?.ToSlims() ?? new List<LocalResourceSlim>());
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetAllGameProfilesAsync()
    {
        var request = await _gameServerRepository.GetAllGameProfilesAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameProfileSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameProfileSlim>());
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetAllGameProfilesPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.GetAllGameProfilesPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameProfileSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameProfileSlim>());
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

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByServerProcessNameAsync(string serverProcessName)
    {
        var request = await _gameServerRepository.GetGameProfilesByServerProcessNameAsync(serverProcessName);
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameProfiles, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"ProfileName", convertedRequest.FriendlyName},
                {"Detail", "Failed to create a game profile"},
                {"Error", profileCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdProfile = await _gameServerRepository.GetGameProfileByIdAsync(profileCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameProfiles, profileCreate.Result, requestUserId, DatabaseActionType.Create,
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameProfiles, foundProfile.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to update game profile"},
                    {"Error", profileUpdate.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var updatedProfile = await _gameServerRepository.GetGameProfileByIdAsync(foundProfile.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameProfiles, foundProfile.Result.Id, requestUserId, DatabaseActionType.Update,
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameProfiles, foundProfile.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to find game before game profile deletion"},
                    {"Error", foundGame.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        if (foundGame.Result.DefaultGameProfileId == foundProfile.Result.Id)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameProfiles.DeleteDefaultProfile);
        }

        // Don't allow deletion if assigned to multiple game servers
        var assignedGameServers = await _gameServerRepository.GetByGameProfileIdAsync(foundProfile.Result.Id);
        if (assignedGameServers.Succeeded && (assignedGameServers.Result ?? Array.Empty<GameServerDb>()).Count() > 1)
        {
            List<string> errorMessages = [ErrorMessageConstants.GameProfiles.AssignedGameServers];
            errorMessages.AddRange((assignedGameServers.Result ?? Array.Empty<GameServerDb>()).ToList().Select(assignment => $"Assigned GameServer: [id]{assignment.Id} [name]{assignment.ServerName}"));
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameProfiles, foundProfile.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to delete game profile after deleting assigned resources"},
                    {"Error", profileDelete.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameProfiles, foundProfile.Result.Id, requestUserId, DatabaseActionType.Delete,
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

    public async Task<IResult<IEnumerable<GameProfileSlim>>> SearchGameProfilesPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.SearchGameProfilesPaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameProfileSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameProfileSlim>());
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetAllModsAsync()
    {
        var request = await _gameServerRepository.GetAllModsAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ModSlim>());
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetAllModsPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.GetAllModsPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ModSlim>());
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootMods, Guid.Empty, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to create a mod"},
                    {"Error", createMod.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdMod = await _gameServerRepository.GetModByIdAsync(createMod.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Mods, createMod.Result, requestUserId, DatabaseActionType.Create,
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootMods, foundMod.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to update mod"},
                    {"Error", modUpdate.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var updatedMod = await _gameServerRepository.GetModByIdAsync(foundMod.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Mods, foundMod.Result.Id, requestUserId, DatabaseActionType.Update,
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootMods, foundMod.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to delete mod"},
                    {"Error", modDelete.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Mods, foundMod.Result.Id, requestUserId, DatabaseActionType.Delete, foundMod.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> SearchModsAsync(string searchText)
    {
        var request = await _gameServerRepository.SearchModsAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ModSlim>());
    }

    public async Task<IResult<IEnumerable<ModSlim>>> SearchModsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.SearchModsPaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<ModSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<ModSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<ModSlim>());
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to send game server start work"},
                    {"Error", startRequest.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, DatabaseActionType.GameServerAction,
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to send game server stop work"},
                    {"Error", stopRequest.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, DatabaseActionType.GameServerAction,
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to send game server restart work"},
                    {"Error", restartRequest.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, DatabaseActionType.GameServerAction,
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameServers, foundServer.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to send game server version update work"},
                    {"Error", updateRequest.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundServer.Result.Id, requestUserId, DatabaseActionType.GameServerAction,
            null, new Dictionary<string, string>
            {
                {"WorkId", updateRequest.Result.ToString()},
                {"Detail", "Game Server Version Update work request sent"}
            });

        return await Result<Guid>.SuccessAsync();
    }
}