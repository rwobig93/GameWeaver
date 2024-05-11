using Application.Constants.Communication;
using Application.Helpers.GameServer;
using Application.Mappers.GameServer;
using Application.Models.Events;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Application.Repositories.GameServer;
using Application.Services.GameServer;
using Application.Services.System;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;

namespace Infrastructure.Services.GameServer;

public class GameServerService : IGameServerService
{
    public event EventHandler<GameServerStatusEvent>? GameServerStatusChanged; 
    
    private readonly IGameServerRepository _gameServerRepository;
    private readonly IDateTimeService _dateTime;
    private readonly IHostRepository _hostRepository;
    private readonly ISerializerService _serializerService;
    private readonly IGameRepository _gameRepository;

    public GameServerService(IGameServerRepository gameServerRepository, IDateTimeService dateTime, IHostRepository hostRepository,
        ISerializerService serializerService, IGameRepository gameRepository)
    {
        _gameServerRepository = gameServerRepository;
        _dateTime = dateTime;
        _hostRepository = hostRepository;
        _serializerService = serializerService;
        _gameRepository = gameRepository;
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

    public async Task<IResult<GameServerSlim>> GetByGameIdAsync(int id)
    {
        var request = await _gameServerRepository.GetByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameServerSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameServerSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameServerSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameServerSlim>> GetByGameProfileIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetByGameProfileIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameServerSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameServerSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameServerSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameServerSlim>> GetByHostIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetByHostIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameServerSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameServerSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameServerSlim>.SuccessAsync(request.Result.ToSlim());
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

    public async Task<IResult<Guid>> CreateAsync(GameServerCreate createObject)
    {
        var gameRequest = await _gameRepository.GetByIdAsync(createObject.GameId);
        if (!gameRequest.Succeeded || gameRequest.Result is null)
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var parentGameProfileRequest = await _gameServerRepository.GetGameProfileByIdAsync(createObject.GameProfileId);
        if (!parentGameProfileRequest.Succeeded || parentGameProfileRequest.Result is null)
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        // TODO: Add a create game profile from parent method that duplicates all linked entities to the new profile
        // TODO: Allow empty game profile creation w/o parent, then prevent running w/o required properties
        var createdGameProfileRequest = await _gameServerRepository.CreateGameProfileAsync(new GameProfileCreate
        {
            FriendlyName = $"{createObject.ServerName} Profile",
            OwnerId = createObject.OwnerId,
            GameId = gameRequest.Result.Id,
            ServerProcessName = parentGameProfileRequest.Result.ServerProcessName,
            CreatedBy = createObject.CreatedBy,
            CreatedOn = createObject.CreatedOn,
            LastModifiedBy = null,
            LastModifiedOn = null,
            IsDeleted = false,
            DeletedOn = null
        });
        if (!createdGameProfileRequest.Succeeded)
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Generic.ContactAdmin);

        createObject.GameProfileId = createdGameProfileRequest.Result;
        
        var gameServerRequest = await _gameServerRepository.CreateAsync(createObject);
        if (!gameServerRequest.Succeeded)
            return await Result<Guid>.FailAsync(gameServerRequest.ErrorMessage);

        var createdGameserverRequest = await _gameServerRepository.GetByIdAsync(gameServerRequest.Result);
        if (!createdGameserverRequest.Succeeded || createdGameserverRequest.Result is null)
            return await Result<Guid>.FailAsync(createdGameserverRequest.ErrorMessage);

        var gameServerHost = createdGameserverRequest.Result.ToHost();
        gameServerHost.ServerProcessName = parentGameProfileRequest.Result.ServerProcessName;
        gameServerHost.SteamName = gameRequest.Result.SteamName;
        gameServerHost.SteamGameId = gameRequest.Result.SteamGameId;
        gameServerHost.SteamToolId = gameRequest.Result.SteamToolId;
        
        var hostInstallRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerInstall, createObject.HostId,
            gameServerHost, createObject.CreatedBy, _dateTime.NowDatabaseTime);
        if (!hostInstallRequest.Succeeded)
            return await Result<Guid>.FailAsync(hostInstallRequest.ErrorMessage);

        return await Result<Guid>.SuccessAsync(gameServerRequest.Result);
    }

    public async Task<IResult> UpdateAsync(GameServerUpdate updateObject)
    {
        var findRequest = await _gameServerRepository.GetByIdAsync(updateObject.Id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        findRequest.Result.LastModifiedOn = _dateTime.NowDatabaseTime;

        var request = await _gameServerRepository.UpdateAsync(findRequest.Result.ToUpdate());
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        if (updateObject.ServerState is not null)
        {
            GameServerStatusChanged?.Invoke(this, findRequest.Result.ToStatusEvent());
        }

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        var findRequest = await _gameServerRepository.GetByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var hostDeleteRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerUninstall, findRequest.Result.HostId,
            findRequest.Result.Id, modifyingUserId, _dateTime.NowDatabaseTime);
        if (!hostDeleteRequest.Succeeded)
            return await Result<Guid>.FailAsync(hostDeleteRequest.ErrorMessage);

        return await Result.SuccessAsync();
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

    public async Task<IResult<Guid>> CreateConfigurationItemAsync(ConfigurationItemCreate createObject)
    {
        var localResourceRequest = await _gameServerRepository.GetLocalResourceByIdAsync(createObject.LocalResourceId);
        if (!localResourceRequest.Succeeded || localResourceRequest.Result is null)
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Generic.NotFound);
        
        var createRequest = await _gameServerRepository.CreateConfigurationItemAsync(createObject);
        if (!createRequest.Succeeded)
            return await Result<Guid>.FailAsync(createRequest.ErrorMessage);

        return await Result<Guid>.SuccessAsync(createRequest.Result);
    }

    public async Task<IResult> UpdateConfigurationItemAsync(ConfigurationItemUpdate updateObject)
    {
        var findRequest = await _gameServerRepository.GetConfigurationItemByIdAsync(updateObject.Id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var updateRequest = await _gameServerRepository.UpdateConfigurationItemAsync(findRequest.Result.ToUpdate());
        if (!updateRequest.Succeeded)
            return await Result.FailAsync(updateRequest.ErrorMessage);
        
        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteConfigurationItemAsync(Guid id, Guid modifyingUserId)
    {
        var findRequest = await _gameServerRepository.GetConfigurationItemByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);
        
        var deleteRequest = await _gameServerRepository.DeleteConfigurationItemAsync(id);
        if (!deleteRequest.Succeeded)
            return await Result.FailAsync(deleteRequest.ErrorMessage);

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

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesByGameServerIdAsync(Guid id)
    {
        var localResourcesRequest = await _gameServerRepository.GetLocalResourcesByGameServerIdAsync(id);
        if (!localResourcesRequest.Succeeded)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(localResourcesRequest.ErrorMessage);
        if (localResourcesRequest.Result is null)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var convertedLocalResource = localResourcesRequest.Result.ToSlims().ToList();
        
        foreach (var resource in convertedLocalResource)
        {
            resource.ConfigSets = await GetLocalResourceConfigurationItems(resource);
        }

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(localResourcesRequest.Result.ToSlims());
    }

    public async Task<IResult<Guid>> CreateLocalResourceAsync(LocalResourceCreate createObject, Guid modifyingUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(createObject.GameServerId);
        if (!foundServer.Succeeded || foundServer.Result is null)
            return await Result<Guid>.FailAsync(ErrorMessageConstants.GameServers.NotFound);

        var foundProfile = await _gameServerRepository.GetGameProfileByIdAsync(createObject.GameProfileId);
        if (!foundProfile.Succeeded || foundProfile.Result is null)
            return await Result<Guid>.FailAsync(ErrorMessageConstants.GameProfiles.NotFound);
        
        var currentResources = await _gameServerRepository.GetLocalResourcesByGameServerIdAsync(foundServer.Result.Id);
        if (!currentResources.Succeeded)
            return await Result<Guid>.FailAsync(currentResources.ErrorMessage);
        currentResources.Result ??= new List<LocalResourceDb>();
        
        // Ensure we aren't creating a duplicate resource
        var duplicateResources = currentResources.Result.Where(x =>
            x.Type == createObject.Type &&
            x.Path == createObject.Path &&
            x.Args == createObject.Args);
        if (duplicateResources.Any())
            return await Result<Guid>.FailAsync(ErrorMessageConstants.LocalResources.DuplicateResource);
        
        // TODO: Decide on data validation framework / method
        createObject.Extension = createObject.Extension.Replace(".", "");
        
        var createRequest = await _gameServerRepository.CreateLocalResourceAsync(createObject);
        if (!createRequest.Succeeded)
            return await Result<Guid>.FailAsync(createRequest.ErrorMessage);

        // Send local resource to the host game server to enforce state
        var configUpdateRequest = await UpdateLocalResourceOnGameServerAsync(createRequest.Result, modifyingUserId);
        if (!configUpdateRequest.Succeeded)
            return await Result<Guid>.FailAsync(configUpdateRequest.Messages);

        return await Result<Guid>.SuccessAsync(createRequest.Result);
    }

    public async Task<IResult> UpdateLocalResourceAsync(LocalResourceUpdate updateObject, Guid modifyingUserId)
    {
        var findRequest = await _gameServerRepository.GetLocalResourceByIdAsync(updateObject.Id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var updateRequest = await _gameServerRepository.UpdateLocalResourceAsync(findRequest.Result.ToUpdate());
        if (!updateRequest.Succeeded)
            return await Result.FailAsync(updateRequest.ErrorMessage);

        // Send local resource to the host game server to enforce state
        var configUpdateRequest = await UpdateLocalResourceOnGameServerAsync(findRequest.Result.Id, modifyingUserId);
        if (!configUpdateRequest.Succeeded)
            return await Result<Guid>.FailAsync(configUpdateRequest.Messages);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteLocalResourceAsync(Guid id, Guid modifyingUserId)
    {
        var findRequest = await _gameServerRepository.GetLocalResourceByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);
        
        var foundServer = await _gameServerRepository.GetByIdAsync(findRequest.Result.GameServerId);
        if (!foundServer.Succeeded || foundServer.Result is null)
            return await Result<Guid>.FailAsync(ErrorMessageConstants.GameServers.NotFound);

        var deleteRequest = await _gameServerRepository.DeleteLocalResourceAsync(id);
        if (!deleteRequest.Succeeded)
            return await Result.FailAsync(deleteRequest.ErrorMessage);

        // Send local resource to the host game server to enforce state
        var configUpdateRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerConfigDelete, foundServer.Result.HostId,
            findRequest.Result.ToHost(), modifyingUserId, _dateTime.NowDatabaseTime);
        if (!configUpdateRequest.Succeeded)
            return await Result<Guid>.FailAsync(configUpdateRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> UpdateLocalResourceOnGameServerAsync(Guid id, Guid modifyingUserId)
    {
        var findRequest = await GetLocalResourceByIdAsync(id);
        if (!findRequest.Succeeded)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);
        
        var foundServer = await _gameServerRepository.GetByIdAsync(findRequest.Data.GameServerId);
        if (!foundServer.Succeeded || foundServer.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        
        var configUpdateRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerConfigUpdate, foundServer.Result.HostId,
            findRequest.Data.ToHost(), modifyingUserId, _dateTime.NowDatabaseTime);
        if (!configUpdateRequest.Succeeded)
            return await Result.FailAsync(configUpdateRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> UpdateAllLocalResourcesOnGameServerAsync(Guid serverId, Guid modifyingUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(serverId);
        if (!foundServer.Succeeded || foundServer.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
        
        // TODO: Framework for Local resources: game.DefaultGameProfile, overwrite by gameServer.ParentGameProfile, root is gameServer.GameProfile
        
        var findRequest = await GetLocalResourcesByGameServerIdAsync(serverId);
        if (!findRequest.Succeeded)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);
        
        var configUpdateRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerConfigUpdateFull, foundServer.Result.HostId,
            findRequest.Data.ToHosts(), modifyingUserId, _dateTime.NowDatabaseTime);
        if (!configUpdateRequest.Succeeded)
            return await Result.FailAsync(configUpdateRequest.ErrorMessage);

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

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByGameIdAsync(int id)
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

    public async Task<IResult<Guid>> CreateGameProfileAsync(GameProfileCreate createObject)
    {
        // Game profiles shouldn't have matching friendly names, so we'll enforce that 
        var matchingUsernameRequest = await _gameServerRepository.GetGameProfileByFriendlyNameAsync(createObject.FriendlyName);
        if (matchingUsernameRequest.Result is not null)
            createObject.FriendlyName = $"{createObject.FriendlyName} - {Guid.NewGuid()}";
        
        var request = await _gameServerRepository.CreateGameProfileAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        return await Result<Guid>.SuccessAsync(request.Result);
    }

    public async Task<IResult> UpdateGameProfileAsync(GameProfileUpdate updateObject)
    {
        var findRequest = await _gameServerRepository.GetGameProfileByIdAsync(updateObject.Id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        if (!string.IsNullOrWhiteSpace(updateObject.FriendlyName))
        {
            // Game profiles shouldn't have matching friendly names, so we'll enforce that 
            var matchingUsernameRequest = await _gameServerRepository.GetGameProfileByFriendlyNameAsync(updateObject.FriendlyName);
            if (matchingUsernameRequest.Result is not null)
                return await Result.FailAsync(ErrorMessageConstants.GameProfiles.MatchingName);
        }
        
        findRequest.Result.LastModifiedOn = _dateTime.NowDatabaseTime;

        var request = await _gameServerRepository.UpdateGameProfileAsync(findRequest.Result.ToUpdate());
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteGameProfileAsync(Guid id, Guid modifyingUserId)
    {
        var findRequest = await _gameServerRepository.GetGameProfileByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var request = await _gameServerRepository.DeleteGameProfileAsync(id, modifyingUserId);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

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

    public async Task<IResult<Guid>> CreateModAsync(ModCreate createObject)
    {
        var request = await _gameServerRepository.CreateModAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        return await Result<Guid>.SuccessAsync(request.Result);
    }

    public async Task<IResult> UpdateModAsync(ModUpdate updateObject)
    {
        var findRequest = await _gameServerRepository.GetModByIdAsync(updateObject.Id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        findRequest.Result.LastModifiedOn = _dateTime.NowDatabaseTime;

        var request = await _gameServerRepository.UpdateModAsync(findRequest.Result.ToUpdate());
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteModAsync(Guid id, Guid modifyingUserId)
    {
        var findRequest = await _gameServerRepository.GetModByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var request = await _gameServerRepository.DeleteModAsync(id, modifyingUserId);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

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

    public async Task<IResult> StartServerAsync(Guid id, Guid modifyingUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(id);
        if (!foundServer.Succeeded || foundServer.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var startRequest =
            await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerStart, foundServer.Result.HostId, foundServer.Result.Id,
                modifyingUserId, _dateTime.NowDatabaseTime);
        if (!startRequest.Succeeded)
            return await Result<Guid>.FailAsync(startRequest.ErrorMessage);

        return await Result<Guid>.SuccessAsync();
    }

    public async Task<IResult> StopServerAsync(Guid id, Guid modifyingUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(id);
        if (!foundServer.Succeeded || foundServer.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var stopRequest =
            await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerStop, foundServer.Result.HostId, foundServer.Result.Id,
                modifyingUserId, _dateTime.NowDatabaseTime);
        if (!stopRequest.Succeeded)
            return await Result<Guid>.FailAsync(stopRequest.ErrorMessage);

        return await Result<Guid>.SuccessAsync();
    }

    public async Task<IResult> RestartServerAsync(Guid id, Guid modifyingUserId)
    {
        var foundServer = await _gameServerRepository.GetByIdAsync(id);
        if (!foundServer.Succeeded || foundServer.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var restartRequest = await _hostRepository.SendWeaverWork(WeaverWorkTarget.GameServerRestart, foundServer.Result.HostId,
            foundServer.Result.Id, modifyingUserId, _dateTime.NowDatabaseTime);
        if (!restartRequest.Succeeded)
            return await Result<Guid>.FailAsync(restartRequest.ErrorMessage);

        return await Result<Guid>.SuccessAsync();
    }
}