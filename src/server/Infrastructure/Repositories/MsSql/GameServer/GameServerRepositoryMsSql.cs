using Application.Helpers.Runtime;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Application.Repositories.GameServer;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
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

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameServerDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameServerDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameServerDb, dynamic>(
                GameServersTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
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

    public async Task<DatabaseActionResult<GameServerDb?>> GetByIdAsync(Guid id)
    {
        DatabaseActionResult<GameServerDb?> actionReturn = new();

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

    public async Task<DatabaseActionResult<GameServerDb?>> GetByServerNameAsync(string serverName)
    {
        DatabaseActionResult<GameServerDb?> actionReturn = new();

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

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetByGameIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<GameServerDb>> actionReturn = new();

        try
        {
            var foundGameServer = (await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetByGameId, new {GameId = id}));
            actionReturn.Succeed(foundGameServer);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetByGameProfileIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<GameServerDb>> actionReturn = new();

        try
        {
            var foundGameServers = await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetByGameProfileId, new {GameProfileId = id});
            actionReturn.Succeed(foundGameServers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetByGameProfileId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetByHostIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<GameServerDb>> actionReturn = new();

        try
        {
            var foundGameServer = await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetByHostId, new {HostId = id});
            actionReturn.Succeed(foundGameServer);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.GetByHostId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameServerDb>>> GetByOwnerIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<GameServerDb>> actionReturn = new();

        try
        {
            var foundGameServers = await _database.LoadData<GameServerDb, dynamic>(
                GameServersTableMsSql.GetByOwnerId, new {OwnerId = id});
            actionReturn.Succeed(foundGameServers);
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
            var createdId = await _database.SaveDataReturnId(GameServersTableMsSql.Insert, createObject);
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
            await _database.SaveData(GameServersTableMsSql.Update, updateObject);
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameServersTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid id, Guid requestUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(GameServersTableMsSql.Delete, new { Id = id, DeletedBy = requestUserId, DeletedOn = _dateTime.NowDatabaseTime });
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

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameServerDb>>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameServerDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameServerDb, dynamic>(
                GameServersTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
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

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<ConfigurationItemDb>>>> GetAllConfigurationItemsPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<ConfigurationItemDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<ConfigurationItemDb, dynamic>(
                ConfigurationItemsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
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

    public async Task<DatabaseActionResult<ConfigurationItemDb?>> GetConfigurationItemByIdAsync(Guid id)
    {
        DatabaseActionResult<ConfigurationItemDb?> actionReturn = new();

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

    public async Task<DatabaseActionResult<IEnumerable<ConfigurationItemDb>>> GetConfigurationItemsByLocalResourceIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<ConfigurationItemDb>> actionReturn = new();

        try
        {
            var foundConfigItem = await _database.LoadData<ConfigurationItemDb, dynamic>(
                ConfigurationItemsTableMsSql.GetByLocalResourceId, new {LocalResourceId = id});
            actionReturn.Succeed(foundConfigItem);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.GetByLocalResourceId.Path, ex.Message);
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

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<ConfigurationItemDb>>>> SearchConfigurationItemsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<ConfigurationItemDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<ConfigurationItemDb, dynamic>(
                ConfigurationItemsTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ConfigurationItemsTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> GetAllLocalResourcesAsync()
    {
        DatabaseActionResult<IEnumerable<LocalResourceDb>> actionReturn = new();

        try
        {
            var allResources = await _database.LoadData<LocalResourceDb, dynamic>(LocalResourcesTableMsSql.GetAll, new { });
            actionReturn.Succeed(allResources);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, LocalResourcesTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<LocalResourceDb>>>> GetAllLocalResourcesPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<LocalResourceDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<LocalResourceDb, dynamic>(
                LocalResourcesTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, LocalResourcesTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetLocalResourcesCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {LocalResourcesTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<LocalResourceDb?>> GetLocalResourceByIdAsync(Guid id)
    {
        DatabaseActionResult<LocalResourceDb?> actionReturn = new();

        try
        {
            var foundResource = (await _database.LoadData<LocalResourceDb, dynamic>(
                LocalResourcesTableMsSql.GetById, new {Id = id})).FirstOrDefault();

            actionReturn.Succeed(foundResource!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, LocalResourcesTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> GetLocalResourcesByGameProfileIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<LocalResourceDb>> actionReturn = new();

        try
        {
            var foundResource = await _database.LoadData<LocalResourceDb, dynamic>(
                LocalResourcesTableMsSql.GetByGameProfileId, new {GameProfileId = id});
            actionReturn.Succeed(foundResource);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, LocalResourcesTableMsSql.GetByGameProfileId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateLocalResourceAsync(LocalResourceCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(LocalResourcesTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, LocalResourcesTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateLocalResourceAsync(LocalResourceUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(LocalResourcesTableMsSql.Update, updateObject);
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, LocalResourcesTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteLocalResourceAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(LocalResourcesTableMsSql.Delete, new { Id = id });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, LocalResourcesTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<LocalResourceDb>>> SearchLocalResourceAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<LocalResourceDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<LocalResourceDb, dynamic>(
                LocalResourcesTableMsSql.Search, new { SearchTerm = searchText });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, LocalResourcesTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<LocalResourceDb>>>> SearchLocalResourcePaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<LocalResourceDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<LocalResourceDb, dynamic>(
                LocalResourcesTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, LocalResourcesTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetAllGameProfilesAsync()
    {
        DatabaseActionResult<IEnumerable<GameProfileDb>> actionReturn = new();

        try
        {
            var allProfiles = await _database.LoadData<GameProfileDb, dynamic>(GameProfilesTableMsSql.GetAll, new { });
            actionReturn.Succeed(allProfiles);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameProfileDb>>>> GetAllGameProfilesPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameProfileDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameProfileDb, dynamic>(
                GameProfilesTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetGameProfileCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {GameProfilesTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameProfileDb?>> GetGameProfileByIdAsync(Guid id)
    {
        DatabaseActionResult<GameProfileDb?> actionReturn = new();

        try
        {
            var foundProfile = (await _database.LoadData<GameProfileDb, dynamic>(
                GameProfilesTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundProfile!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameProfileDb?>> GetGameProfileByFriendlyNameAsync(string friendlyName)
    {
        DatabaseActionResult<GameProfileDb?> actionReturn = new();

        try
        {
            var foundProfile = (await _database.LoadData<GameProfileDb, dynamic>(
                GameProfilesTableMsSql.GetByFriendlyName, new {FriendlyName = friendlyName})).FirstOrDefault();
            actionReturn.Succeed(foundProfile!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.GetByFriendlyName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetGameProfilesByGameIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<GameProfileDb>> actionReturn = new();

        try
        {
            var foundProfile = await _database.LoadData<GameProfileDb, dynamic>(
                GameProfilesTableMsSql.GetByGameId, new {GameId = id});
            actionReturn.Succeed(foundProfile);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> GetGameProfilesByOwnerIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<GameProfileDb>> actionReturn = new();

        try
        {
            var foundProfile = await _database.LoadData<GameProfileDb, dynamic>(
                GameProfilesTableMsSql.GetByOwnerId, new {OwnerId = id});
            actionReturn.Succeed(foundProfile);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.GetByOwnerId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateGameProfileAsync(GameProfileCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(GameProfilesTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateGameProfileAsync(GameProfileUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(GameProfilesTableMsSql.Update, updateObject);
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteGameProfileAsync(Guid id, Guid requestUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(GameProfilesTableMsSql.Delete, new { Id = id, DeletedBy = requestUserId, DeletedOn = _dateTime.NowDatabaseTime });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameProfileDb>>> SearchGameProfilesAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<GameProfileDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<GameProfileDb, dynamic>(
                GameProfilesTableMsSql.Search, new { SearchTerm = searchText });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameProfileDb>>>> SearchGameProfilesPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameProfileDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameProfileDb, dynamic>(
                GameProfilesTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameProfilesTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetAllModsAsync()
    {
        DatabaseActionResult<IEnumerable<ModDb>> actionReturn = new();

        try
        {
            var allMods = await _database.LoadData<ModDb, dynamic>(ModsTableMsSql.GetAll, new { });
            actionReturn.Succeed(allMods);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<ModDb>>>> GetAllModsPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<ModDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<ModDb, dynamic>(
                ModsTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetModCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {ModsTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<ModDb?>> GetModByIdAsync(Guid id)
    {
        DatabaseActionResult<ModDb?> actionReturn = new();

        try
        {
            var foundMod = (await _database.LoadData<ModDb, dynamic>(
                ModsTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundMod!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<ModDb?>> GetModByCurrentHashAsync(string hash)
    {
        DatabaseActionResult<ModDb?> actionReturn = new();

        try
        {
            var foundMod = (await _database.LoadData<ModDb, dynamic>(
                ModsTableMsSql.GetByCurrentHash, new {CurrentHash = hash})).FirstOrDefault();
            actionReturn.Succeed(foundMod!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.GetByCurrentHash.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsByFriendlyNameAsync(string friendlyName)
    {
        DatabaseActionResult<IEnumerable<ModDb>> actionReturn = new();

        try
        {
            var foundMods = await _database.LoadData<ModDb, dynamic>(
                ModsTableMsSql.GetByFriendlyName, new {FriendlyName = friendlyName});
            actionReturn.Succeed(foundMods);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.GetByFriendlyName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsByGameIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<ModDb>> actionReturn = new();

        try
        {
            var foundMods = await _database.LoadData<ModDb, dynamic>(
                ModsTableMsSql.GetByGameId, new {GameId = id});
            actionReturn.Succeed(foundMods);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsBySteamGameIdAsync(int id)
    {
        DatabaseActionResult<IEnumerable<ModDb>> actionReturn = new();

        try
        {
            var foundMods = await _database.LoadData<ModDb, dynamic>(
                ModsTableMsSql.GetBySteamGameId, new {SteamGameId = id});
            actionReturn.Succeed(foundMods);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.GetBySteamGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<ModDb?>> GetModBySteamIdAsync(string id)
    {
        DatabaseActionResult<ModDb?> actionReturn = new();

        try
        {
            var foundMod = (await _database.LoadData<ModDb, dynamic>(
                ModsTableMsSql.GetBySteamId, new {SteamId = id})).FirstOrDefault();
            actionReturn.Succeed(foundMod!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.GetBySteamId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> GetModsBySteamToolIdAsync(int id)
    {
        DatabaseActionResult<IEnumerable<ModDb>> actionReturn = new();

        try
        {
            var foundMods = await _database.LoadData<ModDb, dynamic>(
                ModsTableMsSql.GetBySteamToolId, new {SteamToolId = id});
            actionReturn.Succeed(foundMods);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.GetBySteamToolId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateModAsync(ModCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(ModsTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateModAsync(ModUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(ModsTableMsSql.Update, updateObject);
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteModAsync(Guid id, Guid requestUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(ModsTableMsSql.Delete, new { Id = id, DeletedBy = requestUserId, DeletedOn = _dateTime.NowDatabaseTime });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<ModDb>>> SearchModsAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<ModDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<ModDb, dynamic>(
                ModsTableMsSql.Search, new { SearchTerm = searchText });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<ModDb>>>> SearchModsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<ModDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<ModDb, dynamic>(
                ModsTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, ModsTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }
}