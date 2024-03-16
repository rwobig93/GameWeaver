using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Mappers.GameServer;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.WeaverWork;
using Application.Requests.v1.GameServer;
using Application.Responses.v1.GameServer;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Services.System;
using Domain.Contracts;
using Microsoft.AspNetCore.Http;

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
        app.MapPost(ApiRouteConstants.GameServer.Host.GetRegistration, RegistrationGenerateNew).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Host.RegistrationConfirm, RegistrationConfirm).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Host.GetToken, GetToken).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Host.CheckIn, Checkin).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Host.UpdateWorkStatus, WorkStatusUpdate).ApiVersionOne();
    }

    /// <summary>
    /// Generates a new registration token to register a new host, the host must then use the registration URI to complete registration
    /// </summary>
    /// <param name="description">A unique description of the host registration, intended to be a detailed identifier</param>
    /// <param name="hostOwnerId">ID of the account that will be the owner of this host</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Host ID, Key and full registration confirmation URI for the new host to complete registration</returns>
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
    private static async Task<IResult> WorkStatusUpdate(byte[] request, IHostService hostService, ICurrentUserService currentUserService,
        IDateTimeService dateTimeService, ISerializerService serializerService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();

            var deserializedRequest = serializerService.DeserializeMemory<WeaverWorkUpdate>(request);
            if (deserializedRequest is null)
                return await Result<IEnumerable<WeaverWorkClient>>.FailAsync("Invalid work update request provided, please verify your payload");

            var workUpdate = new WeaverWorkUpdate
            {
                Id = deserializedRequest.Id,
                HostId = deserializedRequest.HostId,
                GameServerId = deserializedRequest.GameServerId,
                TargetType = deserializedRequest.TargetType,
                Status = deserializedRequest.Status,
                WorkData = deserializedRequest.WorkData,
                CreatedBy = currentUserId,
                CreatedOn = dateTimeService.NowDatabaseTime,
                LastModifiedBy = currentUserId,
                LastModifiedOn = dateTimeService.NowDatabaseTime
            };

            var workUpdateResponse = await hostService.UpdateWeaverWorkAsync(workUpdate);
            if (!workUpdateResponse.Succeeded)
                return await Result.FailAsync(workUpdateResponse.Messages);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
}