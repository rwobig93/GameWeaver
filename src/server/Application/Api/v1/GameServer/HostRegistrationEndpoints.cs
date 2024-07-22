using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.HostRegistration;
using Application.Requests.GameServer.Host;
using Application.Responses.v1.GameServer;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class HostRegistrationEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsHostRegistration(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRouteConstants.GameServer.HostRegistration.Create, Create).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.HostRegistration.Confirm, Confirm).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostRegistration.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostRegistration.GetActive, GetAllActive).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostRegistration.GetInActive, GetAllInActive).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostRegistration.GetCount, GetCount).ApiVersionOne();
        app.MapPatch(ApiRouteConstants.GameServer.HostRegistration.Update, Update).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.HostRegistration.Search, Search).ApiVersionOne();
    }

    /// <summary>
    /// Generates a new registration token to register a new host, the host must then use the registration URI to complete registration
    /// </summary>
    /// <param name="request">Required properties to generate a host registration</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Host ID, Key and full registration confirmation URI for the new host to complete registration</returns>
    [Authorize(PermissionConstants.GameServer.HostRegistration.Create)]
    private static async Task<IResult<HostNewRegisterResponse>> Create([FromBody]HostRegistrationCreateRequest request, IHostService hostService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await hostService.RegistrationGenerateNew(request, currentUserId);
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
    [AllowAnonymous]
    private static async Task<IResult<HostRegisterResponse>> Confirm([FromBody]HostRegistrationConfirmRequest request, IHostService hostService, HttpContext context)
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
    /// Get all registrations with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="hostService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of host registrations</returns>
    [Authorize(PermissionConstants.GameServer.HostRegistration.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IHostService hostService,
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await hostService.GetAllRegistrationsPaginatedAsync(pageNumber, pageSize) as PaginatedResult<IEnumerable<HostRegistrationFull>>;
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<HostRegistrationFull>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.HostRegistration.GetAll, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.HostRegistration.GetAll, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get active host registrations
    /// </summary>
    /// <param name="hostService"></param>
    /// <returns>List of host registrations</returns>
    [Authorize(PermissionConstants.GameServer.HostRegistration.GetAllActive)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllActive(IHostService hostService)
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
    
    /// <summary>
    /// Get inactive host registrations
    /// </summary>
    /// <param name="hostService"></param>
    /// <returns>List of host registrations</returns>
    [Authorize(PermissionConstants.GameServer.HostRegistration.GetAllInActive)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> GetAllInActive(IHostService hostService)
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
    
    /// <summary>
    /// Get the count of all host registrations
    /// </summary>
    /// <param name="hostService"></param>
    /// <returns>Count of host registrations</returns>
    [Authorize(PermissionConstants.GameServer.HostRegistration.GetCount)]
    private static async Task<IResult<int>> GetCount(IHostService hostService)
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

    /// <summary>
    /// Update a host registration's properties
    /// </summary>
    /// <param name="request">Required properties to update a host registration</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.HostRegistration.Update)]
    private static async Task<IResult> Update([FromBody]HostRegistrationUpdateRequest request, IHostService hostService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await hostService.UpdateRegistrationAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Search for registrations by properties
    /// </summary>
    /// <param name="searchText">Text to search by</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="hostService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of matching registrations</returns>
    /// <remarks>Searches by ID, HostId, Key, Description and Public Ip</remarks>
    [Authorize(PermissionConstants.GameServer.HostRegistration.Search)]
    private static async Task<IResult<IEnumerable<HostRegistrationFull>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IHostService hostService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await hostService.SearchRegistrationsPaginatedAsync(searchText, pageNumber, pageSize) as PaginatedResult<IEnumerable<HostRegistrationFull>>;
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<HostRegistrationFull>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.HostRegistration.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.HostRegistration.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostRegistrationFull>>.FailAsync(ex.Message);
        }
    }
}