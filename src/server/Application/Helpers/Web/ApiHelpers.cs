using Application.Constants.Web;
using Application.Responses.Identity;
using Application.Services.Identity;
using Domain.Exceptions;

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
}