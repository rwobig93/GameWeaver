﻿using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.Publishers;
using Domain.DatabaseEntities.GameServer;
using Domain.Models.Database;

namespace Application.Repositories.GameServer;

public interface IGameRepository
{
    Task<DatabaseActionResult<IEnumerable<GameDb>>> GetAllAsync();
    Task<DatabaseActionResult<IEnumerable<GameDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<GameDb>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<GameDb>>> GetBySteamNameAsync(string steamName);
    Task<DatabaseActionResult<GameDb>> GetByFriendlyNameAsync(string friendlyName);
    Task<DatabaseActionResult<GameDb>> GetBySteamGameIdAsync(int id);
    Task<DatabaseActionResult<GameDb>> GetBySteamToolIdAsync(int id);
    Task<DatabaseActionResult<Guid>> CreateAsync(GameCreate createObject);
    Task<DatabaseActionResult> UpdateAsync(GameUpdate updateObject);
    Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId);
    Task<DatabaseActionResult<IEnumerable<GameDb>>> SearchAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<GameDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetAllDevelopersAsync();
    Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetAllDevelopersPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetDevelopersCountAsync();
    Task<DatabaseActionResult<DeveloperDb>> GetDeveloperByIdAsync(Guid id);
    Task<DatabaseActionResult<DeveloperDb>> GetDeveloperByNameAsync(string name);
    Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetDevelopersByGameIdAsync(Guid id);
    Task<DatabaseActionResult<Guid>> CreateDeveloperAsync(DeveloperCreate createObject);
    Task<DatabaseActionResult> DeleteDeveloperAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> SearchDevelopersAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> SearchDevelopersPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetAllPublishersAsync();
    Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetAllPublishersPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetPublishersCountAsync();
    Task<DatabaseActionResult<PublisherDb>> GetPublisherByIdAsync(Guid id);
    Task<DatabaseActionResult<PublisherDb>> GetPublisherByNameAsync(string name);
    Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetPublishersByGameIdAsync(Guid id);
    Task<DatabaseActionResult<Guid>> CreatePublisherAsync(PublisherCreate createObject);
    Task<DatabaseActionResult> DeletePublisherAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<PublisherDb>>> SearchPublishersAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<PublisherDb>>> SearchPublishersPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetAllGameGenresAsync();
    Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetAllGameGenresPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetGameGenresCountAsync();
    Task<DatabaseActionResult<GameGenreDb>> GetGameGenreByIdAsync(Guid id);
    Task<DatabaseActionResult<GameGenreDb>> GetGameGenreByNameAsync(string name);
    Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetGameGenresByGameIdAsync(Guid id);
    Task<DatabaseActionResult<Guid>> CreateGameGenreAsync(PublisherCreate createObject);
    Task<DatabaseActionResult> DeleteGameGenreAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> SearchGameGenresAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> SearchGameGenresPaginatedAsync(string searchText, int pageNumber, int pageSize);
}