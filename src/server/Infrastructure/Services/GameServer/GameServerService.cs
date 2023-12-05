using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Application.Models.Web;
using Application.Services.GameServer;

namespace Infrastructure.Services.GameServer;

public class GameServerService : IGameServerService
{
    public async Task<IResult<IEnumerable<GameServerSlim>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<GameServerSlim>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<GameServerSlim>> GetByServerNameAsync(string serverName)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<GameServerSlim>> GetByGameIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<GameServerSlim>> GetByGameProfileIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<GameServerSlim>> GetByHostIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<GameServerSlim>> GetByOwnerIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<Guid>> CreateAsync(GameServerCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> UpdateAsync(GameServerUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> SearchAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameServerSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetAllConfigurationItemsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetAllConfigurationItemsPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> GetConfigurationItemsCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<ConfigurationItemSlim>> GetConfigurationItemByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetConfigurationItemsByGameProfileIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<Guid>> CreateConfigurationItemAsync(ConfigurationItemCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> UpdateConfigurationItemAsync(ConfigurationItemUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> DeleteConfigurationItemAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> SearchConfigurationItemsAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ConfigurationItemSlim>>> SearchConfigurationItemsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetAllLocalResourcesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetAllLocalResourcesPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> GetLocalResourcesCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<LocalResourceSlim>> GetLocalResourceByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesByGameProfileIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesByGameServerIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<Guid>> CreateLocalResourceAsync(LocalResourceCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> UpdateLocalResourceAsync(LocalResourceUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> DeleteLocalResourceAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> SearchLocalResourceAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<LocalResourceSlim>>> SearchLocalResourcePaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetAllGameProfilesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetAllGameProfilesPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> GetGameProfileCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<GameProfileSlim>> GetGameProfileByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<GameProfileSlim>> GetGameProfileByFriendlyNameAsync(string friendlyName)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByGameIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByOwnerIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByServerProcessNameAsync(string serverProcessName)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<Guid>> CreateGameProfileAsync(GameProfileCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> UpdateGameProfileAsync(GameProfileUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> DeleteGameProfileAsync(Guid id, Guid modifyingUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> SearchGameProfilesAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<GameProfileSlim>>> SearchGameProfilesPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetAllModsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetAllModsPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> GetModCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<ModSlim>> GetModByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<ModSlim>> GetModByCurrentHashAsync(string hash)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetModsByFriendlyNameAsync(string friendlyName)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetModsByGameIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetModsBySteamGameIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<ModSlim>> GetModBySteamIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> GetModsBySteamToolIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<Guid>> CreateModAsync(ModCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> UpdateModAsync(ModUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> DeleteModAsync(Guid id, Guid modifyingUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> SearchModsAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<ModSlim>>> SearchModsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }
}