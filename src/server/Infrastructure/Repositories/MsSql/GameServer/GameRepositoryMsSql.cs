using Application.Helpers.Runtime;
using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.GameUpdate;
using Application.Models.GameServer.Publishers;
using Application.Repositories.GameServer;
using Application.Services.Database;
using Application.Services.System;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.GameServer;
using Infrastructure.Database.MsSql.Shared;

namespace Infrastructure.Repositories.MsSql.GameServer;

public class GameRepositoryMsSql : IGameRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTime;

    public GameRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTime)
    {
        _database = database;
        _logger = logger;
        _dateTime = dateTime;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<GameDb>> actionReturn = new();

        try
        {
            var allGames = await _database.LoadData<GameDb, dynamic>(GamesTableMsSql.GetAll, new { });
            actionReturn.Succeed(allGames);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameDb, dynamic>(
                GamesTableMsSql.GetAllPaginated, new {Offset = offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {GamesTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameDb?>> GetByIdAsync(Guid id)
    {
        DatabaseActionResult<GameDb?> actionReturn = new();

        try
        {
            var foundGame = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundGame!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> GetBySteamNameAsync(string steamName)
    {
        DatabaseActionResult<IEnumerable<GameDb>> actionReturn = new();

        try
        {
            var foundGames = await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetBySteamName, new {SteamName = steamName});
            actionReturn.Succeed(foundGames);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetBySteamName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameDb?>> GetByFriendlyNameAsync(string friendlyName)
    {
        DatabaseActionResult<GameDb?> actionReturn = new();

        try
        {
            var foundGame = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetByFriendlyName, new {FriendlyName = friendlyName})).FirstOrDefault();
            actionReturn.Succeed(foundGame!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetByFriendlyName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameDb?>> GetBySteamGameIdAsync(int id)
    {
        DatabaseActionResult<GameDb?> actionReturn = new();

        try
        {
            var foundGame = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetBySteamGameId, new {SteamGameId = id})).FirstOrDefault();
            actionReturn.Succeed(foundGame!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetBySteamGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameDb?>> GetBySteamToolIdAsync(int id)
    {
        DatabaseActionResult<GameDb?> actionReturn = new();

        try
        {
            var foundGame = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetBySteamToolId, new {SteamToolId = id})).FirstOrDefault();
            actionReturn.Succeed(foundGame!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetBySteamToolId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(GameCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(GamesTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateAsync(GameUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(GamesTableMsSql.Update, updateObject);
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid id, Guid requestUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(GamesTableMsSql.Delete, new { Id = id, DeletedBy = requestUserId, DeletedOn = _dateTime.NowDatabaseTime });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> SearchAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<GameDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.Search, new { SearchTerm = searchText });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameDb>>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameDb, dynamic>(
                GamesTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetAllDevelopersAsync()
    {
        DatabaseActionResult<IEnumerable<DeveloperDb>> actionReturn = new();

        try
        {
            var allDevelopers = await _database.LoadData<DeveloperDb, dynamic>(DevelopersTableMsSql.GetAll, new { });
            actionReturn.Succeed(allDevelopers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<DeveloperDb>>>> GetAllDevelopersPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<DeveloperDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<DeveloperDb, dynamic>(
                DevelopersTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetDevelopersCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {DevelopersTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<DeveloperDb?>> GetDeveloperByIdAsync(Guid id)
    {
        DatabaseActionResult<DeveloperDb?> actionReturn = new();

        try
        {
            var foundDeveloper = (await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundDeveloper!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<DeveloperDb?>> GetDeveloperByNameAsync(string name)
    {
        DatabaseActionResult<DeveloperDb?> actionReturn = new();

        try
        {
            var foundDeveloper = (await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.GetByName, new {Name = name})).FirstOrDefault();
            actionReturn.Succeed(foundDeveloper!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetByName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetDevelopersByGameIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<DeveloperDb>> actionReturn = new();

        try
        {
            var foundDeveloper = await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.GetByGameId, new {Id = id});
            actionReturn.Succeed(foundDeveloper);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateDeveloperAsync(DeveloperCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(DevelopersTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteDeveloperAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(DevelopersTableMsSql.Delete, new { Id = id });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> SearchDevelopersAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<DeveloperDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.Search, new { SearchTerm = searchText });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<DeveloperDb>>>> SearchDevelopersPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<DeveloperDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<DeveloperDb, dynamic>(
                DevelopersTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);


            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetAllPublishersAsync()
    {
        DatabaseActionResult<IEnumerable<PublisherDb>> actionReturn = new();

        try
        {
            var allPublishers = await _database.LoadData<PublisherDb, dynamic>(PublishersTableMsSql.GetAll, new { });
            actionReturn.Succeed(allPublishers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<PublisherDb>>>> GetAllPublishersPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<PublisherDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<PublisherDb, dynamic>(
                PublishersTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetPublishersCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {PublishersTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PublisherDb?>> GetPublisherByIdAsync(Guid id)
    {
        DatabaseActionResult<PublisherDb?> actionReturn = new();

        try
        {
            var foundPublisher = (await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundPublisher!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PublisherDb?>> GetPublisherByNameAsync(string name)
    {
        DatabaseActionResult<PublisherDb?> actionReturn = new();

        try
        {
            var foundPublisher = (await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.GetByName, new {Name = name})).FirstOrDefault();
            actionReturn.Succeed(foundPublisher!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetByName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetPublishersByGameIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<PublisherDb>> actionReturn = new();

        try
        {
            var foundPublishers = await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.GetByGameId, new {Id = id});
            actionReturn.Succeed(foundPublishers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreatePublisherAsync(PublisherCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(PublishersTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeletePublisherAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(PublishersTableMsSql.Delete, new { Id = id });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> SearchPublishersAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<PublisherDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.Search, new { SearchTerm = searchText });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<PublisherDb>>>> SearchPublishersPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<PublisherDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<PublisherDb, dynamic>(
                PublishersTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);


            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetAllGameGenresAsync()
    {
        DatabaseActionResult<IEnumerable<GameGenreDb>> actionReturn = new();

        try
        {
            var allGenres = await _database.LoadData<GameGenreDb, dynamic>(GameGenreTableMsSql.GetAll, new { });
            actionReturn.Succeed(allGenres);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameGenreDb>>>> GetAllGameGenresPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameGenreDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameGenreDb, dynamic>(
                GameGenreTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});

            response.UpdatePaginationProperties(pageNumber, pageSize);

            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetGameGenresCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {GameGenreTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameGenreDb?>> GetGameGenreByIdAsync(Guid id)
    {
        DatabaseActionResult<GameGenreDb?> actionReturn = new();

        try
        {
            var foundGenre = (await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundGenre!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameGenreDb?>> GetGameGenreByNameAsync(string name)
    {
        DatabaseActionResult<GameGenreDb?> actionReturn = new();

        try
        {
            var foundGenre = (await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.GetByName, new {Name = name})).FirstOrDefault();
            actionReturn.Succeed(foundGenre!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetByName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetGameGenresByGameIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<GameGenreDb>> actionReturn = new();

        try
        {
            var foundGenres = await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.GetByGameId, new {Id = id});
            actionReturn.Succeed(foundGenres);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateGameGenreAsync(GameGenreCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(GameGenreTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteGameGenreAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(GameGenreTableMsSql.Delete, new { Id = id });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> SearchGameGenresAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<GameGenreDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.Search, new { SearchTerm = searchText });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameGenreDb>>>> SearchGameGenresPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameGenreDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameGenreDb, dynamic>(
                GameGenreTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);


            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameUpdateDb>>> GetAllGameUpdatesAsync()
    {
        DatabaseActionResult<IEnumerable<GameUpdateDb>> actionReturn = new();

        try
        {
            var foundUpdates = await _database.LoadData<GameUpdateDb, dynamic>(GameUpdatesTableMsSql.GetAll, new { });

            actionReturn.Succeed(foundUpdates);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameUpdatesTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameUpdateDb>>>> GetAllGameUpdatesPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameUpdateDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameUpdateDb, dynamic>(
                GameUpdatesTableMsSql.GetAllPaginated, new { Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);


            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameUpdatesTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetGameUpdatesCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {GameUpdatesTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameUpdateDb?>> GetGameUpdateByIdAsync(Guid id)
    {
        DatabaseActionResult<GameUpdateDb?> actionReturn = new();

        try
        {
            var foundUpdate = (await _database.LoadData<GameUpdateDb, dynamic>(
                GameUpdatesTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundUpdate);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameUpdatesTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameUpdateDb>>> GetGameUpdatesByGameId(Guid id)
    {
        DatabaseActionResult<IEnumerable<GameUpdateDb>> actionReturn = new();

        try
        {
            var foundUpdates = await _database.LoadData<GameUpdateDb, dynamic>(
                GameUpdatesTableMsSql.GetByGameId, new {Id = id});
            actionReturn.Succeed(foundUpdates);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameUpdatesTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateGameUpdateAsync(GameUpdateCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(GameUpdatesTableMsSql.Insert, createObject);
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameUpdatesTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteGameUpdateAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(GameUpdatesTableMsSql.Delete, new { Id = id });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameUpdatesTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteGameUpdatesForGameIdAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            await _database.SaveData(GameUpdatesTableMsSql.DeleteForGameId, new { Id = id });
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameUpdatesTableMsSql.DeleteForGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameUpdateDb>>> SearchGameUpdateAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<GameUpdateDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<GameUpdateDb, dynamic>(
                GameUpdatesTableMsSql.Search, new { SearchTerm = searchText });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameUpdatesTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameUpdateDb>>>> SearchGameUpdatePaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameUpdateDb>>> actionReturn = new();

        try
        {
            var offset = PaginationHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var response = await _database.LoadDataPaginated<GameUpdateDb, dynamic>(
                GameUpdatesTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            response.UpdatePaginationProperties(pageNumber, pageSize);


            actionReturn.Succeed(response);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameUpdatesTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }
}