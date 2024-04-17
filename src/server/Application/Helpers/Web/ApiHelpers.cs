using Application.Constants.Web;
using Application.Responses.v1.Identity;
using Application.Services.Identity;
using Domain.Exceptions;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Helpers.Web;

public static class ApiHelpers
{
    public static void ApiVersionOne(this RouteHandlerBuilder apiMethod) =>
        apiMethod
            .WithApiVersionSet(ApiConstants.SupportsVersionOne!)
            .HasApiVersion(ApiConstants.Version1);
    
    public static void ApiVersionTwo(this RouteHandlerBuilder apiMethod) =>
        apiMethod
            .WithApiVersionSet(ApiConstants.SupportsVersionOne!)
            .HasApiVersion(ApiConstants.Version2);

    public static async Task<UserBasicResponse> GetApiCurrentUserBasic(this ICurrentUserService currentUserService)
    {
        var currentUser = await currentUserService.GetCurrentUserBasic();
        if (currentUser is null)
            throw new ApiException("You aren't currently authenticated, please authenticate and try again");

        return currentUser;
    }

    public static async Task<Guid> GetApiCurrentUserId(this ICurrentUserService currentUserService)
    {
        var currentUserId = await currentUserService.GetCurrentUserId();
        var isGuid = Guid.TryParse(currentUserId.ToString(), out var userId);
        if (currentUserId is null || !isGuid)
            throw new ApiException("You aren't currently authenticated, please authenticate and try again");

        return userId;
    }

    public static string GetPaginatedUrl(this string baseUrl, string endpoint, int pageNumber, int pageSize)
    {
        var endpointUrl = new Uri(string.Concat(baseUrl, endpoint));
        var pageNumberUri = QueryHelpers.AddQueryString(endpointUrl.ToString(), "pageNumber", pageNumber.ToString());
        var fullUri = QueryHelpers.AddQueryString(pageNumberUri, "pageSize", pageSize.ToString());
        
        return fullUri;
    }

    public static string? GetPaginatedPreviousUrl(this string baseUrl, string endpoint, int pageNumber, int pageSize)
    {
        return pageNumber >= 1 ? baseUrl.GetPaginatedUrl(endpoint, pageNumber - 1, pageSize) : null;
    }

    public static string? GetPaginatedNextUrl(this string baseUrl, string endpoint, int pageNumber, int pageSize, int totalCount)
    {
        return (pageNumber + 1) * pageSize < totalCount ? baseUrl.GetPaginatedUrl(endpoint, pageNumber + 1, pageSize) : null;
    }
}