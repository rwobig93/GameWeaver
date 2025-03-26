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
using Infrastructure.Database.PgSql.GameServer;

namespace Infrastructure.Repositories.PgSql.GameServer;

public class GameRepositoryPgSql : IGameRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTime;

    public GameRepositoryPgSql(ISqlDataService database, ILogger logger, IDateTimeService dateTime)
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
            var allGames = await _database.LoadData<GameDb, dynamic>(GamesTablePgSql.GetAll, new { });
            actionReturn.Succeed(allGames);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTablePgSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameDb?>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> GetBySteamNameAsync(string steamName)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameDb?>> GetByFriendlyNameAsync(string friendlyName)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameDb?>> GetBySteamGameIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameDb?>> GetBySteamToolIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(GameCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> UpdateAsync(GameUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid id, Guid requestUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> SearchAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameDb>>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetAllDevelopersAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<DeveloperDb>>>> GetAllDevelopersPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetDevelopersCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<DeveloperDb?>> GetDeveloperByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<DeveloperDb?>> GetDeveloperByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetDevelopersByGameIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreateDeveloperAsync(DeveloperCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteDeveloperAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> SearchDevelopersAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<DeveloperDb>>>> SearchDevelopersPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetAllPublishersAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<PublisherDb>>>> GetAllPublishersPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetPublishersCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PublisherDb?>> GetPublisherByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PublisherDb?>> GetPublisherByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetPublishersByGameIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreatePublisherAsync(PublisherCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeletePublisherAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> SearchPublishersAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<PublisherDb>>>> SearchPublishersPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetAllGameGenresAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameGenreDb>>>> GetAllGameGenresPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetGameGenresCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameGenreDb?>> GetGameGenreByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameGenreDb?>> GetGameGenreByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetGameGenresByGameIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreateGameGenreAsync(GameGenreCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteGameGenreAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> SearchGameGenresAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameGenreDb>>>> SearchGameGenresPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameUpdateDb>>> GetAllGameUpdatesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameUpdateDb>>>> GetAllGameUpdatesPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<int>> GetGameUpdatesCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<GameUpdateDb?>> GetGameUpdateByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameUpdateDb>>> GetGameUpdatesByGameId(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<Guid>> CreateGameUpdateAsync(GameUpdateCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteGameUpdateAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult> DeleteGameUpdatesForGameIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<IEnumerable<GameUpdateDb>>> SearchGameUpdateAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<GameUpdateDb>>>> SearchGameUpdatePaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }
}