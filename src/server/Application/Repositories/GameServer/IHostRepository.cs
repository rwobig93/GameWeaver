using Domain.DatabaseEntities.GameServer;
using Domain.Models.Database;

namespace Application.Repositories.GameServer;

public interface IHostRepository
{
    Task<DatabaseActionResult<IEnumerable<HostDb>>> GetAllAsync();
    Task<DatabaseActionResult<IEnumerable<HostDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<HostDb>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<HostDb>> GetByHostnameAsync(string hostName);
}