using Application.Constants.Communication;
using Application.Mappers.GameServer;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Application.Models.Web;
using Application.Repositories.GameServer;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.GameServer;

public class GameServerService : IGameServerService
{
    private readonly IGameServerRepository _gameServerRepository;
    private readonly IDateTimeService _dateTime;
    private readonly IRunningServerState _serverState;
    private readonly AppConfiguration _appConfig;

    public GameServerService(IGameServerRepository gameServerRepository, IDateTimeService dateTime, IRunningServerState serverState, IOptions<AppConfiguration> appConfig)
    {
        _gameServerRepository = gameServerRepository;
        _dateTime = dateTime;
        _serverState = serverState;
        _appConfig = appConfig.Value;
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
        var request = await _gameServerRepository.CreateAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        return await Result<Guid>.SuccessAsync(request.Result);
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

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        var findRequest = await _gameServerRepository.GetByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var request = await _gameServerRepository.DeleteAsync(id, modifyingUserId);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

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

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetConfigurationItemsByGameProfileIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetConfigurationItemsByGameProfileIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<Guid>> CreateConfigurationItemAsync(ConfigurationItemCreate createObject)
    {
        var request = await _gameServerRepository.CreateConfigurationItemAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        return await Result<Guid>.SuccessAsync(request.Result);
    }

    public async Task<IResult> UpdateConfigurationItemAsync(ConfigurationItemUpdate updateObject)
    {
        var findRequest = await _gameServerRepository.GetConfigurationItemByIdAsync(updateObject.Id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var request = await _gameServerRepository.UpdateConfigurationItemAsync(findRequest.Result.ToUpdate());
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteConfigurationItemAsync(Guid id)
    {
        var findRequest = await _gameServerRepository.GetConfigurationItemByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var request = await _gameServerRepository.DeleteConfigurationItemAsync(id);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

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

    public async Task<IResult<LocalResourceSlim>> GetLocalResourceByIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetLocalResourceByIdAsync(id);
        if (!request.Succeeded)
            return await Result<LocalResourceSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<LocalResourceSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<LocalResourceSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesByGameProfileIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetLocalResourcesByGameProfileIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesByGameServerIdAsync(Guid id)
    {
        var request = await _gameServerRepository.GetLocalResourcesByGameServerIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(request.Result.ToSlims());
    }

    public async Task<IResult<Guid>> CreateLocalResourceAsync(LocalResourceCreate createObject)
    {
        var request = await _gameServerRepository.CreateLocalResourceAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        return await Result<Guid>.SuccessAsync(request.Result);
    }

    public async Task<IResult> UpdateLocalResourceAsync(LocalResourceUpdate updateObject)
    {
        var findRequest = await _gameServerRepository.GetLocalResourceByIdAsync(updateObject.Id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var request = await _gameServerRepository.UpdateLocalResourceAsync(findRequest.Result.ToUpdate());
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteLocalResourceAsync(Guid id)
    {
        var findRequest = await _gameServerRepository.GetLocalResourceByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var request = await _gameServerRepository.DeleteLocalResourceAsync(id);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> SearchLocalResourceAsync(string searchText)
    {
        var request = await _gameServerRepository.SearchLocalResourceAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<LocalResourceSlim>());
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> SearchLocalResourcePaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameServerRepository.SearchLocalResourcePaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<LocalResourceSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<LocalResourceSlim>());
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
        // Game profiles shouldn't have matching friendly names so we'll enforce that 
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
            // Game profiles shouldn't have matching friendly names so we'll enforce that 
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

        var request = await _gameServerRepository.DeleteAsync(id, modifyingUserId);
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
}