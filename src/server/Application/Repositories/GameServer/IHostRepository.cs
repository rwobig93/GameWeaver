using Application.Models.GameServer.Host;
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
    Task<DatabaseActionResult<Guid>> CreateAsync(HostCreate createObject);
    Task<DatabaseActionResult> UpdateAsync(HostUpdate updateObject);
    Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId);
    Task<DatabaseActionResult<IEnumerable<HostDb>>> SearchAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<HostDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllRegistrationsAsync();
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllRegistrationsPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllActiveRegistrationsAsync();
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllInActiveRegistrationsAsync();
    Task<DatabaseActionResult<int>> GetRegistrationCountAsync();
    Task<DatabaseActionResult<HostRegistrationDb>> GetRegistrationByIdAsync(Guid id);
    Task<DatabaseActionResult<HostRegistrationDb>> GetRegistrationByHostIdAsync(Guid hostId);
    Task<DatabaseActionResult<Guid>> CreateRegistrationAsync(HostRegistrationCreate createObject);
    Task<DatabaseActionResult> UpdateRegistrationAsync(HostRegistrationUpdate updateObject);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> SearchRegistrationsAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> SearchRegistrationsPaginatedAsync(string searchText, int pageNumber, int pageSize);
}