using System.Security.Claims;
using Application.Constants.Communication;
using Application.Constants.GameServer;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Auth;
using Application.Helpers.GameServer;
using Application.Helpers.Identity;
using Application.Helpers.Lifecycle;
using Application.Helpers.Web;
using Application.Mappers.GameServer;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.HostRegistration;
using Application.Models.GameServer.WeaverWork;
using Application.Models.Identity.Permission;
using Application.Repositories.GameServer;
using Application.Repositories.Lifecycle;
using Application.Requests.GameServer.Host;
using Application.Requests.GameServer.WeaverWork;
using Application.Responses.v1.GameServer;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Domain.Enums.Database;
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
    private readonly IAuditTrailsRepository _auditRepository;

    public HostService(IHostRepository hostRepository, IDateTimeService dateTime, IRunningServerState serverState, IOptions<AppConfiguration> appConfig,
        IOptions<SecurityConfiguration> securityConfig, ILogger logger, IGameServerRepository gameServerRepository, ISerializerService serializerService,
        IEventService eventService, IGameRepository gameRepository, IAuditTrailsRepository auditRepository)
    {
        _hostRepository = hostRepository;
        _dateTime = dateTime;
        _serverState = serverState;
        _logger = logger;
        _gameServerRepository = gameServerRepository;
        _serializerService = serializerService;
        _eventService = eventService;
        _gameRepository = gameRepository;
        _auditRepository = auditRepository;
        _securityConfig = securityConfig.Value;
        _appConfig = appConfig.Value;
    }

    public async Task<IResult<HostNewRegisterResponse>> RegistrationGenerateNew(HostRegistrationCreateRequest request, Guid requestUserId)
    {
        // Description is the unique identifier for registration since we know nothing about the host being registered yet, ensure only 1
        var matchingRegistration = await _hostRepository.GetActiveRegistrationsByDescriptionAsync(request.Description);
        if (matchingRegistration.Result is not null & matchingRegistration.Result!.Any())
        {
            return await Result<HostNewRegisterResponse>.FailAsync(ErrorMessageConstants.Hosts.MatchingRegistrationExists);
        }

        if (request.OwnerId == Guid.Empty)
        {
            request.OwnerId = requestUserId;
        }

        var createRequest = new HostCreate
        {
            OwnerId = request.OwnerId,
            PasswordHash = UrlHelpers.GenerateToken(),
            PasswordSalt = UrlHelpers.GenerateToken(),
            Hostname = "",
            FriendlyName = request.Name,
            Description = request.Description,
            PrivateIp = "",
            PublicIp = "",
            CurrentState = ConnectivityState.UnRegistered,
            Os = OsType.Unknown,
            AllowedPorts = request.AllowedPorts,
            CreatedBy = requestUserId,
            CreatedOn = _dateTime.NowDatabaseTime,
            LastModifiedBy = null,
            LastModifiedOn = null,
            IsDeleted = false,
            DeletedOn = null
        };

        var convertedCreate = createRequest.ToCreateDb();
        
        // Create host to get the GUID to bind the request to for registering this host
        var hostCreate = await _hostRepository.CreateAsync(convertedCreate);
        if (!hostCreate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHostRegistrations, Guid.Empty, requestUserId,
                new Dictionary<string, string>
                {
                    {"HostName", request.Name},
                    {"HostDescription", request.Description},
                    {"HostOwnerId", request.OwnerId.ToString()},
                    {"HostAllowedPorts", request.AllowedPorts.ToString() ?? "[]"},
                    {"Detail", "Failed to create host for registration"},
                    {"Error", hostCreate.ErrorMessage}
                });
            return await Result<HostNewRegisterResponse>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdHost = await _hostRepository.GetByIdAsync(hostCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Hosts, hostCreate.Result, requestUserId, DatabaseActionType.Create,
            null, createdHost.Result);

        var registrationKey = UrlHelpers.GenerateToken();

        var registrationCreate = await _hostRepository.CreateRegistrationAsync(new HostRegistrationCreate
        {
            HostId = hostCreate.Result,
            Description = request.Description,
            Active = true,
            Key = registrationKey,
            ActivationDate = null,
            ActivationPublicIp = "",
            CreatedBy = requestUserId,
            CreatedOn = _dateTime.NowDatabaseTime,
            LastModifiedBy = null,
            LastModifiedOn = null
        });
        if (!registrationCreate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHostRegistrations, Guid.Empty, requestUserId,
                new Dictionary<string, string>
                {
                    {"HostName", request.Name},
                    {"HostDescription", request.Description},
                    {"HostOwnerId", request.OwnerId.ToString()},
                    {"HostAllowedPorts", request.AllowedPorts.ToString() ?? ""},
                    {"CreatedHostId", hostCreate.Result.ToString()},
                    {"Detail", "Failed to generate new registration for host registration but did create host"},
                    {"Error", registrationCreate.ErrorMessage}
                });
            return await Result<HostNewRegisterResponse>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdRegistration = await _hostRepository.GetRegistrationByIdAsync(registrationCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.HostRegistrations, registrationCreate.Result, requestUserId, DatabaseActionType.Create,
            null, createdRegistration.Result);

        // Build the registration URI for the host to register with
        var endpointUri = new Uri(string.Concat(_appConfig.BaseUrl, ApiRouteConstants.GameServer.HostRegistration.Confirm));
        var registrationUriUser = QueryHelpers.AddQueryString(endpointUri.ToString(), HostConstants.QueryHostId, hostCreate.Result.ToString());
        var registrationUriFull = QueryHelpers.AddQueryString(registrationUriUser, HostConstants.QueryHostRegisterKey, registrationKey);
        
        var response = new HostNewRegisterResponse
        {
            HostId = hostCreate.Result,
            Key = registrationKey,
            RegisterUrl = registrationUriFull
        };

        return await Result<HostNewRegisterResponse>.SuccessAsync(response);
    }

    public async Task<IResult<HostRegisterResponse>> RegistrationConfirm(HostRegistrationConfirmRequest request, string registrationIp)
    {
        var foundRegistration = await _hostRepository.GetRegistrationByHostIdAndKeyAsync(request.HostId, request.Key);
        if (foundRegistration.Result is null)
        {
            await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHostRegistrations, Guid.Empty, Guid.Empty, 
                new Dictionary<string, string>
                {
                    {"SenderIp", registrationIp},
                    {"ProvidedHostId", request.HostId.ToString()},
                    {"ProvidedKey", request.Key},
                    {"Detail", "Invalid host registration was provided in an attempt to be confirmed"},
                    {"Error", foundRegistration.ErrorMessage}
                });
            return await Result<HostRegisterResponse>.FailAsync(ErrorMessageConstants.Hosts.RegistrationNotFound);
        }

        var foundHost = await _hostRepository.GetByIdAsync(request.HostId);
        if (foundHost.Result is null)
        {
            return await Result<HostRegisterResponse>.FailAsync(ErrorMessageConstants.Hosts.NotFound);
        }

        var convertedUpdate = foundRegistration.Result.ToUpdate();
        convertedUpdate.Active = false;
        convertedUpdate.ActivationDate = _dateTime.NowDatabaseTime;
        convertedUpdate.LastModifiedOn = _dateTime.NowDatabaseTime;
        convertedUpdate.LastModifiedBy = _serverState.SystemUserId;
        convertedUpdate.ActivationPublicIp = registrationIp;
        
        var registrationConfirm = await _hostRepository.UpdateRegistrationAsync(convertedUpdate);
        if (!registrationConfirm.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootHostRegistrations, foundRegistration.Result.Id,
                new Dictionary<string, string>
                {
                    {"SenderIp", registrationIp},
                    {"ProvidedHostId", request.HostId.ToString()},
                    {"ProvidedKey", request.Key},
                    {"Detail", "Failed to confirm valid host registration via registration update"},
                    {"Error", registrationConfirm.ErrorMessage}
                });
            return await Result<HostRegisterResponse>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var confirmedRegistration = await _hostRepository.GetRegistrationByIdAsync(foundRegistration.Result.Id);
        await _auditRepository.CreateAuditTrail(_serverState, _dateTime, AuditTableName.HostRegistrations, foundRegistration.Result.Id, DatabaseActionType.Update,
            foundRegistration.Result, confirmedRegistration.Result);

        var hostUpdate = new HostUpdateDb()
        {
            Id = foundHost.Result.Id,
            CurrentState = ConnectivityState.Unknown,
            PublicIp = registrationIp,
            LastModifiedOn = _dateTime.NowDatabaseTime,
            LastModifiedBy = _serverState.SystemUserId
        };
        var updateHost = await _hostRepository.UpdateAsync(hostUpdate);
        if (!updateHost.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootHosts, foundHost.Result.Id,
                new Dictionary<string, string>
                {
                    {"SenderIp", registrationIp},
                    {"RegistrationId", foundRegistration.Result.Id.ToString()},
                    {"Detail", "Failed to update host for checkin confirmation"},
                    {"Error", updateHost.ErrorMessage}
                });
            return await Result<HostRegisterResponse>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var updatedHost = await _hostRepository.GetByIdAsync(foundHost.Result.Id);
        await _auditRepository.CreateAuditTrail(_serverState, _dateTime, AuditTableName.Hosts, foundHost.Result.Id, DatabaseActionType.Update,
            foundHost.Result, updatedHost.Result);

        var workHostDetail = await _hostRepository.SendWeaverWork(WeaverWorkTarget.HostDetail, foundHost.Result.Id, foundHost.Result.Id,
            _serverState.SystemUserId, _dateTime.NowDatabaseTime);
        if (!workHostDetail.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHostRegistrations, foundRegistration.Result.Id,
                _serverState.SystemUserId, new Dictionary<string, string>
                {
                    {"Detail", "Failed to send host detail start work post host registration confirmation"},
                    {"Error", workHostDetail.ErrorMessage}
                });
            return await Result<HostRegisterResponse>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.WeaverWorks, foundRegistration.Result.Id, _serverState.SystemUserId,
            DatabaseActionType.GameServerAction, null, new Dictionary<string, string>
            {
                {"WorkId", workHostDetail.Result.ToString()},
                {"Detail", "Host detail gather work request sent"}
            });

        return await GetNewHostCredentials(request.HostId, _serverState.SystemUserId);
    }

    private async Task<IResult<HostRegisterResponse>> GetNewHostCredentials(Guid hostId, Guid requestUserId)
    {
        var hostToken = UrlHelpers.GenerateToken();
        AccountHelpers.GenerateHashAndSalt(hostToken, HostConstants.HostPepper, out var salt, out var hash);

        var foundHost = await _hostRepository.GetByIdAsync(hostId);
        if (foundHost.Result is null)
        {
            return await Result<HostRegisterResponse>.FailAsync(ErrorMessageConstants.Hosts.NotFound);
        }

        var hostUpdate = new HostUpdateDb
        {
            Id = foundHost.Result.Id,
            PasswordSalt = salt,
            PasswordHash = hash,
            LastModifiedOn = _dateTime.NowDatabaseTime,
            LastModifiedBy = requestUserId
        };
        var updateHost = await _hostRepository.UpdateAsync(hostUpdate);
        if (!updateHost.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHosts, foundHost.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to update host for new host credential generation"},
                    {"Error", updateHost.ErrorMessage}
                });
            return await Result<HostRegisterResponse>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

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
            var foundHost = await _hostRepository.GetByIdAsync(request.HostId);
            if (foundHost.Result is null)
            {
                return await Result<HostAuthResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);
            }
            
            var keyIsCorrect = await IsProvidedKeyCorrect(foundHost.Result.Id, request.HostToken);
            if (!keyIsCorrect.Succeeded)
            {
                return await Result<HostAuthResponse>.FailAsync(ErrorMessageConstants.Authentication.CredentialsInvalidError);
            }
            
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
            var tshootId = await _auditRepository.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootHostAuth, Guid.Empty,
                new Dictionary<string, string>
                {
                    {"ProvidedHostId", request.HostId.ToString()},
                    {"ProvidedHostKey", request.HostToken},
                    {"Detail", "Failure occurred handling host auth request"},
                    {"Error", ex.Message}
                });
            return await Result<HostAuthResponse>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
    }

    public async Task<IResult> IsProvidedKeyCorrect(Guid hostId, string key)
    {
        var foundHost = await _hostRepository.GetByIdAsync(hostId);
        if (foundHost.Result is null)
        {
            return await Result.FailAsync();
        }

        var keyIsCorrect = AccountHelpers.IsPasswordCorrect(key, foundHost.Result.PasswordSalt, HostConstants.HostPepper, foundHost.Result.PasswordHash);
        if (!keyIsCorrect)
        {
            return await Result.FailAsync();
        }

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

    public async Task<IResult<Guid>> CreateAsync(HostCreateRequest request, Guid requestUserId)
    {
        var convertedRequest = request.ToCreate();
        convertedRequest.CreatedBy = requestUserId;
        convertedRequest.CreatedOn = _dateTime.NowDatabaseTime;
        
        var hostCreate = await _hostRepository.CreateAsync(convertedRequest.ToCreateDb());
        if (!hostCreate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHosts, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to create host"},
                {"Error", hostCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdHost = await _hostRepository.GetByIdAsync(hostCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Hosts, hostCreate.Result, requestUserId, DatabaseActionType.Create,
            null, createdHost.Result);

        return await Result<Guid>.SuccessAsync(hostCreate.Result);
    }

    public async Task<IResult> UpdateAsync(HostUpdateRequest request, Guid requestUserId)
    {
        var foundHost = await _hostRepository.GetByIdAsync(request.Id);
        if (foundHost.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.Hosts.NotFound);
        }

        var convertedRequest = request.ToUpdate();
        convertedRequest.LastModifiedOn = _dateTime.NowDatabaseTime;
        convertedRequest.LastModifiedBy = requestUserId;
        
        var hostUpdate = await _hostRepository.UpdateAsync(convertedRequest.ToUpdateDb());
        if (!hostUpdate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHosts, foundHost.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to update host"},
                {"Error", hostUpdate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var updatedHost = await _hostRepository.GetByIdAsync(foundHost.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Hosts, foundHost.Result.Id, requestUserId, DatabaseActionType.Update,
            foundHost.Result, updatedHost.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid requestUserId)
    {
        var foundHost = await _hostRepository.GetByIdAsync(id);
        if (foundHost.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.Hosts.NotFound);
        }

        var assignedServers = await _gameServerRepository.GetByHostIdAsync(foundHost.Result.Id);
        if (assignedServers.Succeeded)
        {
            List<string> errorMessages = [ErrorMessageConstants.Hosts.AssignedGameServers];
            errorMessages.AddRange(from server in assignedServers.Result?.ToList() ?? [] select $"Assigned Game Server: [id]{server.Id} [name]{server.ServerName}");
            return await Result.FailAsync(errorMessages);
        }

        var deleteCheckins = await _hostRepository.DeleteAllCheckInsForHostIdAsync(foundHost.Result.Id);
        if (!deleteCheckins.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHosts, foundHost.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to delete checkins for host deletion"},
                    {"Error", deleteCheckins.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var deleteHost = await _hostRepository.DeleteAsync(id, requestUserId);
        if (!deleteHost.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHosts, foundHost.Result.Id, requestUserId,
                new Dictionary<string, string>
                {
                    {"Detail", "Failed to delete host but successfully removed host checkins"},
                    {"Error", deleteHost.ErrorMessage}
                });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Hosts, foundHost.Result.Id, requestUserId, DatabaseActionType.Delete, foundHost.Result);
        
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

    public async Task<IResult> UpdateRegistrationAsync(HostRegistrationUpdateRequest request, Guid requestUserId)
    {
        var foundRegistration = await _hostRepository.GetRegistrationByIdAsync(request.Id);
        if (foundRegistration.Result is null)
        {
            await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHostRegistrations, Guid.Empty, requestUserId, 
                new Dictionary<string, string>
                {
                    {"ProvidedRegistrationId", request.Id.ToString()},
                    {"Detail", "Invalid host registration was provided in an attempt to be updated"},
                    {"Error", foundRegistration.ErrorMessage}
                });
            return await Result<HostRegisterResponse>.FailAsync(ErrorMessageConstants.Hosts.RegistrationNotFound);
        }

        var convertedRequest = request.ToUpdate();
        convertedRequest.LastModifiedOn = _dateTime.NowDatabaseTime;
        convertedRequest.LastModifiedBy = requestUserId;
        
        var updateRegistrationRequest = await _hostRepository.UpdateRegistrationAsync(convertedRequest);
        if (!updateRegistrationRequest.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHostRegistrations, foundRegistration.Result.Id,
                requestUserId, new Dictionary<string, string>
                {
                    {"ProvidedRegistrationId", request.Id.ToString()},
                    {"Detail", "Failed to update host registration"},
                    {"Error", updateRegistrationRequest.ErrorMessage}
                });
            return await Result.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var updatedRegistration = await _hostRepository.GetRegistrationByIdAsync(foundRegistration.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.HostRegistrations, foundRegistration.Result.Id, requestUserId, DatabaseActionType.Update,
            foundRegistration.Result, updatedRegistration.Result);

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

    public async Task<IResult> CreateCheckInAsync(HostCheckInCreate request)
    {
        if (!float.IsNormal(request.CpuUsage) || !float.IsNormal(request.RamUsage) || !float.IsNormal(request.Uptime))
        {
            return await Result.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);
        }
        
        var checkInCreate = await _hostRepository.CreateCheckInAsync(request);
        if (checkInCreate.Succeeded) return await Result.SuccessAsync();
        
        var tshootId = await _auditRepository.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootHostCheckins, Guid.Empty,
            new Dictionary<string, string>
            {
                {"ProvidedHostId", request.HostId.ToString()},
                {"Detail", "Failed to create host checkin"},
                {"Error", checkInCreate.ErrorMessage}
            });
        return await Result.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
    }

    public async Task<IResult<int>> DeleteAllCheckInsForHostIdAsync(Guid id, Guid requestUserId)
    {
        var checkInsDelete = await _hostRepository.DeleteAllCheckInsForHostIdAsync(id);
        if (checkInsDelete.Succeeded) return await Result<int>.SuccessAsync(checkInsDelete.Result);
        
        var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHostCheckins, Guid.Empty, requestUserId,
            new Dictionary<string, string>
            {
                {"ProvidedHostId", id.ToString()},
                {"Detail", "Failed to delete all checkins for host"},
                {"Error", checkInsDelete.ErrorMessage}
            });
        return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
    }

    public async Task<IResult<int>> DeleteAllOldCheckInsAsync(CleanupTimeframe olderThan, Guid requestUserId)
    {
        var checkInDelete = await _hostRepository.DeleteAllOldCheckInsAsync(olderThan);
        if (checkInDelete.Succeeded) return await Result<int>.SuccessAsync(checkInDelete.Result);
        
        var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootHostCheckins, Guid.Empty, requestUserId,
            new Dictionary<string, string>
            {
                {"ProvidedTimeframe", olderThan.ToString()},
                {"Detail", "Failed to delete old checkins for a given timeframe"},
                {"Error", checkInDelete.ErrorMessage}
            });
        return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
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

    public async Task<IResult<int>> CreateWeaverWorkAsync(WeaverWorkCreateRequest request, Guid requestUserId)
    {
        var convertedRequest = request.ToCreate();
        convertedRequest.CreatedBy = requestUserId;
        convertedRequest.CreatedOn = _dateTime.NowDatabaseTime;

        var workCreate = await _hostRepository.CreateWeaverWorkAsync(convertedRequest);
        if (workCreate.Succeeded) return await Result<int>.SuccessAsync(workCreate.Result);
        
        var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootWeaverWork, Guid.Empty, requestUserId,
            new Dictionary<string, string>
            {
                {"ProvidedTarget", request.TargetType.ToString()},
                {"ProvidedStatus", request.Status.ToString()},
                {"ProvidedWorkData", request.WorkData?.ToString() ?? ""},
                {"Detail", "Failed to create weaver work"},
                {"Error", workCreate.ErrorMessage}
            });
        return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
    }

    public async Task<IResult> UpdateWeaverWorkAsync(WeaverWorkUpdate request, string sourceIp = "")
    {
        var foundHost = await _hostRepository.GetWeaverWorkByIdAsync(request.Id);
        var hostId = foundHost.Result?.HostId ?? Guid.Empty;

        // Some updates like GameServerStateUpdate or HostDetail can come in without being bound to specific work
        if (hostId == Guid.Empty && request.TargetType is not WeaverWorkTarget.GameServerStateUpdate and not WeaverWorkTarget.HostDetail)
        {
            return await Result.FailAsync(ErrorMessageConstants.Hosts.NotFound);
        }

        request.HostId = hostId;
        request.LastModifiedOn = _dateTime.NowDatabaseTime;

        switch (request.TargetType)
        {
            case WeaverWorkTarget.GameServerStateUpdate:
                var serverStateResult = await HandleUpdateGameServerState(request);
                if (!serverStateResult.Succeeded)
                    _logger.Error("Game server state update failed for [{WorkId}]{WorkType}: {Error}", request.Id, request.TargetType, serverStateResult.Messages);
                request.WorkData = null;  // Empty WorkData so we don't overwrite the actual command
                break;
            case WeaverWorkTarget.StatusUpdate:
                _logger.Debug("Weaver work status update received: [{WorkId}]{WorkStatus}", request.Id, request.Status);
                break;
            case WeaverWorkTarget.HostDetail:
                var hostDetailResult = await HandleHostDetail(request, sourceIp);
                if (!hostDetailResult.Succeeded)
                    _logger.Error("Host detail update failed for [{WorkId}]{WorkType}: {Error}", request.Id, request.TargetType, hostDetailResult.Messages);
                request.WorkData = null;  // Empty WorkData so we don't overwrite the actual command
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
                    request.Id, request.TargetType);
                break;
            default:
                _logger.Warning("Weaver work type received doesn't have a handler yet so nothing is being done with it: [{WorkId}]{WorkType}",
                    request.Id, request.TargetType);
                break;
        }

        // Some updates like GameServerStateUpdate or HostDetail may not be tied to Weaver Work, so we'll skip attempting to update non-existent work
        if (hostId == Guid.Empty)
        {
            return await Result.SuccessAsync();
        }
        
        var workUpdate = await _hostRepository.UpdateWeaverWorkAsync(request);
        if (!workUpdate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootWeaverWork, Guid.Empty, hostId,
                new Dictionary<string, string>
                {
                    {"WorkId", request.Id.ToString()},
                    {"Detail", "Failed to update weaver work"},
                    {"Error", workUpdate.ErrorMessage}
                });
            return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        if (request.Status is not null)
        {
            _eventService.TriggerWeaverWorkStatus("HostServiceWorkUpdate", request.ToEvent());
        }

        return await Result.SuccessAsync();
    }

    private async Task<IResult> HandleHostDetail(WeaverWorkUpdate request, string sourceIp)
    {
        try
        {
            if (request.WorkData is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.WeaverWork.InvalidWorkData);
            }
        
            var deserializedData = _serializerService.DeserializeMemory<HostDetailUpdate>(request.WorkData);
            if (deserializedData is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.WeaverWork.InvalidDeserializedWorkData);
            }

            var foundHost = await _hostRepository.GetByIdAsync(deserializedData.HostId);
            if (!foundHost.Succeeded || foundHost.Result is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.Hosts.NotFound);
            }

            request.HostId = foundHost.Result.Id;

            var convertedRequest = deserializedData.ToUpdate();
            convertedRequest.PublicIp = sourceIp;
            convertedRequest.PrivateIp = convertedRequest.NetworkInterfaces.GetPrimaryIp();
            convertedRequest.FriendlyName = string.IsNullOrWhiteSpace(foundHost.Result.FriendlyName) ? convertedRequest.Hostname : foundHost.Result.FriendlyName;

            var hostUpdate = await _hostRepository.UpdateAsync(convertedRequest.ToUpdateDb());
            if (!hostUpdate.Succeeded)
            {
                var tshootId = await _auditRepository.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootWeaverWork, Guid.Empty,
                    new Dictionary<string, string>
                    {
                        {"WorkId", request.Id.ToString()},
                        {"HostId", request.HostId.ToString() ?? ""},
                        {"Detail", "Failed to update host from weaver work host detail"},
                        {"Error", hostUpdate.ErrorMessage}
                    });
                return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
            }

            var updatedHost = await _hostRepository.GetByIdAsync(foundHost.Result.Id);
            await _auditRepository.CreateAuditTrail(_serverState, _dateTime, AuditTableName.Hosts, foundHost.Result.Id, DatabaseActionType.Update,
                foundHost.Result, updatedHost.Result);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootWeaverWork, Guid.Empty,
                new Dictionary<string, string>
                {
                    {"WorkId", request.Id.ToString()},
                    {"HostId", request.HostId.ToString() ?? ""},
                    {"Detail", "Failed to handle host detail update from weaver work"},
                    {"Error", ex.Message}
                });
            return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
    }
    
    private async Task<IResult> HandleUpdateGameServerState(WeaverWorkUpdate request)
    {
        try
        {
            if (request.WorkData is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.WeaverWork.InvalidWorkData);
            }
        
            var deserializedData = _serializerService.DeserializeMemory<GameServerStateUpdate>(request.WorkData);
            if (deserializedData is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.WeaverWork.InvalidDeserializedWorkData);
            }

            var foundServer = await _gameServerRepository.GetByIdAsync(deserializedData.Id);
            if (!foundServer.Succeeded || foundServer.Result is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
            }

            request.HostId = foundServer.Result.HostId;

            var gameServerUpdate = new GameServerUpdate {Id = foundServer.Result.Id, ServerState = deserializedData.ServerState};
            
            // Update the game server version to the latest if we just did a server update and it was successful
            if (deserializedData.BuildVersionUpdated)
            {
                var gameServerGame = await _gameRepository.GetByIdAsync(foundServer.Result.GameId);
                if (gameServerGame.Result is not null)
                {
                    gameServerUpdate.ServerBuildVersion = gameServerGame.Result.LatestBuildVersion;
                }
            }
            
            var updateGameServer = await _gameServerRepository.UpdateAsync(gameServerUpdate);
            if (!updateGameServer.Succeeded)
            {
                var tshootId = await _auditRepository.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootWeaverWork, foundServer.Result.Id, 
                    new Dictionary<string, string>
                    {
                        {"WorkId", request.Id.ToString()},
                        {"Detail", "Failed to update game server state from weaver work"},
                        {"Error", updateGameServer.ErrorMessage}
                    });
                return await Result.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
            }

            var updatedGameServer = await _gameServerRepository.GetByIdAsync(foundServer.Result.Id);
            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, foundServer.Result.Id, _serverState.SystemUserId,
                DatabaseActionType.Update, foundServer.Result, updatedGameServer.Result);

            var serverStatusEvent = deserializedData.ToStatusEvent();
            serverStatusEvent.ServerName = foundServer.Result.ServerName;
            
            _eventService.TriggerGameServerStatus("HostServiceGameServerStateUpdate", serverStatusEvent);

            if (deserializedData.ServerState != ConnectivityState.Uninstalled) return await Result.SuccessAsync();
            
            // State update is uninstalled, so we'll wrap up by deleting the game server
            var deleteServerRequest = await _gameServerRepository.DeleteAsync(foundServer.Result.Id, _serverState.SystemUserId);
            if (!deleteServerRequest.Succeeded)
            {
                return await Result.FailAsync(deleteServerRequest.ErrorMessage);
            }
        
            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameServers, foundServer.Result.Id, (Guid)foundServer.Result.LastModifiedBy!,
                DatabaseActionType.Delete, foundServer.Result);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_serverState, _dateTime, AuditTableName.TshootWeaverWork, Guid.Empty,
                new Dictionary<string, string>
                {
                    {"WorkId", request.Id.ToString()},
                    {"HostId", request.HostId.ToString() ?? ""},
                    {"Detail", "Failed to handle game server state update from weaver work"},
                    {"Error", ex.Message}
                });
            return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
    }

    public async Task<IResult> DeleteWeaverWorkAsync(int id, Guid requestUserId)
    {
        var foundWork = await _hostRepository.GetWeaverWorkByIdAsync(id);
        if (foundWork.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.WeaverWork.NotFound);
        }

        var workDelete = await _hostRepository.DeleteWeaverWorkAsync(id);
        if (workDelete.Succeeded) return await Result.SuccessAsync();
        
        var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootWeaverWork, Guid.Empty, requestUserId,
            new Dictionary<string, string>
            {
                {"WorkId", id.ToString()},
                {"Detail", "Failed to delete weaver work"},
                {"Error", workDelete.ErrorMessage}
            });
        return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
    }

    public async Task<IResult> DeleteWeaverWorkForHostAsync(Guid hostId, Guid requestUserId)
    {
        var workDelete = await _hostRepository.DeleteWeaverWorkForHostAsync(hostId);
        if (workDelete.Succeeded) return await Result.SuccessAsync();
        
        var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootWeaverWork, Guid.Empty, requestUserId,
            new Dictionary<string, string>
            {
                {"HostId", hostId.ToString()},
                {"Detail", "Failed to delete weaver work for host"},
                {"Error", workDelete.ErrorMessage}
            });
        return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
    }

    public async Task<IResult> DeleteWeaverWorkOlderThanAsync(DateTime olderThan, Guid requestUserId)
    {
        var workDelete = await _hostRepository.DeleteWeaverWorkOlderThanAsync(olderThan);
        if (workDelete.Succeeded) return await Result.SuccessAsync();
        
        var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootWeaverWork, Guid.Empty, requestUserId,
            new Dictionary<string, string>
            {
                {"Timeframe", olderThan.ToString(DataConstants.DateTime.DisplayFormat)},
                {"Detail", "Failed to delete weaver work for a given timeframe"},
                {"Error", workDelete.ErrorMessage}
            });
        return await Result<int>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
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