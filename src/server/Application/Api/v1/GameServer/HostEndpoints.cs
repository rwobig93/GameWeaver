using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.HostRegistration;
using Application.Models.GameServer.WeaverWork;
using Application.Requests.v1.GameServer;
using Application.Responses.v1.GameServer;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Services.System;
using Domain.Contracts;
using Domain.Enums.GameServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Application.Api.v1.GameServer;

/// <summary>
/// Endpoints dealing with game server host operations
/// </summary>
public static class HostEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsHost(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRouteConstants.GameServer.Host.CreateRegistration, RegistrationGenerateNew).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Host.RegistrationConfirm, RegistrationConfirm).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Host.GetToken, GetToken).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Host.CheckIn, Checkin).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Host.UpdateWorkStatus, WorkStatusUpdate).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Host.GetAll, GetAll).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Host.GetAllRegistrations, GetAllRegistrations).ApiVersionOne();
    }

    /// <summary>
    /// Generates a new registration token to register a new host, the host must then use the registration URI to complete registration
    /// </summary>
    /// <param name="description">A unique description of the host registration, intended to be a detailed identifier</param>
    /// <param name="hostOwnerId">ID of the account that will be the owner of this host</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Host ID, Key and full registration confirmation URI for the new host to complete registration</returns>
    [Authorize(PermissionConstants.Hosts.CreateRegistration)]
    private static async Task<IResult<HostNewRegisterResponse>> RegistrationGenerateNew(string description, Guid hostOwnerId,
        IHostService hostService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUser = await currentUserService.GetApiCurrentUserBasic();
            return await hostService.RegistrationGenerateNew(description, currentUser.Id, hostOwnerId);
        }
        catch (Exception ex)
        {
            return await Result<HostNewRegisterResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Confirms and completes an active registration
    /// </summary>
    /// <param name="request">Host ID and Key of a valid and active registration</param>
    /// <param name="hostService"></param>
    /// <param name="context"></param>
    /// <returns>Host ID and Host Token for use when authenticating the host</returns>
    [Authorize(PermissionConstants.Hosts.Register)]
    private static async Task<IResult<HostRegisterResponse>> RegistrationConfirm(HostRegisterRequest request, IHostService hostService, HttpContext context)
    {
        try
        {
            var initiatorIp = context.GetConnectionIp();
            return await hostService.RegistrationConfirm(request, initiatorIp);
        }
        catch (Exception ex)
        {
            return await Result<HostRegisterResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Generate a valid authorization token for use in host API endpoints
    /// </summary>
    /// <param name="request">Valid and active Host ID and Host Key</param>
    /// <param name="hostService"></param>
    /// <returns>JWT with an expiration datetime in GMT/UTC</returns>
    /// <remarks>
    /// - Expiration time returned is in GMT/UTC
    /// </remarks>
    [AllowAnonymous]
    private static async Task<IResult<HostAuthResponse>> GetToken(HostAuthRequest request, IHostService hostService)
    {
        try
        {
            return await hostService.GetToken(request);
        }
        catch (Exception ex)
        {
            return await Result<HostAuthResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Inject a valid host check-in status
    /// </summary>
    /// <param name="request">Host check-in details</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <param name="dateTimeService"></param>
    /// <param name="serializerService"></param>
    /// <returns>Success or Failure, payload is a serialized list of work for the host to process</returns>
    [Authorize(PermissionConstants.Hosts.CheckIn)]
    private static async Task<IResult<byte[]>> Checkin(byte[] request, IHostService hostService, ICurrentUserService currentUserService,
        IDateTimeService dateTimeService, ISerializerService serializerService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();

            var deserializedRequest = serializerService.DeserializeMemory<HostCheckInRequest>(request);
            if (deserializedRequest is null)
                return await Result<byte[]>.FailAsync("Invalid checkin request provided, please verify your payload");

            var createCheckIn = new HostCheckInCreate
            {
                HostId = currentUserId,
                SendTimestamp = deserializedRequest.SendTimestamp,
                ReceiveTimestamp = dateTimeService.NowDatabaseTime,
                CpuUsage = deserializedRequest.CpuUsage,
                RamUsage = deserializedRequest.RamUsage,
                Uptime = deserializedRequest.Uptime,
                NetworkOutMb = deserializedRequest.NetworkOutMb,
                NetworkInMb = deserializedRequest.NetworkInMb
            };

            var checkInResponse = await hostService.CreateCheckInAsync(createCheckIn);
            if (!checkInResponse.Succeeded)
                return await Result<byte[]>.FailAsync(checkInResponse.Messages);

            var nextHostWork = await hostService.GetWeaverWaitingWorkByHostIdAsync(currentUserId);
            var serializedHostWork = serializerService.SerializeMemory(nextHostWork.Data.ToClientWorks());
            return await Result<byte[]>.SuccessAsync(serializedHostWork);
        }
        catch (Exception ex)
        {
            return await Result<byte[]>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Update the status of requested weaver work from the host
    /// </summary>
    /// <param name="request">Work status update from the host</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <param name="dateTimeService"></param>
    /// <param name="serializerService"></param>
    /// <returns></returns>
    [Authorize(PermissionConstants.Hosts.WorkUpdate)]
    private static async Task<IResult> WorkStatusUpdate(byte[] request, IHostService hostService, ICurrentUserService currentUserService,
        IDateTimeService dateTimeService, ISerializerService serializerService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();

            var deserializedRequest = serializerService.DeserializeMemory<WeaverWorkUpdate>(request);
            if (deserializedRequest is null)
                return await Result<IEnumerable<WeaverWorkClient>>.FailAsync("Invalid work update request provided, please verify your payload");

            deserializedRequest.CreatedBy = null;
            deserializedRequest.CreatedOn = null;
            deserializedRequest.LastModifiedBy = currentUserId;
            deserializedRequest.LastModifiedOn = dateTimeService.NowDatabaseTime;

            return await hostService.UpdateWeaverWorkAsync(deserializedRequest);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get all game server hosts
    /// </summary>
    /// <param name="hostService"></param>
    /// <returns>List of game server hosts</returns>
    [Authorize(PermissionConstants.Hosts.GetAll)]
    private static async Task<IResult<IEnumerable<HostSlim>>> GetAll(IHostService hostService)
    {
        try
        {
            return await hostService.GetAllAsync();
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostSlim>>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<HostSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IHostService hostService)
    {
        try
        {
            return await hostService.GetAllPaginatedAsync(pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostSlim>>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.Get)]
    private static async Task<IResult<HostSlim>> GetById([FromQuery]Guid id, IHostService hostService)
    {
        try
        {
            return await hostService.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<HostSlim>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.Get)]
    private static async Task<IResult<HostSlim>> GetByHostname([FromQuery]string hostname, IHostService hostService)
    {
        try
        {
            return await hostService.GetByHostnameAsync(hostname);
        }
        catch (Exception ex)
        {
            return await Result<HostSlim>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]HostCreate request, IHostService hostService)
    {
        try
        {
            return await hostService.CreateAsync(request);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.Update)]
    private static async Task<IResult> Update([FromBody]HostUpdate request, IHostService hostService)
    {
        try
        {
            return await hostService.UpdateAsync(request);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.Delete)]
    private static async Task<IResult> Delete([FromQuery]Guid id, IHostService hostService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUser = await currentUserService.GetApiCurrentUserBasic();
            return await hostService.DeleteAsync(id, currentUser.Id);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.Search)]
    private static async Task<IResult<IEnumerable<HostSlim>>> Search([FromQuery]string searchText, IHostService hostService)
    {
        try
        {
            return await hostService.SearchAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostSlim>>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.SearchPaginated)]
    private static async Task<IResult<IEnumerable<HostSlim>>> SearchPaginated([FromQuery]string searchText, [FromQuery]int pageNumber,
        [FromQuery]int pageSize, IHostService hostService)
    {
        try
        {
            return await hostService.SearchPaginatedAsync(searchText, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get all game server host registrations
    /// </summary>
    /// <param name="hostService"></param>
    /// <returns>List of game server host registrations</returns>
    [Authorize(PermissionConstants.Hosts.GetAllRegistrations)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrations(IHostService hostService)
    {
        try
        {
            var allRegistrations = await hostService.GetAllRegistrationsAsync();
            return await Result<IEnumerable<HostRegistrationFull>>.SuccessAsync(allRegistrations.Data);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.GetAllRegistrationsPaginated)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrationsPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IHostService hostService)
    {
        try
        {
            return await hostService.GetAllRegistrationsPaginatedAsync(pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.GetAllRegistrationsActive)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrationsActive(IHostService hostService)
    {
        try
        {
            return await hostService.GetAllActiveRegistrationsAsync();
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.GetAllRegistrationsInActive)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllRegistrationsInActive(IHostService hostService)
    {
        try
        {
            return await hostService.GetAllInActiveRegistrationsAsync();
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.GetRegistrationsCount)]
    private static async Task<IResult<int>> GetRegistrationsCount(IHostService hostService)
    {
        try
        {
            return await hostService.GetRegistrationCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.UpdateRegistration)]
    private static async Task<IResult> UpdateRegistration([FromBody]HostRegistrationUpdate request, IHostService hostService)
    {
        try
        {
            return await hostService.UpdateRegistrationAsync(request);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.SearchRegistrations)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> SearchRegistrations([FromQuery]string searchText, IHostService hostService)
    {
        try
        {
            return await hostService.SearchRegistrationsAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(ex.Message);
        }
    }
    
    
    [Authorize(PermissionConstants.Hosts.SearchRegistrationsPaginated)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> SearchRegistrationsPaginated([FromQuery]string searchText, [FromQuery]int pageNumber,
        [FromQuery]int pageSize, IHostService hostService)
    {
        try
        {
            return await hostService.SearchRegistrationsPaginatedAsync(searchText, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(ex.Message);
        }
    }
}