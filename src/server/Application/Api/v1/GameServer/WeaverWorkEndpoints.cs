using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Mappers.GameServer;
using Application.Models.GameServer.WeaverWork;
using Application.Requests.GameServer.WeaverWork;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Domain.Enums.GameServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class WeaverWorkEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsWeaverWork(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.WeaverWork.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.WeaverWork.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.WeaverWork.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.WeaverWork.GetByStatus, GetByStatus).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.WeaverWork.GetByType, GetByType).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.WeaverWork.GetWaitingForHost, GetWaitingForHost).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.WeaverWork.GetAllWaitingForHost, GetAllWaitingForHost).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.WeaverWork.Create, Create).ApiVersionOne();
        app.MapPatch(ApiRouteConstants.GameServer.WeaverWork.Update, Update).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.WeaverWork.UpdateStatus, WorkStatusUpdate).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.WeaverWork.Delete, Delete).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.WeaverWork.DeleteOld, DeleteOld).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.WeaverWork.Search, Search).ApiVersionOne();
    }
        
    /// <summary>
    /// Get all weaver work with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="hostService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of weaver work</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IHostService hostService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await hostService.GetAllWeaverWorkPaginatedAsync(pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<WeaverWorkSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.WeaverWork.GetAll, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.WeaverWork.GetAll, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get count of total weaver work
    /// </summary>
    /// <param name="hostService"></param>
    /// <returns>Weaver work count</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.GetCount)]
    private static async Task<IResult<int>> GetCount(IHostService hostService)
    {
        try
        {
            return await hostService.GetWeaverWorkCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a unit of weaver work by id
    /// </summary>
    /// <param name="id">Id of the weaver work</param>
    /// <param name="hostService"></param>
    /// <returns>Weaver work object</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Get)]
    private static async Task<IResult<WeaverWorkSlim>> GetById([FromQuery]int id, IHostService hostService)
    {
        try
        {
            return await hostService.GetWeaverWorkByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<WeaverWorkSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get weaver work by status
    /// </summary>
    /// <param name="status">0=Waiting, 1=PickedUp, 2=InProgress, 3=Completed, 4=Cancelled, 5=Failed</param>
    /// <param name="hostService"></param>
    /// <returns>List of weaver work matching the status</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Get)]
    private static async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetByStatus([FromQuery]WeaverWorkState status, IHostService hostService)
    {
        try
        {
            return await hostService.GetWeaverWorkByStatusAsync(status);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get weaver work by target type
    /// </summary>
    /// <param name="target">001=StatusUpdate, 100=Host, 101=HostStatusUpdate, 102=HostDetail, 200=GameServer, 201=GameServerInstall, 202=GameServerUpdate,
    /// 203=GameServerUninstall, 204=GameServerStateUpdate</param>
    /// <param name="hostService"></param>
    /// <returns>List of weaver work matching the target type</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Get)]
    private static async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetByType([FromQuery]WeaverWorkTarget target, IHostService hostService)
    {
        try
        {
            return await hostService.GetWeaverWorkByTargetTypeAsync(target);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Gets the 10 oldest waiting Weaver work jobs for a given host id
    /// </summary>
    /// <param name="id">Id of the host to get weaver work for</param>
    /// <param name="hostService"></param>
    /// <returns>Up to 10 of the latest weaver work jobs</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Get)]
    private static async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetWaitingForHost([FromQuery]Guid id, IHostService hostService)
    {
        try
        {
            return await hostService.GetWeaverWaitingWorkByHostIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Gets all of the currently waiting weaver work jobs for a given host id
    /// </summary>
    /// <param name="id">Id of the host to get weaver work for</param>
    /// <param name="hostService"></param>
    /// <returns>All the current waiting weaver work jobs</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Get)]
    private static async Task<IResult<IEnumerable<WeaverWorkSlim>>> GetAllWaitingForHost([FromQuery]Guid id, IHostService hostService)
    {
        try
        {
            return await hostService.GetWeaverAllWaitingWorkByHostIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create weaver work for a host
    /// </summary>
    /// <param name="request">Required properties to create weaver work for a host</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>ID of the weaver work created</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Create)]
    private static async Task<IResult<int>> Create([FromBody]WeaverWorkCreateRequest request, IHostService hostService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await hostService.CreateWeaverWorkAsync(request.ToCreate(), currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Update weaver work properties
    /// </summary>
    /// <param name="request">Weaver work properties to update</param>
    /// <param name="hostService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Update)]
    private static async Task<IResult> Update([FromBody]WeaverWorkUpdate request, IHostService hostService)
    {
        try
        {
            return await hostService.UpdateWeaverWorkAsync(request);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
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
    /// <param name="context"></param>
    /// <returns></returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.UpdateStatus)]
    private static async Task<IResult> WorkStatusUpdate([FromBody]WeaverWorkUpdate request, IHostService hostService, ICurrentUserService currentUserService,
        IDateTimeService dateTimeService, ISerializerService serializerService, HttpContext context)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            var initiatorIp = context.GetConnectionIp();
            
            request.CreatedBy = null;
            request.CreatedOn = null;
            request.LastModifiedBy = currentUserId;
            request.LastModifiedOn = dateTimeService.NowDatabaseTime;

            return await hostService.UpdateWeaverWorkAsync(request, initiatorIp);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete weaver work
    /// </summary>
    /// <param name="id">Id of a weaver work</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Delete)]
    private static async Task<IResult> Delete([FromQuery]int id, IHostService hostService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await hostService.DeleteWeaverWorkAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete weaver work older than a timeframe
    /// </summary>
    /// <param name="olderThan">Serializable DateTime, anything older than this datetime will be deleted</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Delete)]
    private static async Task<IResult> DeleteOld([FromQuery]DateTime olderThan, IHostService hostService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await hostService.DeleteWeaverWorkOlderThanAsync(olderThan, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Search for weaver work by properties
    /// </summary>
    /// <param name="searchText">Text to search for</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="hostService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of matching weaver work</returns>
    /// <remarks>Search by: ID, HostId</remarks>
    [Authorize(PermissionConstants.GameServer.WeaverWork.Search)]
    private static async Task<IResult<IEnumerable<WeaverWorkSlim>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IHostService hostService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await hostService.SearchWeaverWorkPaginatedAsync(searchText, pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<WeaverWorkSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.WeaverWork.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.WeaverWork.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<WeaverWorkSlim>>.FailAsync(ex.Message);
        }
    }
}