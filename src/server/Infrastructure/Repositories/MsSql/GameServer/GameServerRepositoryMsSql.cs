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
using Application.Services.Database;
using Application.Services.System;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.Database;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.GameServer;
using Infrastructure.Database.MsSql.Shared;

namespace Infrastructure.Repositories.MsSql.GameServer;

public class GameServerRepositoryMsSql : IGameServerRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTime;
    private readonly IAuditTrailsRepository _auditRepository;

    public GameServerRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTime, IAuditTrailsRepository auditRepository)
    {
        _database = database;
        _logger = logger;
        _dateTime = dateTime;
        _auditRepository = auditRepository;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<GameServerDb>> actionReturn = new();

        try
        {
            var allGameServers = await _database.LoadData<GameServerDb, dynamic>(GameServersTableMsSql.GetAll, new { });
            actionReturn.Succeed(allGameServers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<GameServerDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allGameServers = await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(allGameServers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {GameServersTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByIdAsync(Guid id)
    {
        DatabaseActionResult<GameServerDb> actionReturn = new();

        try
        {
            var foundGameServer = (await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundGameServer!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByServerNameAsync(string serverName)
    {
        DatabaseActionResult<GameServerDb> actionReturn = new();

        try
        {
            var foundGameServer = (await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetByServerName, new {ServerName = serverName})).FirstOrDefault();
            actionReturn.Succeed(foundGameServer!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetByServerName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByGameIdAsync(int id)
    {
        DatabaseActionResult<GameServerDb> actionReturn = new();

        try
        {
            var foundGameServer = (await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetByGameId, new {GameId = id})).FirstOrDefault();
            actionReturn.Succeed(foundGameServer!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByGameProfileIdAsync(Guid id)
    {
        DatabaseActionResult<GameServerDb> actionReturn = new();

        try
        {
            var foundGameServer = (await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetByGameProfileId, new {GameProfileId = id})).FirstOrDefault();
            actionReturn.Succeed(foundGameServer!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetByGameProfileId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByHostIdAsync(Guid id)
    {
        DatabaseActionResult<GameServerDb> actionReturn = new();

        try
        {
            var foundGameServer = (await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetByHostId, new {HostId = id})).FirstOrDefault();
            actionReturn.Succeed(foundGameServer!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetByHostId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameServerDb>> GetByOwnerIdAsync(Guid id)
    {
        DatabaseActionResult<GameServerDb> actionReturn = new();

        try
        {
            var foundGameServer = (await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetByOwnerId, new {OwnerId = id})).FirstOrDefault();
            actionReturn.Succeed(foundGameServer!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetByOwnerId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(GameServerCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createObject.CreatedOn = _dateTime.NowDatabaseTime;

            var createdId = await _database.SaveDataReturnId(GameServersTableMsSql.Insert, createObject);

            var foundHost = await GetByIdAsync(createdId);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, foundHost.Result!.Id,
                createObject.CreatedBy, DatabaseActionType.Create, null, foundHost.Result!.ToSlim());

            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateAsync(GameServerUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var beforeObject = (await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetById, new {updateObject.Id})).FirstOrDefault();
            
            await _database.SaveData(GameServersTableMsSql.Update, updateObject);
            
            var afterObject = (await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetById, new {updateObject.Id})).FirstOrDefault();

            var updateDiff = AuditHelpers.GetAuditDiff(beforeObject, afterObject);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, beforeObject!.Id,
                updateObject.LastModifiedBy.GetFromNullable(), DatabaseActionType.Update, updateDiff.Before, updateDiff.After);
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var foundGameServer = await GetByIdAsync(id);
            if (!foundGameServer.Succeeded || foundGameServer.Result is null)
                throw new Exception(foundGameServer.ErrorMessage);
            var gameServerUpdate = foundGameServer.Result.ToUpdate();
            
            // Update user w/ a property that is modified so we get the last updated on/by for the deleting user
            gameServerUpdate.LastModifiedBy = modifyingUserId;
            await UpdateAsync(gameServerUpdate);
            await _database.SaveData(GameServersTableMsSql.Delete, 
                new { Id = id, DeletedOn = _dateTime.NowDatabaseTime });

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, id,
                gameServerUpdate.LastModifiedBy.GetFromNullable(), DatabaseActionType.Delete, gameServerUpdate);
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> SearchAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<GameServerDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.Search, new { SearchTerm = searchText });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<GameServerDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var searchResults = await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetAllConfigurationItemsAsync()
    {
        DatabaseActionResult<IEnumerable<ConfigurationItemDb>> actionReturn = new();

        try
        {
            var allConfigItems = await _database.LoadData<ConfigurationItemDb, dynamic>(ConfigurationItemsTableMsSql.GetAll, new { });
            actionReturn.Succeed(allConfigItems);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetAllConfigurationItemsPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<ConfigurationItemDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allConfigItems = await _database.LoadData<ConfigurationItemDb, dynamic>(
                ConfigurationItemsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(allConfigItems);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetConfigurationItemsCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {ConfigurationItemsTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<ConfigurationItemDb>> GetConfigurationItemByIdAsync(Guid id)
    {
        DatabaseActionResult<ConfigurationItemDb> actionReturn = new();

        try
        {
            var foundConfigItem = (await _database.LoadData<ConfigurationItemDb, dynamic>(
                ConfigurationItemsTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundConfigItem!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetConfigurationItemsByGameProfileIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<ConfigurationItemDb>> actionReturn = new();

        try
        {
            var foundConfigItem = await _database.LoadData<ConfigurationItemDb, dynamic>(
                ConfigurationItemsTableMsSql.GetByGameProfileId, new {GameProfileId = id});
            actionReturn.Succeed(foundConfigItem);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.GetByGameProfileId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateConfigurationItemAsync(ConfigurationItemCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(ConfigurationItemsTableMsSql.Insert, createObject);

            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateConfigurationItemAsync(ConfigurationItemUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(ConfigurationItemsTableMsSql.Update, updateObject);
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteConfigurationItemAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var foundConfigItem = await GetConfigurationItemByIdAsync(id);
            if (!foundConfigItem.Succeeded || foundConfigItem.Result is null)
                throw new Exception(foundConfigItem.ErrorMessage);
            
            await _database.SaveData(ConfigurationItemsTableMsSql.Delete, new { Id = id });
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> SearchConfigurationItemsAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<ConfigurationItemDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<ConfigurationItemDb, dynamic>(
                ConfigurationItemsTableMsSql.Search, new { SearchTerm = searchText });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
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