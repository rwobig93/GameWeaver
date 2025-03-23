using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Runtime;
using Application.Helpers.Web;
using Application.Mappers.Identity;
using Application.Requests.Identity.Permission;
using Application.Responses.v1.Identity;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.Identity;

/// <summary>
/// API endpoints for application permissions
/// </summary>
public static class PermissionEndpoints
{
    /// <summary>
    /// Register the API endpoints
    /// </summary>
    /// <param name="app"></param>
    public static void MapEndpointsPermissions(this IEndpointRouteBuilder app)
    {
        // Permissions
        app.MapGet(ApiRouteConstants.Identity.Permission.GetAll, GetAllPermissions).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.Permission.GetById, GetPermission).ApiVersionOne();
        
        // Users
        app.MapGet(ApiRouteConstants.Identity.Permission.GetDirectPermissionsForUser, GetDirectPermissionsForUser).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.Permission.GetAllPermissionsForUser, GetAllPermissionsForUser).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.Permission.AddPermissionToUser, AddPermissionToUser).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.Permission.RemovePermissionFromUser, RemovePermissionFromUser).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.Permission.DoesUserHavePermission, DoesUserHavePermission).ApiVersionOne();
        
        // Roles
        app.MapGet(ApiRouteConstants.Identity.Permission.GetAllPermissionsForRole, GetAllPermissionsForRole).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.Permission.AddPermissionToRole, AddPermissionToRole).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.Permission.RemovePermissionFromRole, RemovePermissionFromRole).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.Permission.DoesRoleHavePermission, DoesRoleHavePermission).ApiVersionOne();
    }

    /// <summary>
    /// Get all application permissions for roles and users
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="permissionService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of application permissions for both roles and users</returns>
    [Authorize(PermissionConstants.Identity.Permissions.View)]
    private static async Task<IResult<IEnumerable<PermissionResponse>>> GetAllPermissions([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IAppPermissionService permissionService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;

            var result = await permissionService.GetAllAssignedPaginatedAsync(pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<PermissionResponse>>.FailAsync(result.Messages);
            }
            
            var convertedResult = await PaginatedResult<IEnumerable<PermissionResponse>>.SuccessAsync(
                result.Data.ToResponses(),
                result.StartPage,
                result.CurrentPage,
                result.EndPage,
                result.TotalCount,
                result.PageSize);

            if (convertedResult.TotalCount <= 0) return convertedResult;

            convertedResult.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.Identity.Permission.GetAll, pageNumber, pageSize);
            convertedResult.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.Identity.Permission.GetAll, pageNumber, pageSize, convertedResult.TotalCount);
            return convertedResult;
        }
        catch (Exception ex)
        {
            return await Result<List<PermissionResponse>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get a specific permission
    /// </summary>
    /// <param name="permissionId">GUID ID of the desired permission to retrieve</param>
    /// <param name="permissionService"></param>
    /// <returns>Permission object for a user or role</returns>
    [Authorize(PermissionConstants.Identity.Permissions.View)]
    private static async Task<IResult<PermissionResponse>> GetPermission([FromQuery]Guid permissionId, IAppPermissionService permissionService)
    {
        try
        {
            var foundPermission = await permissionService.GetByIdAsync(permissionId);
            if (!foundPermission.Succeeded)
                return await Result<PermissionResponse>.FailAsync(foundPermission.Messages);

            if (foundPermission.Data is null)
                    return await Result<PermissionResponse>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);

            return await Result<PermissionResponse>.SuccessAsync(foundPermission.Data.ToResponse());
        }
        catch (Exception ex)
        {
            return await Result<PermissionResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Add the specified permission to the specified role
    /// </summary>
    /// <param name="permissionRequest">Detail used to map a permission to a role</param>
    /// <param name="permissionService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Guid ID of the permission added</returns>
    [Authorize(PermissionConstants.Identity.Permissions.Add)]
    private static async Task<IResult<Guid>> AddPermissionToRole(PermissionCreateForRoleRequest permissionRequest, IAppPermissionService 
    permissionService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = (await currentUserService.GetCurrentUserId()).GetFromNullable();
            var addRequest = permissionRequest.ToCreate();
            
            return await permissionService.CreateAsync(addRequest, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Add the specified permission to the specified user
    /// </summary>
    /// <param name="permissionRequest">Detail used to map a permission to a user</param>
    /// <param name="permissionService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>GUID ID of the permission added</returns>
    [Authorize(PermissionConstants.Identity.Permissions.Add)]
    private static async Task<IResult<Guid>> AddPermissionToUser(PermissionCreateForUserRequest permissionRequest, IAppPermissionService 
    permissionService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = (await currentUserService.GetCurrentUserId()).GetFromNullable();
            var addRequest = permissionRequest.ToCreate();
            
            return await permissionService.CreateAsync(addRequest, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get whether a user has a specific permission
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="permissionId">GUID ID of the permission</param>
    /// <param name="permissionService"></param>
    /// <returns>Boolean indicating whether the specified user has the specified permission</returns>
    [Authorize(PermissionConstants.Identity.Permissions.View)]
    private static async Task<IResult<bool>> DoesUserHavePermission([FromQuery]Guid userId, [FromQuery]Guid permissionId, 
    IAppPermissionService permissionService)
    {
        try
        {
            var foundPermission = await permissionService.GetByIdAsync(permissionId);
            if (!foundPermission.Succeeded) return await Result<bool>.FailAsync(foundPermission.Messages);
            if (foundPermission.Data is null) return await Result<bool>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);
            
            return await permissionService.UserIncludingRolesHasPermission(userId, foundPermission.Data!.ClaimValue);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get whether a role has a specific permission
    /// </summary>
    /// <param name="roleId">GUID ID of the role</param>
    /// <param name="permissionId">GUID ID of the permission</param>
    /// <param name="permissionService"></param>
    /// <returns>Boolean indicating whether the specified role has the specified permission</returns>
    [Authorize(PermissionConstants.Identity.Permissions.View)]
    private static async Task<IResult<bool>> DoesRoleHavePermission([FromQuery]Guid roleId, [FromQuery]Guid permissionId, 
        IAppPermissionService permissionService)
    {
        try
        {
            var foundPermission = await permissionService.GetByIdAsync(permissionId);
            if (!foundPermission.Succeeded) return await Result<bool>.FailAsync(foundPermission.Messages);
            if (foundPermission.Data is null) return await Result<bool>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);
            
            return await permissionService.RoleHasPermission(roleId, foundPermission.Data!.ClaimValue);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Remove the specified permission from the specified user
    /// </summary>
    /// <param name="permissionRequest">Detail used to remove a permission from a user</param>
    /// <param name="permissionService"></param>
    /// <returns></returns>
    [Authorize(PermissionConstants.Identity.Permissions.Remove)]
    private static async Task<IResult> RemovePermissionFromUser(PermissionRemoveFromUserRequest permissionRequest,
        IAppPermissionService permissionService)
    {
        try
        {
            var foundPermission =
                await permissionService.GetByUserIdAndValueAsync(permissionRequest.UserId, permissionRequest.PermissionValue);
            if (!foundPermission.Succeeded) return await Result.FailAsync(foundPermission.Messages);
            if (foundPermission.Data is null) return await Result.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);
            
            var permissionResponse = await permissionService.DeleteAsync(foundPermission.Data.Id, Guid.Empty);
            if (!permissionResponse.Succeeded) return permissionResponse;
            
            return await Result.SuccessAsync("Successfully removed permission from user!");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Remove the specified permission from the specified role 
    /// </summary>
    /// <param name="permissionRequest">Detail used to remove a permission from a role</param>
    /// <param name="permissionService"></param>
    /// <returns></returns>
    [Authorize(PermissionConstants.Identity.Permissions.Remove)]
    private static async Task<IResult> RemovePermissionFromRole(PermissionRemoveFromRoleRequest permissionRequest,
        IAppPermissionService permissionService)
    {
        try
        {
            var foundPermission =
                await permissionService.GetByUserIdAndValueAsync(permissionRequest.RoleId, permissionRequest.PermissionValue);
            if (!foundPermission.Succeeded) return await Result.FailAsync(foundPermission.Messages);
            if (foundPermission.Data is null) return await Result.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);
            
            var permissionResponse = await permissionService.DeleteAsync(foundPermission.Data.Id, Guid.Empty);
            if (!permissionResponse.Succeeded) return permissionResponse;

            return await Result.SuccessAsync("Successfully removed permission from role!");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get only permissions assigned directly to the specified user
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="permissionService"></param>
    /// <returns>List of directly assigned permissions to the specified user</returns>
    [Authorize(PermissionConstants.Identity.Permissions.View)]
    private static async Task<IResult<List<PermissionResponse>>> GetDirectPermissionsForUser([FromQuery]Guid userId,
        IAppPermissionService permissionService)
    {
        try
        {
            var foundPermissions = await permissionService.GetAllDirectForUserAsync(userId);
            if (!foundPermissions.Succeeded)
                return await Result<List<PermissionResponse>>.FailAsync(foundPermissions.Messages);

            return await Result<List<PermissionResponse>>.SuccessAsync(foundPermissions.Data.ToResponses());
        }
        catch (Exception ex)
        {
            return await Result<List<PermissionResponse>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get all permissions for a user, including those inherited from assigned roles
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="permissionService"></param>
    /// <returns>List of all permissions for a user, including those inherited from roles</returns>
    [Authorize(PermissionConstants.Identity.Permissions.View)]
    private static async Task<IResult<List<PermissionResponse>>> GetAllPermissionsForUser([FromQuery]Guid userId,
        IAppPermissionService permissionService)
    {
        try
        {
            var foundPermissions = await permissionService.GetAllIncludingRolesForUserAsync(userId);
            if (!foundPermissions.Succeeded)
                return await Result<List<PermissionResponse>>.FailAsync(foundPermissions.Messages);

            return await Result<List<PermissionResponse>>.SuccessAsync(foundPermissions.Data.ToResponses());
        }
        catch (Exception ex)
        {
            return await Result<List<PermissionResponse>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get all permissions assigned to the specified role
    /// </summary>
    /// <param name="roleId">GUID ID of the role</param>
    /// <param name="permissionService"></param>
    /// <returns>List of permissions assigned to the specified role</returns>
    [Authorize(PermissionConstants.Identity.Permissions.View)]
    private static async Task<IResult<List<PermissionResponse>>> GetAllPermissionsForRole([FromQuery]Guid roleId,
        IAppPermissionService permissionService)
    {
        try
        {
            var foundPermissions = await permissionService.GetAllForRoleAsync(roleId);
            if (!foundPermissions.Succeeded)
                return await Result<List<PermissionResponse>>.FailAsync(foundPermissions.Messages);

            return await Result<List<PermissionResponse>>.SuccessAsync(foundPermissions.Data.ToResponses());
        }
        catch (Exception ex)
        {
            return await Result<List<PermissionResponse>>.FailAsync(ex.Message);
        }
    }
}