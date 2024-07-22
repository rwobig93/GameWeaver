using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Mappers.GameServer;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.WeaverWork;
using Application.Requests.GameServer.Host;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class HostCheckinEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsHostCheckin(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRouteConstants.GameServer.HostCheckins.CheckIn, Checkin).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostCheckins.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostCheckins.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostCheckins.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostCheckins.GetByHost, GetByHostId).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.HostCheckins.DeleteOld, DeleteOld).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostCheckins.Search, Search).ApiVersionOne();
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
    [Authorize(PermissionConstants.GameServer.HostCheckins.CheckIn)]
    private static async Task<IResult<IEnumerable<WeaverWorkClient>>> Checkin([FromBody]HostCheckInRequest request, IHostService hostService, ICurrentUserService currentUserService,
        IDateTimeService dateTimeService, ISerializerService serializerService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            
            var createCheckIn = new HostCheckInCreate
            {
                HostId = currentUserId,
                SendTimestamp = request.SendTimestamp,
                ReceiveTimestamp = dateTimeService.NowDatabaseTime,
                CpuUsage = request.CpuUsage,
                RamUsage = request.RamUsage,
                Uptime = request.Uptime,
                NetworkOutBytes = request.NetworkOutBytes,
                NetworkInBytes = request.NetworkInBytes
            };
            
            var checkInResponse = await hostService.CreateCheckInAsync(createCheckIn);
            if (!checkInResponse.Succeeded)
            {
                return await Result<IEnumerable<WeaverWorkClient>>.FailAsync(checkInResponse.Messages);
            }

            var nextHostWork = await hostService.GetWeaverWaitingWorkByHostIdAsync(currentUserId);
            return await Result<IEnumerable<WeaverWorkClient>>.SuccessAsync(nextHostWork.Data.ToClientWorks());
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<WeaverWorkClient>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get all checkins with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="hostService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of host checkins</returns>
    [Authorize(PermissionConstants.GameServer.HostCheckins.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<HostCheckInFull>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IHostService hostService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await hostService.GetAllCheckInsPaginatedAsync(pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<HostCheckInFull>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.HostCheckins.GetAll, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.HostCheckins.GetAll, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get total count of host checkins
    /// </summary>
    /// <param name="hostService"></param>
    /// <returns>Count of host checkins</returns>
    [Authorize(PermissionConstants.GameServer.HostCheckins.GetCount)]
    private static async Task<IResult<int>> GetCount(IHostService hostService)
    {
        try
        {
            return await hostService.GetCheckInCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a host checkin by id
    /// </summary>
    /// <param name="id">Id of the checkin</param>
    /// <param name="hostService"></param>
    /// <returns>Host checkin</returns>
    [Authorize(PermissionConstants.GameServer.HostCheckins.Get)]
    private static async Task<IResult<HostCheckInFull>> GetById([FromQuery]int id, IHostService hostService)
    {
        try
        {
            return await hostService.GetCheckInByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<HostCheckInFull>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a host checkins by the host ID
    /// </summary>
    /// <param name="id">Id of the host</param>
    /// <param name="hostService"></param>
    /// <returns>List of checkins for the host</returns>
    [Authorize(PermissionConstants.GameServer.HostCheckins.GetByHost)]
    private static async Task<IResult<IEnumerable<HostCheckInFull>>> GetByHostId([FromQuery]Guid id, IHostService hostService)
    {
        try
        {
            return await hostService.GetChecksInByHostIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete host checkins older than the provided timeframe
    /// </summary>
    /// <param name="olderThanDays">Number of days to remove checkins after</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <param name="dateTime"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.HostCheckins.DeleteOld)]
    private static async Task<IResult> DeleteOld([FromBody]int olderThanDays, IHostService hostService, ICurrentUserService currentUserService, IDateTimeService dateTime)
    {
        try
        {
            var workCleanupTimestamp = dateTime.NowDatabaseTime.AddDays(-olderThanDays);
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await hostService.DeleteAllOldCheckInsAsync(workCleanupTimestamp, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Search for checkins by properties
    /// </summary>
    /// <param name="searchText">Text to search by</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="hostService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of matching host checkins</returns>
    /// <remarks>Searches by: ID, HostId</remarks>
    [Authorize(PermissionConstants.GameServer.HostCheckins.Search)]
    private static async Task<IResult<IEnumerable<HostCheckInFull>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IHostService hostService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await hostService.SearchCheckInsPaginatedAsync(searchText, pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<HostCheckInFull>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.HostCheckins.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.HostCheckins.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostCheckInFull>>.FailAsync(ex.Message);
        }
    }
}