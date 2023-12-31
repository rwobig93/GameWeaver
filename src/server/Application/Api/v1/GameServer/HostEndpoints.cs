using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.Web;
using Application.Requests.v1.GameServer;
using Application.Responses.v1.GameServer;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Services.System;

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
        app.MapPost(ApiRouteConstants.GameServer.Host.CheckIn, GetToken).ApiVersionOne();
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
    /// <returns>Host ID and Host Token for use when authenticating the host</returns>
    private static async Task<IResult<HostRegisterResponse>> RegistrationConfirm(HostRegisterRequest request, IHostService hostService)
    {
        try
        {
            // TODO: Get request IP Address
            return await hostService.RegistrationConfirm(request, "");
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
    /// <returns>Success or Failure with no return payload</returns>
    private static async Task<IResult> Checkin(HostCheckInRequest request, IHostService hostService, ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        try
        {
            // TODO: Add return with host job list
            var currentUserId = await currentUserService.GetApiCurrentUserId();

            var createCheckIn = new HostCheckInCreate
            {
                HostId = currentUserId,
                SendTimestamp = request.SendTimestamp,
                ReceiveTimestamp = dateTimeService.NowDatabaseTime,
                CpuUsage = request.CpuUsage,
                RamUsage = request.RamUsage,
                Uptime = request.Uptime,
                NetworkOutMb = request.NetworkOutMb,
                NetworkInMb = request.NetworkInMb
            };
            
            return await hostService.CreateCheckInAsync(createCheckIn);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
}