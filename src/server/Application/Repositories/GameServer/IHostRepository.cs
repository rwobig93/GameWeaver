using Application.Models.GameServer.Host;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.Lifecycle;
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
    Task<DatabaseActionResult<HostRegistrationDb>> GetRegistrationByHostIdAndKeyAsync(Guid hostId, string key);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetActiveRegistrationsByDescriptionAsync(string description);
    Task<DatabaseActionResult<Guid>> CreateRegistrationAsync(HostRegistrationCreate createObject);
    Task<DatabaseActionResult> UpdateRegistrationAsync(HostRegistrationUpdate updateObject);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> SearchRegistrationsAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> SearchRegistrationsPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetAllCheckInsAsync();
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetAllCheckInsAfterAsync(DateTime afterDate);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetAllCheckInsPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCheckInCountAsync();
    Task<DatabaseActionResult<HostCheckInDb>> GetCheckInByIdAsync(int id);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetCheckInByHostIdAsync(Guid id);
    Task<DatabaseActionResult> CreateCheckInAsync(HostCheckInCreate createObject);
    Task<DatabaseActionResult<int>> DeleteAllCheckInsForHostIdAsync(Guid id);
    Task<DatabaseActionResult<int>> DeleteAllOldCheckInsAsync(CleanupTimeframe olderThan);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> SearchCheckInsAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> SearchCheckInsPaginatedAsync(string searchText, int pageNumber, int pageSize);
}