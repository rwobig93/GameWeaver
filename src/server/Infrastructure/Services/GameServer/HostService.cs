using Application.Constants.Communication;
using Application.Models.GameServer.Host;
using Application.Models.Web;
using Application.Repositories.GameServer;
using Application.Requests.v1.GameServer;
using Application.Responses.v1.GameServer;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Enums.Lifecycle;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.GameServer;

public class HostService : IHostService
{
    private readonly IHostRepository _hostRepository;
    private readonly IDateTimeService _dateTime;
    private readonly IRunningServerState _serverState;
    private readonly AppConfiguration _appConfig;

    public HostService(IHostRepository hostRepository, IDateTimeService dateTime, IRunningServerState serverState, IOptions<AppConfiguration> appConfig)
    {
        _hostRepository = hostRepository;
        _dateTime = dateTime;
        _serverState = serverState;
        _appConfig = appConfig.Value;
    }

    public async Task<IResult<HostNewRegisterResponse>> GetNewRegistration(string description)
    {
        var matchingDescriptionRequest = await _hostRepository.GetActiveRegistrationsByDescriptionAsync(description);
        if (!matchingDescriptionRequest.Succeeded)
            return await Result<HostNewRegisterResponse>.FailAsync(matchingDescriptionRequest.ErrorMessage);
        if (matchingDescriptionRequest.Result is null || matchingDescriptionRequest.Result.Any())
            return await Result<HostNewRegisterResponse>.FailAsync("Active registration with matching description already exists," +
                                                                   " please use the existing registration or provide a different description");

        return await Result<HostNewRegisterResponse>.SuccessAsync();
    }

    public async Task<IResult<HostRegisterResponse>> Register(HostRegisterRequest request)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<IResult<HostAuthResponse>> GetToken(HostAuthRequest request)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostSlim>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<HostSlim>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<HostSlim>> GetByHostnameAsync(string hostName)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<Guid>> CreateAsync(HostCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> UpdateAsync(HostUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostSlim>>> SearchAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrationsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrationsPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllActiveRegistrationsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllInActiveRegistrationsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> GetRegistrationCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<HostRegistrationFull>> GetRegistrationByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<HostRegistrationFull>> GetRegistrationByHostIdAsync(Guid hostId)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<Guid>> CreateRegistrationAsync(HostRegistrationCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> UpdateRegistrationAsync(HostRegistrationUpdate updateObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> SearchRegistrationsAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> SearchRegistrationsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> GetAllCheckInsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> GetAllCheckInsAfterAsync(DateTime afterDate)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> GetAllCheckInsPaginatedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> GetCheckInCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<HostCheckInFull>> GetCheckInByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> GetCheckInByHostIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> CreateCheckInAsync(HostCheckInCreate createObject)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> DeleteAllCheckInsForHostIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<int>> DeleteAllOldCheckInsAsync(CleanupTimeframe olderThan)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> SearchCheckInsAsync(string searchText)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> SearchCheckInsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }
}