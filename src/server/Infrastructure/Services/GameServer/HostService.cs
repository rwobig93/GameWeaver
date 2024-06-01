using System.Security.Claims;
using Application.Constants.Communication;
using Application.Constants.GameServer;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Auth;
using Application.Helpers.Identity;
using Application.Helpers.Web;
using Application.Mappers.GameServer;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.HostRegistration;
using Application.Models.GameServer.WeaverWork;
using Application.Models.Identity.Permission;
using Application.Repositories.GameServer;
using Application.Requests.v1.GameServer;
using Application.Responses.v1.GameServer;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;
using Domain.Enums.Lifecycle;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.GameServer;

public class HostService : IHostService
{
    private readonly IHostRepository _hostRepository;
    private readonly IDateTimeService _dateTime;
    private readonly IRunningServerState _serverState;
    private readonly AppConfiguration _appConfig;
    private readonly SecurityConfiguration _securityConfig;
    private readonly ILogger _logger;
    private readonly IGameServerRepository _gameServerRepository;
    private readonly ISerializerService _serializerService;
    private readonly IEventService _eventService;
    private readonly IGameRepository _gameRepository;

    public HostService(IHostRepository hostRepository, IDateTimeService dateTime, IRunningServerState serverState, IOptions<AppConfiguration> appConfig,
        IOptions<SecurityConfiguration> securityConfig, ILogger logger, IGameServerRepository gameServerRepository, ISerializerService serializerService,
        IEventService eventService, IGameRepository gameRepository)
    {
        _hostRepository = hostRepository;
        _dateTime = dateTime;
        _serverState = serverState;
        _logger = logger;
        _gameServerRepository = gameServerRepository;
        _serializerService = serializerService;
        _eventService = eventService;
        _gameRepository = gameRepository;
        _securityConfig = securityConfig.Value;
        _appConfig = appConfig.Value;
    }

    public async Task<IResult<HostNewRegisterResponse>> RegistrationGenerateNew(string description, Guid requestingUserId, Guid hostOwnerId)
    {
        // Description is the unique identifier for registration since we know nothing about the host being registered yet, ensure only 1
        var matchingDescriptionRequest = await _hostRepository.GetActiveRegistrationsByDescriptionAsync(description);
        if (!matchingDescriptionRequest.Succeeded)
            return await Result<HostNewRegisterResponse>.FailAsync(matchingDescriptionRequest.ErrorMessage);
        if (matchingDescriptionRequest.Result is null || matchingDescriptionRequest.Result.Any())
            return await Result<HostNewRegisterResponse>.FailAsync(ErrorMessageConstants.Hosts.MatchingRegistrationExists);

        var hostCreate = new HostCreate
        {
            OwnerId = hostOwnerId,
            PasswordHash = UrlHelpers.GenerateToken(),
            PasswordSalt = UrlHelpers.GenerateToken(),
            Hostname = "",
            FriendlyName = "",
            Description = description,
            PrivateIp = "",
            PublicIp = "",
            CurrentState = ConnectivityState.UnRegistered,
            Os = OsType.Unknown,
            AllowedPorts = [],
            CreatedBy = requestingUserId,
            CreatedOn = _dateTime.NowDatabaseTime,
            LastModifiedBy = null,
            LastModifiedOn = null,
            IsDeleted = false,
            DeletedOn = null
        };

        var convertedCreate = hostCreate.ToCreateDb();
        
        // Create host to get the GUID to bind the request to for registering this host
        var newHostRequest = await _hostRepository.CreateAsync(convertedCreate);
        if (!newHostRequest.Succeeded)
            return await Result<HostNewRegisterResponse>.FailAsync(newHostRequest.ErrorMessage);

        var registrationKey = UrlHelpers.GenerateToken();

        var newRegistrationRequest = await _hostRepository.CreateRegistrationAsync(new HostRegistrationCreate
        {
            HostId = newHostRequest.Result,
            Description = description,
            Active = true,
            Key = registrationKey,
            ActivationDate = null,
            ActivationPublicIp = "",
            CreatedBy = requestingUserId,
            CreatedOn = _dateTime.NowDatabaseTime,
            LastModifiedBy = null,
            LastModifiedOn = null
        });
        if (!newRegistrationRequest.Succeeded)
            return await Result<HostNewRegisterResponse>.FailAsync(newRegistrationRequest.ErrorMessage);

        // Build the registration URI for the host to register with
        var endpointUri = new Uri(string.Concat(_appConfig.BaseUrl, ApiRouteConstants.GameServer.Host.RegistrationConfirm));
        var registrationUriUser = QueryHelpers.AddQueryString(endpointUri.ToString(), HostConstants.QueryHostId, newHostRequest.Result.ToString());
        var registrationUriFull = QueryHelpers.AddQueryString(registrationUriUser, HostConstants.QueryHostRegisterKey, registrationKey);
        
        var response = new HostNewRegisterResponse
        {
            HostId = newHostRequest.Result,
            Key = registrationKey,
            RegisterUrl = registrationUriFull
        };

        return await Result<HostNewRegisterResponse>.SuccessAsync(response);
    }

    public async Task<IResult<HostRegisterResponse>> RegistrationConfirm(HostRegisterRequest request, string registrationIp)
    {
        var foundRegistrationRequest = await _hostRepository.GetRegistrationByHostIdAndKeyAsync(request.HostId, request.Key);
        if (!foundRegistrationRequest.Succeeded || foundRegistrationRequest.Result is null)
            return await Result<HostRegisterResponse>.FailAsync(ErrorMessageConstants.Hosts.RegisterNotFound);

        var registrationUpdate = foundRegistrationRequest.Result.ToUpdate();
        registrationUpdate.Active = false;
        registrationUpdate.ActivationDate = _dateTime.NowDatabaseTime;
        registrationUpdate.LastModifiedOn = _dateTime.NowDatabaseTime;
        registrationUpdate.LastModifiedBy = _serverState.SystemUserId;
        registrationUpdate.ActivationPublicIp = registrationIp;
        
        await _hostRepository.UpdateRegistrationAsync(registrationUpdate);

        var foundHostRequest = await _hostRepository.GetByIdAsync(request.HostId);
        if (!foundHostRequest.Succeeded || foundHostRequest.Result is null)
            return await Result<HostRegisterResponse>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var hostUpdate = new HostUpdateDb()
        {
            Id = foundHostRequest.Result.Id,
            CurrentState = ConnectivityState.Unknown,
            LastModifiedOn = _dateTime.NowDatabaseTime,
            LastModifiedBy = _serverState.SystemUserId
        };
        var hostUpdateRequest = await _hostRepository.UpdateAsync(hostUpdate);
        if (!hostUpdateRequest.Succeeded)
            return await Result<HostRegisterResponse>.FailAsync(hostUpdateRequest.ErrorMessage);

        return await GetNewHostCredentials(request.HostId, _serverState.SystemUserId);
    }

    private async Task<IResult<HostRegisterResponse>> GetNewHostCredentials(Guid hostId, Guid modifyingUserId)
    {
        var hostToken = UrlHelpers.GenerateToken();
        AccountHelpers.GenerateHashAndSalt(hostToken, HostConstants.HostPepper, out var salt, out var hash);

        var foundHostRequest = await _hostRepository.GetByIdAsync(hostId);
        if (!foundHostRequest.Succeeded || foundHostRequest.Result is null)
            return await Result<HostRegisterResponse>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var hostUpdate = new HostUpdateDb
        {
            Id = foundHostRequest.Result.Id,
            PasswordSalt = salt,
            PasswordHash = hash,
            LastModifiedOn = _dateTime.NowDatabaseTime,
            LastModifiedBy = modifyingUserId
        };
        var hostUpdateRequest = await _hostRepository.UpdateAsync(hostUpdate);
        if (!hostUpdateRequest.Succeeded)
            return await Result<HostRegisterResponse>.FailAsync(hostUpdateRequest.ErrorMessage);

        var response = new HostRegisterResponse
        {
            HostId = hostId,
            HostToken = hostToken
        };

        return await Result<HostRegisterResponse>.SuccessAsync(response);
    }
    
    public async Task<IResult<HostAuthResponse>> GetToken(HostAuthRequest request)
    {
        try
        {
            var foundHost = await GetByIdAsync(request.HostId);
            if (!foundHost.Succeeded)
                return await Result<HostAuthResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);
            var keyIsCorrect = await IsProvidedKeyCorrect(foundHost.Data.Id, request.HostToken);
            if (!keyIsCorrect.Succeeded)
                return await Result<HostAuthResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);
            
            var token = JwtHelpers.GenerateApiJwtEncryptedToken(GetHostClaims(request.HostId), _dateTime, _securityConfig, _appConfig);

            var tokenExpiration = JwtHelpers.GetJwtExpirationTime(token);

            var response = new HostAuthResponse
            {
                Token = token,
                RefreshToken = token,
                RefreshTokenExpiryTime = tokenExpiration
            };
            
            return await Result<HostAuthResponse>.SuccessAsync(response);
        }
        catch (Exception ex)
        {
            return await Result<HostAuthResponse>.FailAsync(ex.Message);
        }
    }

    public async Task<IResult> IsProvidedKeyCorrect(Guid hostId, string key)
    {
        var foundHost = await _hostRepository.GetByIdAsync(hostId);
        if (!foundHost.Succeeded)
            return await Result.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result.FailAsync(foundHost.ErrorMessage);

        var keyIsCorrect = AccountHelpers.IsPasswordCorrect(key, foundHost.Result.PasswordSalt, HostConstants.HostPepper, foundHost.Result.PasswordHash);
        if (!keyIsCorrect)
            return await Result.FailAsync();

        return await Result.SuccessAsync();
    }

    private static IEnumerable<Claim> GetHostClaims(Guid hostId)
    {
        return new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, hostId.ToString()),
            new(ClaimTypes.Email, $"{hostId.ToString()}@host.game.weaver"),
            new(ClaimTypes.Name, $"{hostId.ToString()}@host.game.weaver"),
            new(ApplicationClaimTypes.Permission, PermissionConstants.Hosts.CheckIn),
            new(ApplicationClaimTypes.Permission, PermissionConstants.Hosts.WorkUpdate)
        };
    }

    public async Task<IResult<IEnumerable<HostSlim>>> GetAllAsync()
    {
        var hosts = await _hostRepository.GetAllAsync();
        if (!hosts.Succeeded)
            return await Result<IEnumerable<HostSlim>>.FailAsync(hosts.ErrorMessage);

        return await Result<IEnumerable<HostSlim>>.SuccessAsync(hosts.Result?.ToSlims() ?? new List<HostSlim>());
    }

    public async Task<IResult<IEnumerable<HostSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        var hosts = await _hostRepository.GetAllPaginatedAsync(pageNumber, pageSize);
        if (!hosts.Succeeded)
            return await Result<IEnumerable<HostSlim>>.FailAsync(hosts.ErrorMessage);

        return await Result<IEnumerable<HostSlim>>.SuccessAsync(hosts.Result?.ToSlims() ?? new List<HostSlim>());
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        var hostCount = await _hostRepository.GetCountAsync();
        if (!hostCount.Succeeded)
            return await Result<int>.FailAsync(hostCount.ErrorMessage);

        return await Result<int>.SuccessAsync(hostCount.Result);
    }

    public async Task<IResult<HostSlim>> GetByIdAsync(Guid id)
    {
        var foundHost = await _hostRepository.GetByIdAsync(id);
        if (!foundHost.Succeeded)
            return await Result<HostSlim>.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result<HostSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<HostSlim>.SuccessAsync(foundHost.Result.ToSlim());
    }

    public async Task<IResult<HostSlim>> GetByHostnameAsync(string hostName)
    {
        var foundHost = await _hostRepository.GetByHostnameAsync(hostName);
        if (!foundHost.Succeeded)
            return await Result<HostSlim>.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result<HostSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<HostSlim>.SuccessAsync(foundHost.Result.ToSlim());
    }

    public async Task<IResult<Guid>> CreateAsync(HostCreate createObject)
    {
        var createHostRequest = await _hostRepository.CreateAsync(createObject.ToCreateDb());
        if (!createHostRequest.Succeeded)
            return await Result<Guid>.FailAsync(createHostRequest.ErrorMessage);

        return await Result<Guid>.SuccessAsync(createHostRequest.Result);
    }

    public async Task<IResult> UpdateAsync(HostUpdate updateObject)
    {
        var foundHostRequest = await _hostRepository.GetByIdAsync(updateObject.Id);
        if (!foundHostRequest.Succeeded || foundHostRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        updateObject.LastModifiedOn = _dateTime.NowDatabaseTime;
        
        var updateHostRequest = await _hostRepository.UpdateAsync(updateObject.ToUpdateDb());
        if (!updateHostRequest.Succeeded)
            return await Result.FailAsync(updateHostRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        var foundHostRequest = await _hostRepository.GetByIdAsync(id);
        if (!foundHostRequest.Succeeded || foundHostRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var assignedServers = await _gameServerRepository.GetByHostIdAsync(foundHostRequest.Result.Id);
        if (assignedServers.Succeeded)
        {
            List<string> errorMessages = [ErrorMessageConstants.Hosts.AssignedGameServers];
            errorMessages.AddRange(from server in assignedServers.Result?.ToList() ?? [] select $"Assigned Game Server: [id]{server.Id} [name]{server.ServerName}");
            return await Result.FailAsync(errorMessages);
        }

        var checkinsDeleteRequest = await _hostRepository.DeleteAllCheckInsForHostIdAsync(foundHostRequest.Result.Id);
        if (!checkinsDeleteRequest.Succeeded)
        {
            return await Result.FailAsync(checkinsDeleteRequest.ErrorMessage);
        }

        var deleteHostRequest = await _hostRepository.DeleteAsync(id, modifyingUserId);
        if (!deleteHostRequest.Succeeded)
            return await Result.FailAsync(deleteHostRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<HostSlim>>> SearchAsync(string searchText)
    {
        var foundHosts = await _hostRepository.SearchAsync(searchText);
        if (!foundHosts.Succeeded)
            return await Result<IEnumerable<HostSlim>>.FailAsync(foundHosts.ErrorMessage);

        return await Result<IEnumerable<HostSlim>>.SuccessAsync(foundHosts.Result?.ToSlims() ?? new List<HostSlim>());
    }

    public async Task<IResult<IEnumerable<HostSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var foundHosts = await _hostRepository.SearchPaginatedAsync(searchText, pageNumber, pageSize);
        if (!foundHosts.Succeeded)
            return await Result<IEnumerable<HostSlim>>.FailAsync(foundHosts.ErrorMessage);

        return await Result<IEnumerable<HostSlim>>.SuccessAsync(foundHosts.Result?.ToSlims() ?? new List<HostSlim>());
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrationsAsync()
    {
        var foundRegistrations = await _hostRepository.GetAllRegistrationsAsync();
        if (!foundRegistrations.Succeeded)
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(foundRegistrations.ErrorMessage);

        return await Result<IEnumerable<HostRegistrationFull>>.SuccessAsync(foundRegistrations.Result?.ToFulls() ?? new List<HostRegistrationFull>());
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrationsPaginatedAsync(int pageNumber, int pageSize)
    {
        var foundRegistrations = await _hostRepository.GetAllRegistrationsPaginatedAsync(pageNumber, pageSize);
        if (!foundRegistrations.Succeeded)
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(foundRegistrations.ErrorMessage);

        return await Result<IEnumerable<HostRegistrationFull>>.SuccessAsync(foundRegistrations.Result?.ToFulls() ?? new List<HostRegistrationFull>());
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllActiveRegistrationsAsync()
    {
        var foundRegistrations = await _hostRepository.GetAllActiveRegistrationsAsync();
        if (!foundRegistrations.Succeeded)
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(foundRegistrations.ErrorMessage);

        return await Result<IEnumerable<HostRegistrationFull>>.SuccessAsync(foundRegistrations.Result?.ToFulls() ?? new List<HostRegistrationFull>());
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllInActiveRegistrationsAsync()
    {
        var foundRegistrations = await _hostRepository.GetAllInActiveRegistrationsAsync();
        if (!foundRegistrations.Succeeded)
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(foundRegistrations.ErrorMessage);

        return await Result<IEnumerable<HostRegistrationFull>>.SuccessAsync(foundRegistrations.Result?.ToFulls() ?? new List<HostRegistrationFull>());
    }

    public async Task<IResult<int>> GetRegistrationCountAsync()
    {
        var registrationCount = await _hostRepository.GetRegistrationCountAsync();
        if (!registrationCount.Succeeded)
            return await Result<int>.FailAsync(registrationCount.ErrorMessage);

        return await Result<int>.SuccessAsync(registrationCount.Result);
    }

    public async Task<IResult<HostRegistrationFull>> GetRegistrationByIdAsync(Guid id)
    {
        var foundRegistration = await _hostRepository.GetRegistrationByIdAsync(id);
        if (!foundRegistration.Succeeded)
            return await Result<HostRegistrationFull>.FailAsync(foundRegistration.ErrorMessage);
        if (foundRegistration.Result is null)
            return await Result<HostRegistrationFull>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<HostRegistrationFull>.SuccessAsync(foundRegistration.Result.ToFull());
    }

    public async Task<IResult<HostRegistrationFull>> GetRegistrationByHostIdAsync(Guid hostId)
    {
        var foundRegistration = await _hostRepository.GetRegistrationByHostIdAsync(hostId);
        if (!foundRegistration.Succeeded)
            return await Result<HostRegistrationFull>.FailAsync(foundRegistration.ErrorMessage);
        if (foundRegistration.Result is null)
            return await Result<HostRegistrationFull>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<HostRegistrationFull>.SuccessAsync(foundRegistration.Result.ToFull());
    }

    public async Task<IResult<Guid>> CreateRegistrationAsync(HostRegistrationCreate createObject)
    {
        var createRequest = await _hostRepository.CreateRegistrationAsync(createObject);
        if (!createRequest.Succeeded)
            return await Result<Guid>.FailAsync(createRequest.ErrorMessage);

        return await Result<Guid>.SuccessAsync(createRequest.Result);
    }

    public async Task<IResult> UpdateRegistrationAsync(HostRegistrationUpdate updateObject)
    {
        var foundRegistration = await _hostRepository.GetRegistrationByIdAsync(updateObject.Id);
        if (!foundRegistration.Succeeded || foundRegistration.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        updateObject.LastModifiedOn = _dateTime.NowDatabaseTime;
        var updateRegistrationRequest = await _hostRepository.UpdateRegistrationAsync(updateObject);
        if (!updateRegistrationRequest.Succeeded)
            return await Result.FailAsync(updateRegistrationRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> SearchRegistrationsAsync(string searchText)
    {
        var foundRegistrations = await _hostRepository.SearchRegistrationsAsync(searchText);
        if (!foundRegistrations.Succeeded)
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(foundRegistrations.ErrorMessage);

        return await Result<IEnumerable<HostRegistrationFull>>.SuccessAsync(foundRegistrations.Result?.ToFulls() ?? new List<HostRegistrationFull>());
    }

    public async Task<IResult<IEnumerable<HostRegistrationFull>>> SearchRegistrationsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var foundRegistrations = await _hostRepository.SearchRegistrationsPaginatedAsync(searchText, pageNumber, pageSize);
        if (!foundRegistrations.Succeeded)
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(foundRegistrations.ErrorMessage);

        return await Result<IEnumerable<HostRegistrationFull>>.SuccessAsync(foundRegistrations.Result?.ToFulls() ?? new List<HostRegistrationFull>());
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> GetAllCheckInsAsync()
    {
        var foundCheckins = await _hostRepository.GetAllCheckInsAsync();
        if (!foundCheckins.Succeeded)
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(foundCheckins.ErrorMessage);

        return await Result<IEnumerable<HostCheckInFull>>.SuccessAsync(foundCheckins.Result?.ToFulls() ?? new List<HostCheckInFull>());
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> GetAllCheckInsAfterAsync(DateTime afterDate)
    {
        var foundCheckins = await _hostRepository.GetAllCheckInsAfterAsync(afterDate);
        if (!foundCheckins.Succeeded)
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(foundCheckins.ErrorMessage);

        return await Result<IEnumerable<HostCheckInFull>>.SuccessAsync(foundCheckins.Result?.ToFulls() ?? new List<HostCheckInFull>());
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> GetAllCheckInsPaginatedAsync(int pageNumber, int pageSize)
    {
        var foundCheckins = await _hostRepository.GetAllCheckInsPaginatedAsync(pageNumber, pageSize);
        if (!foundCheckins.Succeeded)
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(foundCheckins.ErrorMessage);

        return await Result<IEnumerable<HostCheckInFull>>.SuccessAsync(foundCheckins.Result?.ToFulls() ?? new List<HostCheckInFull>());
    }

    public async Task<IResult<int>> GetCheckInCountAsync()
    {
        var checkinCount = await _hostRepository.GetCheckInCountAsync();
        if (!checkinCount.Succeeded)
            return await Result<int>.FailAsync(checkinCount.ErrorMessage);

        return await Result<int>.SuccessAsync(checkinCount.Result);
    }

    public async Task<IResult<HostCheckInFull>> GetCheckInByIdAsync(int id)
    {
        var foundCheckin = await _hostRepository.GetCheckInByIdAsync(id);
        if (!foundCheckin.Succeeded)
            return await Result<HostCheckInFull>.FailAsync(foundCheckin.ErrorMessage);
        if (foundCheckin.Result is null)
            return await Result<HostCheckInFull>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<HostCheckInFull>.SuccessAsync(foundCheckin.Result.ToFull());
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> GetChecksInByHostIdAsync(Guid id)
    {
        var foundCheckins = await _hostRepository.GetChecksInByHostIdAsync(id);
        if (!foundCheckins.Succeeded)
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(foundCheckins.ErrorMessage);
        if (foundCheckins.Result is null)
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<HostCheckInFull>>.SuccessAsync(foundCheckins.Result.ToFulls());
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> GetCheckInsLatestByHostIdAsync(Guid id, int count = 10)
    {
        var foundCheckins = await _hostRepository.GetCheckInsLatestByHostIdAsync(id, count);
        if (!foundCheckins.Succeeded)
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(foundCheckins.ErrorMessage);
        if (foundCheckins.Result is null)
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<HostCheckInFull>>.SuccessAsync(foundCheckins.Result.ToFulls());
    }

    public async Task<IResult> CreateCheckInAsync(HostCheckInCreate createObject)
    {
        if (!float.IsNormal(createObject.CpuUsage) || !float.IsNormal(createObject.RamUsage) || !float.IsNormal(createObject.Uptime))
            return await Result.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);
        
        var createRequest = await _hostRepository.CreateCheckInAsync(createObject);
        if (!createRequest.Succeeded)
            return await Result.FailAsync(createRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<int>> DeleteAllCheckInsForHostIdAsync(Guid id)
    {
        var deleteRequest = await _hostRepository.DeleteAllCheckInsForHostIdAsync(id);
        if (!deleteRequest.Succeeded)
            return await Result<int>.FailAsync(deleteRequest.ErrorMessage);

        return await Result<int>.SuccessAsync(deleteRequest.Result);
    }

    public async Task<IResult<int>> DeleteAllOldCheckInsAsync(CleanupTimeframe olderThan)
    {
        var deleteRequest = await _hostRepository.DeleteAllOldCheckInsAsync(olderThan);
        if (!deleteRequest.Succeeded)
            return await Result<int>.FailAsync(deleteRequest.ErrorMessage);

        return await Result<int>.SuccessAsync(deleteRequest.Result);
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> SearchCheckInsAsync(string searchText)
    {
        var foundCheckIns = await _hostRepository.SearchCheckInsAsync(searchText);
        if (!foundCheckIns.Succeeded)
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(foundCheckIns.ErrorMessage);

        return await Result<IEnumerable<HostCheckInFull>>.SuccessAsync(foundCheckIns.Result?.ToFulls() ?? new List<HostCheckInFull>());
    }

    public async Task<IResult<IEnumerable<HostCheckInFull>>> SearchCheckInsPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var foundCheckIns = await _hostRepository.SearchCheckInsPaginatedAsync(searchText, pageNumber, pageSize);
        if (!foundCheckIns.Succeeded)
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(foundCheckIns.ErrorMessage);

        return await Result<IEnumerable<HostCheckInFull>>.SuccessAsync(foundCheckIns.Result?.ToFulls() ?? new List<HostCheckInFull>());
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetAllWeaverWorkAsync()
    {
        var hosts = await _hostRepository.GetAllWeaverWorkAsync();
        if (!hosts.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(hosts.ErrorMessage);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(hosts.Result?.ToSlims() ?? new List<WeaverWorkSlim>());
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetAllWeaverWorkPaginatedAsync(int pageNumber, int pageSize)
    {
        var hosts = await _hostRepository.GetAllWeaverWorkPaginatedAsync(pageNumber, pageSize);
        if (!hosts.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(hosts.ErrorMessage);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(hosts.Result?.ToSlims() ?? new List<WeaverWorkSlim>());
    }

    public async Task<IResult<int>> GetWeaverWorkCountAsync()
    {
        var hostCount = await _hostRepository.GetWeaverWorkCountAsync();
        if (!hostCount.Succeeded)
            return await Result<int>.FailAsync(hostCount.ErrorMessage);

        return await Result<int>.SuccessAsync(hostCount.Result);
    }

    public async Task<IResult<WeaverWorkSlim>> GetWeaverWorkByIdAsync(int id)
    {
        var foundHost = await _hostRepository.GetWeaverWorkByIdAsync(id);
        if (!foundHost.Succeeded)
            return await Result<WeaverWorkSlim>.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result<WeaverWorkSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<WeaverWorkSlim>.SuccessAsync(foundHost.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetWeaverWorkByHostIdAsync(Guid id)
    {
        var foundHost = await _hostRepository.GetWeaverWorkByHostIdAsync(id);
        if (!foundHost.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(foundHost.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetWeaverWaitingWorkByHostIdAsync(Guid id)
    {
        var foundHost = await _hostRepository.GetWeaverWaitingWorkByHostIdAsync(id);
        if (!foundHost.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(foundHost.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetWeaverAllWaitingWorkByHostIdAsync(Guid id)
    {
        var foundHost = await _hostRepository.GetWeaverAllWaitingWorkByHostIdAsync(id);
        if (!foundHost.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(foundHost.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetWeaverWorkByTargetTypeAsync(WeaverWorkTarget target)
    {
        var foundHost = await _hostRepository.GetWeaverWorkByTargetTypeAsync(target);
        if (!foundHost.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(foundHost.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetWeaverWorkByStatusAsync(WeaverWorkState status)
    {
        var foundHost = await _hostRepository.GetWeaverWorkByStatusAsync(status);
        if (!foundHost.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(foundHost.Result.ToSlims());
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetWeaverWorkCreatedWithinAsync(DateTime from, DateTime until)
    {
        var foundHost = await _hostRepository.GetWeaverWorkCreatedWithinAsync(from, until);
        if (!foundHost.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(foundHost.ErrorMessage);
        if (foundHost.Result is null)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(foundHost.Result.ToSlims());
    }

    public async Task<IResult<int>> CreateWeaverWorkAsync(WeaverWorkCreate createObject)
    {
        var createHostRequest = await _hostRepository.CreateWeaverWorkAsync(createObject);
        if (!createHostRequest.Succeeded)
            return await Result<int>.FailAsync(createHostRequest.ErrorMessage);

        return await Result<int>.SuccessAsync(createHostRequest.Result);
    }

    public async Task<IResult> UpdateWeaverWorkAsync(WeaverWorkUpdate updateObject)
    {
        var foundHostRequest = await _hostRepository.GetWeaverWorkByIdAsync(updateObject.Id);
        var hostId = foundHostRequest.Result?.HostId ?? Guid.Empty;

        // Some updates like GameServerStateUpdate or HostDetail can come in without being bound to specific work
        if (hostId == Guid.Empty && updateObject.TargetType is not WeaverWorkTarget.GameServerStateUpdate and not WeaverWorkTarget.HostDetail)
        {
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);
        }

        updateObject.HostId = hostId;
        updateObject.LastModifiedOn = _dateTime.NowDatabaseTime;

        switch (updateObject.TargetType)
        {
            case WeaverWorkTarget.GameServerStateUpdate:
                var serverStateResult = await HandleUpdateGameServerState(updateObject);
                if (!serverStateResult.Succeeded)
                    _logger.Error("Game server state update failed for [{WorkId}]{WorkType}: {Error}", updateObject.Id, updateObject.TargetType, serverStateResult.Messages);
                updateObject.WorkData = null;  // Empty WorkData so we don't overwrite the actual command
                break;
            case WeaverWorkTarget.StatusUpdate:
                _logger.Debug("Weaver work status update received: [{WorkId}]{WorkStatus}", updateObject.Id, updateObject.Status);
                break;
            case WeaverWorkTarget.HostDetail:
                var hostDetailResult = await HandleHostDetail(updateObject);
                if (!hostDetailResult.Succeeded)
                    _logger.Error("Host detail update failed for [{WorkId}]{WorkType}: {Error}", updateObject.Id, updateObject.TargetType, hostDetailResult.Messages);
                updateObject.WorkData = null;  // Empty WorkData so we don't overwrite the actual command
                break;
            case WeaverWorkTarget.Host:
            case WeaverWorkTarget.HostStatusUpdate:
            case WeaverWorkTarget.GameServer:
            case WeaverWorkTarget.GameServerInstall:
            case WeaverWorkTarget.GameServerUpdate:
            case WeaverWorkTarget.GameServerUninstall:
            case WeaverWorkTarget.GameServerStart:
            case WeaverWorkTarget.GameServerStop:
            case WeaverWorkTarget.GameServerRestart:
            case WeaverWorkTarget.GameServerConfigNew:
            case WeaverWorkTarget.GameServerConfigUpdate:
            case WeaverWorkTarget.GameServerConfigDelete:
            case WeaverWorkTarget.GameServerConfigUpdateFull:
            case WeaverWorkTarget.CurrentEnd:
            case null:
                _logger.Warning("Weaver work type received doesn't have a handler yet so nothing is being done with it: [{WorkId}]{WorkType}",
                    updateObject.Id, updateObject.TargetType);
                break;
            default:
                _logger.Warning("Weaver work type received doesn't have a handler yet so nothing is being done with it: [{WorkId}]{WorkType}",
                    updateObject.Id, updateObject.TargetType);
                break;
        }

        // Some updates like GameServerStateUpdate or HostDetail may not be tied to Weaver Work, so we'll skip attempting to update non-existent work
        if (hostId == Guid.Empty)
        {
            return await Result.SuccessAsync();
        }
        
        var updateWorkRequest = await _hostRepository.UpdateWeaverWorkAsync(updateObject);
        if (!updateWorkRequest.Succeeded)
            return await Result.FailAsync(updateWorkRequest.ErrorMessage);

        if (updateObject.Status is not null)
        {
            _eventService.TriggerWeaverWorkStatus("HostServiceWorkUpdate", updateObject.ToEvent());
        }

        return await Result.SuccessAsync();
    }

    private async Task<IResult> HandleHostDetail(WeaverWorkUpdate updateObject)
    {
        try
        {
            if (updateObject.WorkData is null)
            {
                return await Result.FailAsync("Host detail work data is null, unable to update detail");
            }
        
            var deserializedData = _serializerService.DeserializeMemory<HostDetailUpdate>(updateObject.WorkData);
            if (deserializedData is null)
            {
                return await Result.FailAsync("Deserialized host detail work data is null, unable to update detail");
            }

            var foundHost = await _hostRepository.GetByIdAsync(deserializedData.HostId);
            if (!foundHost.Succeeded || foundHost.Result is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);
            }

            updateObject.HostId = foundHost.Result.Id;

            var hostUpdate = deserializedData.ToUpdate();
            // TODO: Pull public IP from the API request to inject
            // hostUpdate.PublicIp = updateObject.PublicIp
            hostUpdate.PrivateIp = hostUpdate.NetworkInterfaces?.FirstOrDefault()?.IpAddresses.FirstOrDefault();
            hostUpdate.FriendlyName = string.IsNullOrWhiteSpace(foundHost.Result.FriendlyName) ? hostUpdate.Hostname : foundHost.Result.FriendlyName;

            var updateRequest = await _hostRepository.UpdateAsync(hostUpdate.ToUpdateDb());
            if (!updateRequest.Succeeded)
            {
                return await Result.FailAsync(updateRequest.ErrorMessage);
            }

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync($"Failure occurred working with deserialized game server state update: {ex.Message}");
        }
    }
    
    private async Task<IResult> HandleUpdateGameServerState(WeaverWorkUpdate updateObject)
    {
        try
        {
            if (updateObject.WorkData is null)
                return await Result.FailAsync("Game server state update work data is null, unable to update state");
        
            var deserializedData = _serializerService.DeserializeMemory<GameServerStateUpdate>(updateObject.WorkData);
            if (deserializedData is null)
                return await Result.FailAsync("Deserialized game server state update work data is null, unable to update state");

            var gameServer = await _gameServerRepository.GetByIdAsync(deserializedData.Id);
            if (!gameServer.Succeeded || gameServer.Result is null)
                return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

            updateObject.HostId = gameServer.Result.HostId;

            var gameServerUpdate = new GameServerUpdate {Id = gameServer.Result.Id, ServerState = deserializedData.ServerState};
            
            // Update the game server version to the latest if we just did a server update and it was successful
            if (deserializedData.BuildVersionUpdated)
            {
                var gameServerGame = await _gameRepository.GetByIdAsync(gameServer.Result.GameId);
                if (gameServerGame.Result is not null)
                {
                    gameServerUpdate.ServerBuildVersion = gameServerGame.Result.LatestBuildVersion;
                }
            }
            
            var result = await _gameServerRepository.UpdateAsync(gameServerUpdate);
            if (!result.Succeeded)
                return await Result.FailAsync(result.ErrorMessage);

            var serverStatusEvent = deserializedData.ToStatusEvent();
            serverStatusEvent.ServerName = gameServer.Result.ServerName;
            
            _eventService.TriggerGameServerStatus("HostServiceGameServerStateUpdate", serverStatusEvent);

            if (deserializedData.ServerState != ConnectivityState.Uninstalled) return await Result.SuccessAsync();
            
            // State update is uninstalled, so we'll wrap up by deleting the game server
            var deleteServerRequest = await _gameServerRepository.DeleteAsync(gameServer.Result.Id, updateObject.LastModifiedBy ?? Guid.Empty);
            if (!deleteServerRequest.Succeeded)
                return await Result.FailAsync(deleteServerRequest.ErrorMessage);

            var serverGameProfile = await _gameServerRepository.GetGameProfileByIdAsync(gameServer.Result.GameProfileId);
            if (!serverGameProfile.Succeeded || serverGameProfile.Result is null)
                return await Result.FailAsync(serverGameProfile.ErrorMessage);

            var deleteProfileRequest =
                await _gameServerRepository.DeleteGameProfileAsync(serverGameProfile.Result.Id, updateObject.LastModifiedBy ?? Guid.Empty);
            if (!deleteProfileRequest.Succeeded)
                return await Result.FailAsync(deleteProfileRequest.ErrorMessage);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync($"Failure occurred working with deserialized game server state update: {ex.Message}");
        }
    }

    public async Task<IResult> DeleteWeaverWorkAsync(int id)
    {
        var foundHostRequest = await _hostRepository.GetWeaverWorkByIdAsync(id);
        if (!foundHostRequest.Succeeded || foundHostRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var deleteHostRequest = await _hostRepository.DeleteWeaverWorkAsync(id);
        if (!deleteHostRequest.Succeeded)
            return await Result.FailAsync(deleteHostRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteWeaverWorkForHostAsync(Guid hostId)
    {
        var deleteHostRequest = await _hostRepository.DeleteWeaverWorkForHostAsync(hostId);
        if (!deleteHostRequest.Succeeded)
            return await Result.FailAsync(deleteHostRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteWeaverWorkOlderThanAsync(DateTime olderThan)
    {
        var deleteHostRequest = await _hostRepository.DeleteWeaverWorkOlderThanAsync(olderThan);
        if (!deleteHostRequest.Succeeded)
            return await Result.FailAsync(deleteHostRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> SearchWeaverWorkAsync(string searchText)
    {
        var foundHosts = await _hostRepository.SearchWeaverWorkAsync(searchText);
        if (!foundHosts.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(foundHosts.ErrorMessage);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(foundHosts.Result?.ToSlims() ?? new List<WeaverWorkSlim>());
    }

    public async Task<IResult<IEnumerable<WeaverWorkSlim>>> SearchWeaverWorkPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var foundHosts = await _hostRepository.SearchWeaverWorkPaginatedAsync(searchText, pageNumber, pageSize);
        if (!foundHosts.Succeeded)
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(foundHosts.ErrorMessage);

        return await Result<IEnumerable<WeaverWorkSlim>>.SuccessAsync(foundHosts.Result?.ToSlims() ?? new List<WeaverWorkSlim>());
    }
}