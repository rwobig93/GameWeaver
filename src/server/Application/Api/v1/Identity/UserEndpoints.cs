using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Runtime;
using Application.Helpers.Web;
using Application.Mappers.Identity;
using Application.Models.Web;
using Application.Requests.v1.Identity.User;
using Application.Responses.v1.Identity;
using Application.Services.Identity;
using Domain.Contracts;
using Domain.Enums.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.v1.Identity;

/// <summary>
/// API endpoints for application users
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Register user API endpoints
    /// </summary>
    /// <param name="app"></param>
    public static void MapEndpointsUsers(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.Identity.User.GetAll, GetAllUsers).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.User.GetById, GetUserById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.User.GetFullById, GetFullUserById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.User.GetByEmail, GetUserByEmail).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.User.GetFullByEmail, GetFullUserByEmail).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.User.GetByUsername, GetUserByUsername).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.User.GetFullByUsername, GetFullUserByUsername).ApiVersionOne();
        
        app.MapPut(ApiRouteConstants.Identity.User.Update, UpdateUser).ApiVersionOne();

        app.MapPost(ApiRouteConstants.Identity.User.Create, CreateUser).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.User.Register, Register).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.User.ResetPassword, ResetPassword).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.User.Enable, EnableUser).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.User.Disable, DisableUser).ApiVersionOne();
        
        app.MapDelete(ApiRouteConstants.Identity.User.Delete, DeleteUser).ApiVersionOne();
    }

    /// <summary>
    /// Register a user account
    /// </summary>
    /// <param name="registerRequest">Details used to register a user account</param>
    /// <param name="accountService"></param>
    /// <returns></returns>
    [AllowAnonymous]
    private static async Task<IWebResult> Register(UserRegisterRequest registerRequest, IAppAccountService accountService)
    {
        try
        {
            var request = await accountService.RegisterAsync(registerRequest);
            if (!request.Succeeded) return await WebResult.ResultAsync(await Result.FailAsync(request.Messages));
            return await WebResult.ResultAsync(await Result.SuccessAsync("Successfully registered, please check the email provided for details!"));
        }
        catch (Exception ex)
        {
            return await WebResult.FailureAsync(await Result.FailAsync(ex.Message));
        }
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <param name="userService"></param>
    /// <returns>List of all users</returns>
    [Authorize(Policy = PermissionConstants.Users.View)]
    private static async Task<IResult<List<UserBasicResponse>>> GetAllUsers(IAppUserService userService)
    {
        try
        {
            var allUsers = await userService.GetAllAsync();
            if (!allUsers.Succeeded)
                return await Result<List<UserBasicResponse>>.FailAsync(allUsers.Messages);

            return await Result<List<UserBasicResponse>>.SuccessAsync(allUsers.Data.ToResponses());
        }
        catch (Exception ex)
        {
            return await Result<List<UserBasicResponse>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get the specified user
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="userService"></param>
    /// <returns>Detail regarding the specified user</returns>
    [Authorize(Policy = PermissionConstants.Users.View)]
    private static async Task<IResult<UserBasicResponse>> GetUserById([FromQuery]Guid userId, IAppUserService userService)
    {
        try
        {
            var foundUser = await userService.GetByIdAsync(userId);
            if (!foundUser.Succeeded)
                return await Result<UserBasicResponse>.FailAsync(foundUser.Messages);

            if (foundUser.Data is null)
                return await Result<UserBasicResponse>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);

            return await Result<UserBasicResponse>.SuccessAsync(foundUser.Data.ToResponse());
        }
        catch (Exception ex)
        {
            return await Result<UserBasicResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get full user detail for a user including permissions and extended attributes
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="userService"></param>
    /// <returns>Detail for the specified user including permissions and extended attributes</returns>
    [Authorize(Policy = PermissionConstants.Users.View)]
    private static async Task<IResult<UserFullResponse>> GetFullUserById([FromQuery]Guid userId, IAppUserService userService)
    {
        try
        {
            var foundUser = await userService.GetByIdFullAsync(userId);
            if (!foundUser.Succeeded)
                return await Result<UserFullResponse>.FailAsync(foundUser.Messages);

            if (foundUser.Data is null)
                return await Result<UserFullResponse>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);

            return await Result<UserFullResponse>.SuccessAsync(foundUser.Data.ToResponse());
        }
        catch (Exception ex)
        {
            return await Result<UserFullResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get a user by email
    /// </summary>
    /// <param name="email">Email address of the user</param>
    /// <param name="userService"></param>
    /// <returns>Detail for the specified user</returns>
    [Authorize(Policy = PermissionConstants.Users.View)]
    private static async Task<IResult<UserBasicResponse>> GetUserByEmail([FromQuery]string email, IAppUserService userService)
    {
        try
        {
            var foundUser = await userService.GetByEmailAsync(email);
            if (!foundUser.Succeeded)
                return await Result<UserBasicResponse>.FailAsync(foundUser.Messages);

            if (foundUser.Data is null)
                return await Result<UserBasicResponse>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);

            return await Result<UserBasicResponse>.SuccessAsync(foundUser.Data.ToResponse());
        }
        catch (Exception ex)
        {
            return await Result<UserBasicResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get full user detail for a user including permissions and extended attributes
    /// </summary>
    /// <param name="email">Email address for the user</param>
    /// <param name="userService"></param>
    /// <returns>Detail for the specified user including permissions and extended attributes</returns>
    [Authorize(Policy = PermissionConstants.Users.View)]
    private static async Task<IResult<UserFullResponse>> GetFullUserByEmail([FromQuery]string email, IAppUserService userService)
    {
        try
        {
            var foundUser = await userService.GetByEmailFullAsync(email);
            if (!foundUser.Succeeded)
                return await Result<UserFullResponse>.FailAsync(foundUser.Messages);

            if (foundUser.Data is null)
                return await Result<UserFullResponse>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);

            return await Result<UserFullResponse>.SuccessAsync(foundUser.Data.ToResponse());
        }
        catch (Exception ex)
        {
            return await Result<UserFullResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get a user by username
    /// </summary>
    /// <param name="username">Username of the user</param>
    /// <param name="userService"></param>
    /// <returns>Details of the specified user</returns>
    [Authorize(Policy = PermissionConstants.Users.View)]
    private static async Task<IResult<UserBasicResponse>> GetUserByUsername([FromQuery]string username, IAppUserService userService)
    {
        try
        {
            var foundUser = await userService.GetByUsernameAsync(username);
            if (!foundUser.Succeeded)
                return await Result<UserBasicResponse>.FailAsync(foundUser.Messages);

            if (foundUser.Data is null)
                return await Result<UserBasicResponse>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);

            return await Result<UserBasicResponse>.SuccessAsync(foundUser.Data.ToResponse());
        }
        catch (Exception ex)
        {
            return await Result<UserBasicResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get full user detail for a user including permissions and extended attributes
    /// </summary>
    /// <param name="username">Username of the user</param>
    /// <param name="userService"></param>
    /// <returns>Detail for the specified user including permissions and extended attributes</returns>
    [Authorize(Policy = PermissionConstants.Users.View)]
    private static async Task<IResult<UserFullResponse>> GetFullUserByUsername([FromQuery]string username, IAppUserService userService)
    {
        try
        {
            var foundUser = await userService.GetByUsernameFullAsync(username);
            if (!foundUser.Succeeded)
                return await Result<UserFullResponse>.FailAsync(foundUser.Messages);

            if (foundUser.Data is null)
                return await Result<UserFullResponse>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);

            return await Result<UserFullResponse>.SuccessAsync(foundUser.Data.ToResponse());
        }
        catch (Exception ex)
        {
            return await Result<UserFullResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a user account, bypassing registration
    /// </summary>
    /// <param name="userRequest">Details used to create the user</param>
    /// <param name="userService"></param>
    /// <param name="accountService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>GUID ID of the newly created user account</returns>
    [Authorize(Policy = PermissionConstants.Users.Create)]
    private static async Task<IResult<Guid>> CreateUser([FromBody]UserCreateRequest userRequest, IAppUserService userService, IAppAccountService 
    accountService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetCurrentUserId();
            
            var createRequest = userRequest.ToCreateObject();

            return await userService.CreateAsync(createRequest, currentUserId.GetFromNullable());
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Update properties of the specified user account
    /// </summary>
    /// <param name="userRequest">Details used to update a user account, any empty properties won't be updated</param>
    /// <param name="userService"></param>
    /// <param name="currentUserService"></param>
    /// <returns></returns>
    [Authorize(Policy = PermissionConstants.Users.Edit)]
    private static async Task<IResult> UpdateUser(UserUpdateRequest userRequest, IAppUserService userService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetCurrentUserId();

            var updateRequest = await userService.UpdateAsync(userRequest.ToUpdate(), currentUserId.GetFromNullable());
            if (!updateRequest.Succeeded) return updateRequest;
            return await Result.SuccessAsync("Successfully updated user!");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete the specified user account
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="userService"></param>
    /// <param name="currentUserService"></param>
    /// <returns></returns>
    [Authorize(Policy = PermissionConstants.Users.Delete)]
    private static async Task<IResult> DeleteUser(Guid userId, IAppUserService userService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetCurrentUserId();

            var deleteRequest = await userService.DeleteAsync(userId, currentUserId.GetFromNullable());
            if (!deleteRequest.Succeeded) return deleteRequest;
            return await Result.SuccessAsync("Successfully deleted user!");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Reset the password of the specified user account
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="accountService"></param>
    /// <returns></returns>
    /// <remarks>
    /// - User will be forced to re-authenticate after initiating a reset
    /// - Password reset email will be sent to the user's email address
    /// </remarks>
    [Authorize(Policy = PermissionConstants.Users.ResetPassword)]
    private static async Task<IResult> ResetPassword(Guid userId, IAppAccountService accountService)
    {
        try
        {
            var resetRequest = await accountService.ForceUserPasswordReset(userId);
            if (!resetRequest.Succeeded) return resetRequest;
            return await Result.SuccessAsync("Successfully reset password, email has been sent to the user to finish the reset");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Enable the specified user account
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="accountService"></param>
    /// <returns></returns>
    /// <remarks>
    /// - Can also be used to bypass a locked out account timeout
    /// </remarks>
    [Authorize(Policy = PermissionConstants.Users.Enable)]
    private static async Task<IResult> EnableUser([FromQuery]Guid userId, IAppAccountService accountService)
    {
        try
        {
            var enableRequest = await accountService.SetAuthState(userId, AuthState.Enabled);
            if (!enableRequest.Succeeded) return enableRequest;
            return await Result.SuccessAsync("Successfully enabled the specified user account");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Disable the specified user account
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="accountService"></param>
    /// <returns></returns>
    [Authorize(Policy = PermissionConstants.Users.Disable)]
    private static async Task<IResult> DisableUser([FromQuery]Guid userId, IAppAccountService accountService)
    {
        try
        {
            var disableRequest = await accountService.SetAuthState(userId, AuthState.Disabled);
            if (!disableRequest.Succeeded) return disableRequest;
            return await Result.SuccessAsync("Successfully disabled the specified user account");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
}