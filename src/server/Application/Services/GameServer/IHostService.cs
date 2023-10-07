using Application.Models.GameServer.Host;
using Application.Models.Web;
using Application.Requests.v1.Hosts;
using Application.Responses.v1.GameServer;
using Domain.Enums.Lifecycle;

namespace Application.Services.GameServer;

public interface IHostService
{
    Task<IResult<HostNewRegisterResponse>> GetNewRegistration();
    Task<IResult<HostRegisterResponse>> Register(HostRegisterRequest request);
    Task<IResult<HostAuthResponse>> GetToken(HostAuthRequest request);
    Task<IResult<IEnumerable<HostSlim>>> GetAllAsync();
    Task<IResult<IEnumerable<HostSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetCountAsync();
    Task<IResult<HostSlim>> GetByIdAsync(Guid id);
    Task<IResult<HostSlim>> GetByHostnameAsync(string hostName);
    Task<IResult<Guid>> CreateAsync(HostCreate createObject);
    Task<IResult> UpdateAsync(HostUpdate updateObject);
    Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId);
    Task<IResult<IEnumerable<HostSlim>>> SearchAsync(string searchText);
    Task<IResult<IEnumerable<HostSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrationsAsync();
    Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrationsPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllActiveRegistrationsAsync();
    Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllInActiveRegistrationsAsync();
    Task<IResult<int>> GetRegistrationCountAsync();
    Task<IResult<HostRegistrationFull>> GetRegistrationByIdAsync(Guid id);
    Task<IResult<HostRegistrationFull>> GetRegistrationByHostIdAsync(Guid hostId);
    Task<IResult<Guid>> CreateRegistrationAsync(HostRegistrationCreate createObject);
    Task<IResult> UpdateRegistrationAsync(HostRegistrationUpdate updateObject);
    Task<IResult<IEnumerable<HostRegistrationFull>>> SearchRegistrationsAsync(string searchText);
    Task<IResult<IEnumerable<HostRegistrationFull>>> SearchRegistrationsPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<IResult<IEnumerable<HostCheckInFull>>> GetAllCheckInsAsync();
    Task<IResult<IEnumerable<HostCheckInFull>>> GetAllCheckInsAfterAsync(DateTime afterDate);
    Task<IResult<IEnumerable<HostCheckInFull>>> GetAllCheckInsPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetCheckInCountAsync();
    Task<IResult<HostCheckInFull>> GetCheckInByIdAsync(int id);
    Task<IResult<IEnumerable<HostCheckInFull>>> GetCheckInByHostIdAsync(Guid id);
    Task<IResult> CreateCheckInAsync(HostCheckInCreate createObject);
    Task<IResult<int>> DeleteAllCheckInsForHostIdAsync(Guid id);
    Task<IResult<int>> DeleteAllOldCheckInsAsync(CleanupTimeframe olderThan);
    Task<IResult<IEnumerable<HostCheckInFull>>> SearchCheckInsAsync(string searchText);
    Task<IResult<IEnumerable<HostCheckInFull>>> SearchCheckInsPaginatedAsync(string searchText, int pageNumber, int pageSize);
}