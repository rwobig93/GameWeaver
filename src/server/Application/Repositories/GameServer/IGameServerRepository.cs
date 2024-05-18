﻿using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Domain.DatabaseEntities.GameServer;
using Domain.Models.Database;

namespace Application.Repositories.GameServer;

public interface IGameServerRepository
{
    Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetAllAsync();
    Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<GameServerDb>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<GameServerDb>> GetByServerNameAsync(string serverName);
    Task<DatabaseActionResult<GameServerDb>> GetByGameIdAsync(int id);
    Task<DatabaseActionResult<GameServerDb>> GetByGameProfileIdAsync(Guid id);
    Task<DatabaseActionResult<GameServerDb>> GetByHostIdAsync(Guid id);
    Task<DatabaseActionResult<GameServerDb>> GetByOwnerIdAsync(Guid id);
    Task<DatabaseActionResult<Guid>> CreateAsync(GameServerCreate createObject);
    Task<DatabaseActionResult> UpdateAsync(GameServerUpdate updateObject);
    Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId);
    Task<DatabaseActionResult<IEnumerable<GameServerDb>>> SearchAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<GameServerDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetAllConfigurationItemsAsync();
    Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetAllConfigurationItemsPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetConfigurationItemsCountAsync();
    Task<DatabaseActionResult<ConfigurationItemDb>> GetConfigurationItemByIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetConfigurationItemsByLocalResourceIdAsync(Guid id);
    Task<DatabaseActionResult<Guid>> CreateConfigurationItemAsync(ConfigurationItemCreate createObject);
    Task<DatabaseActionResult> UpdateConfigurationItemAsync(ConfigurationItemUpdate updateObject);
    Task<DatabaseActionResult> DeleteConfigurationItemAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> SearchConfigurationItemsAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> SearchConfigurationItemsPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> GetAllLocalResourcesAsync();
    Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> GetAllLocalResourcesPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetLocalResourcesCountAsync();
    Task<DatabaseActionResult<LocalResourceDb>> GetLocalResourceByIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> GetLocalResourcesByGameProfileIdAsync(Guid id);
    Task<DatabaseActionResult<Guid>> CreateLocalResourceAsync(LocalResourceCreate createObject);
    Task<DatabaseActionResult> UpdateLocalResourceAsync(LocalResourceUpdate updateObject);
    Task<DatabaseActionResult> DeleteLocalResourceAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> SearchLocalResourceAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> SearchLocalResourcePaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetAllGameProfilesAsync();
    Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetAllGameProfilesPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetGameProfileCountAsync();
    Task<DatabaseActionResult<GameProfileDb>> GetGameProfileByIdAsync(Guid id);
    Task<DatabaseActionResult<GameProfileDb>> GetGameProfileByFriendlyNameAsync(string friendlyName);
    Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetGameProfilesByGameIdAsync(int id);
    Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetGameProfilesByOwnerIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetGameProfilesByServerProcessNameAsync(string serverProcessName);
    // TODO: Ensure friendly name is validated, these should all be unique on creation and updates
    Task<DatabaseActionResult<Guid>> CreateGameProfileAsync(GameProfileCreate createObject);
    Task<DatabaseActionResult> UpdateGameProfileAsync(GameProfileUpdate updateObject);
    Task<DatabaseActionResult> DeleteGameProfileAsync(Guid id, Guid modifyingUserId);
    Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> SearchGameProfilesAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> SearchGameProfilesPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<ModDb>>> GetAllModsAsync();
    Task<DatabaseActionResult<IEnumerable<ModDb>>> GetAllModsPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetModCountAsync();
    Task<DatabaseActionResult<ModDb>> GetModByIdAsync(Guid id);
    Task<DatabaseActionResult<ModDb>> GetModByCurrentHashAsync(string hash);
    Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsByFriendlyNameAsync(string friendlyName);
    Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsByGameIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsBySteamGameIdAsync(int id);
    Task<DatabaseActionResult<ModDb>> GetModBySteamIdAsync(string id);
    Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsBySteamToolIdAsync(int id);
    Task<DatabaseActionResult<Guid>> CreateModAsync(ModCreate createObject);
    Task<DatabaseActionResult> UpdateModAsync(ModUpdate updateObject);
    Task<DatabaseActionResult> DeleteModAsync(Guid id, Guid modifyingUserId);
    Task<DatabaseActionResult<IEnumerable<ModDb>>> SearchModsAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<ModDb>>> SearchModsPaginatedAsync(string searchText, int pageNumber, int pageSize);
}