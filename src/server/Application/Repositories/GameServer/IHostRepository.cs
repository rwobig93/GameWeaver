using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.HostRegistration;
using Application.Models.GameServer.WeaverWork;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;

namespace Application.Repositories.GameServer;

public interface IHostRepository
{
    Task<DatabaseActionResult<IEnumerable<HostDb>>> GetAllAsync();
    Task<DatabaseActionResult<IEnumerable<HostDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<HostDb?>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<HostDb?>> GetByHostnameAsync(string hostName);
    Task<DatabaseActionResult<Guid>> CreateAsync(HostCreateDb createObject);
    Task<DatabaseActionResult> UpdateAsync(HostUpdateDb updateObject);
    Task<DatabaseActionResult> DeleteAsync(Guid id, Guid requestUserId);
    Task<DatabaseActionResult<IEnumerable<HostDb>>> SearchAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<HostDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllRegistrationsAsync();
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllRegistrationsPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllActiveRegistrationsAsync();
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetAllInActiveRegistrationsAsync();
    Task<DatabaseActionResult<int>> GetRegistrationCountAsync();
    Task<DatabaseActionResult<HostRegistrationDb?>> GetRegistrationByIdAsync(Guid id);
    Task<DatabaseActionResult<HostRegistrationDb?>> GetRegistrationByHostIdAsync(Guid hostId);
    Task<DatabaseActionResult<HostRegistrationDb?>> GetRegistrationByHostIdAndKeyAsync(Guid hostId, string key);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> GetActiveRegistrationsByDescriptionAsync(string description);
    Task<DatabaseActionResult<Guid>> CreateRegistrationAsync(HostRegistrationCreate createObject);
    Task<DatabaseActionResult> UpdateRegistrationAsync(HostRegistrationUpdate updateObject);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> SearchRegistrationsAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<HostRegistrationDb>>> SearchRegistrationsPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetAllCheckInsAsync();
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetAllCheckInsAfterAsync(DateTime afterDate);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetAllCheckInsPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCheckInCountAsync();
    Task<DatabaseActionResult<HostCheckInDb?>> GetCheckInByIdAsync(int id);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetChecksInByHostIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> GetCheckInsLatestByHostIdAsync(Guid id, int count);
    Task<DatabaseActionResult> CreateCheckInAsync(HostCheckInCreate createObject);
    Task<DatabaseActionResult<int>> DeleteAllCheckInsForHostIdAsync(Guid id);
    Task<DatabaseActionResult<int>> DeleteAllOldCheckInsAsync(CleanupTimeframe olderThan);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> SearchCheckInsAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<HostCheckInDb>>> SearchCheckInsPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetAllWeaverWorkAsync();
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetAllWeaverWorkPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetWeaverWorkCountAsync();
    Task<DatabaseActionResult<WeaverWorkDb?>> GetWeaverWorkByIdAsync(int id);
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverWorkByHostIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverWaitingWorkByHostIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverAllWaitingWorkByHostIdAsync(Guid id);
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverWorkByTargetTypeAsync(WeaverWorkTarget target);
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverWorkByStatusAsync(WeaverWorkState status);
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> GetWeaverWorkCreatedWithinAsync(DateTime from, DateTime until);
    Task<DatabaseActionResult<int>> CreateWeaverWorkAsync(WeaverWorkCreate createObject);
    Task<DatabaseActionResult> UpdateWeaverWorkAsync(WeaverWorkUpdate updateObject);
    Task<DatabaseActionResult> DeleteWeaverWorkAsync(int id);
    Task<DatabaseActionResult> DeleteWeaverWorkForHostAsync(Guid hostId);
    Task<DatabaseActionResult> DeleteWeaverWorkOlderThanAsync(DateTime olderThan);
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> SearchWeaverWorkAsync(string searchText);
    Task<DatabaseActionResult<IEnumerable<WeaverWorkDb>>> SearchWeaverWorkPaginatedAsync(string searchText, int pageNumber, int pageSize);
}