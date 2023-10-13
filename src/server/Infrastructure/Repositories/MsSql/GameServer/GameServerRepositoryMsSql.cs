using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Application.Repositories.GameServer;
using Domain.DatabaseEntities.GameServer;
using Domain.Models.Database;

namespace Infrastructure.Repositories.MsSql.GameServer;

public class GameServerRepositoryMsSql : IGameServerRepository
{
    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByServerNameAsync(string serverName)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByGameIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByGameProfileIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByHostIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByOwnerIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(GameServerCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> UpdateAsync(GameServerUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> SearchAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetAllConfigurationItemsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetAllConfigurationItemsPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetConfigurationItemsCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<ConfigurationItemDb>> GetConfigurationItemByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetConfigurationItemsByGameProfileIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreateConfigurationItemAsync(ConfigurationItemCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> UpdateConfigurationItemAsync(ConfigurationItemUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteConfigurationItemAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> SearchConfigurationItemsAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> SearchConfigurationItemsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> GetAllLocalResourcesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> GetAllLocalResourcesPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetLocalResourcesCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<LocalResourceDb>> GetLocalResourceByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> GetLocalResourcesByGameProfileIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> GetLocalResourcesByGameServerIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreateLocalResourceAsync(LocalResourceCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> UpdateLocalResourceAsync(LocalResourceUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteLocalResourceAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> SearchLocalResourceAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> SearchLocalResourcePaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetAllGameProfilesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetAllGameProfilesPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetGameProfileCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameProfileDb>> GetGameProfileByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameProfileDb>> GetGameProfileByFriendlyNameAsync(string friendlyName)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetGameProfilesByGameIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetGameProfilesByOwnerIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetGameProfilesByServerProcessNameAsync(string serverProcessName)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreateGameProfileAsync(GameProfileCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> UpdateGameProfileAsync(GameProfileUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteGameProfileAsync(Guid id, Guid modifyingUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> SearchGameProfilesAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> SearchGameProfilesPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetAllModsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetAllModsPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetModCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<ModDb>> GetModByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<ModDb>> GetModByCurrentHashAsync(string hash)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsByFriendlyNameAsync(string friendlyName)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsByGameIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsBySteamGameIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<ModDb>> GetModBySteamIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsBySteamToolIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreateModAsync(ModCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> UpdateModAsync(ModUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteModAsync(Guid id, Guid modifyingUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> SearchModsAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> SearchModsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }
}