using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.Publishers;
using Application.Models.Web;
using Domain.Contracts;

namespace Application.Services.GameServer;

public interface IGameService
{
    Task<IResult<IEnumerable<GameSlim>>> GetAllAsync();
    Task<IResult<IEnumerable<GameSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetCountAsync();
    Task<IResult<GameSlim>> GetByIdAsync(Guid id);
    Task<IResult<IEnumerable<GameSlim>>> GetBySteamNameAsync(string steamName);
    Task<IResult<GameSlim>> GetByFriendlyNameAsync(string friendlyName);
    Task<IResult<GameSlim>> GetBySteamGameIdAsync(int id);
    Task<IResult<GameSlim>> GetBySteamToolIdAsync(int id);
    Task<IResult<Guid>> CreateAsync(GameCreate createObject);
    Task<IResult> UpdateAsync(GameUpdate updateObject);
    Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId);
    Task<IResult<IEnumerable<GameSlim>>> SearchAsync(string searchText);
    Task<IResult<IEnumerable<GameSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<IResult<IEnumerable<DeveloperSlim>>> GetAllDevelopersAsync();
    Task<IResult<IEnumerable<DeveloperSlim>>> GetAllDevelopersPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetDevelopersCountAsync();
    Task<IResult<DeveloperSlim>> GetDeveloperByIdAsync(Guid id);
    Task<IResult<DeveloperSlim>> GetDeveloperByNameAsync(string name);
    Task<IResult<IEnumerable<DeveloperSlim>>> GetDevelopersByGameIdAsync(Guid id);
    Task<IResult<Guid>> CreateDeveloperAsync(DeveloperCreate createObject);
    Task<IResult> DeleteDeveloperAsync(Guid id);
    Task<IResult<IEnumerable<DeveloperSlim>>> SearchDevelopersAsync(string searchText);
    Task<IResult<IEnumerable<DeveloperSlim>>> SearchDevelopersPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<IResult<IEnumerable<PublisherSlim>>> GetAllPublishersAsync();
    Task<IResult<IEnumerable<PublisherSlim>>> GetAllPublishersPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetPublishersCountAsync();
    Task<IResult<PublisherSlim>> GetPublisherByIdAsync(Guid id);
    Task<IResult<PublisherSlim>> GetPublisherByNameAsync(string name);
    Task<IResult<IEnumerable<PublisherSlim>>> GetPublishersByGameIdAsync(Guid id);
    Task<IResult<Guid>> CreatePublisherAsync(PublisherCreate createObject);
    Task<IResult> DeletePublisherAsync(Guid id);
    Task<IResult<IEnumerable<PublisherSlim>>> SearchPublishersAsync(string searchText);
    Task<IResult<IEnumerable<PublisherSlim>>> SearchPublishersPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<IResult<IEnumerable<GameGenreSlim>>> GetAllGameGenresAsync();
    Task<IResult<IEnumerable<GameGenreSlim>>> GetAllGameGenresPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetGameGenresCountAsync();
    Task<IResult<GameGenreSlim>> GetGameGenreByIdAsync(Guid id);
    Task<IResult<GameGenreSlim>> GetGameGenreByNameAsync(string name);
    Task<IResult<IEnumerable<GameGenreSlim>>> GetGameGenresByGameIdAsync(Guid id);
    Task<IResult<Guid>> CreateGameGenreAsync(GameGenreCreate createObject);
    Task<IResult> DeleteGameGenreAsync(Guid id);
    Task<IResult<IEnumerable<GameGenreSlim>>> SearchGameGenresAsync(string searchText);
    Task<IResult<IEnumerable<GameGenreSlim>>> SearchGameGenresPaginatedAsync(string searchText, int pageNumber, int pageSize);
}