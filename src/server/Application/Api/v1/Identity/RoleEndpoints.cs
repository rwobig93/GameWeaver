using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Runtime;
using Application.Helpers.Web;
using Application.Mappers.Identity;
using Application.Requests.Identity.Role;
using Application.Responses.v1.Identity;
using Application.Services.Identity;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.v1.Identity;

/// <summary>
/// API endpoints for application roles
/// </summary>
public static class RoleEndpoints
{
    /// <summary>
    /// Register role API endpoints
    /// </summary>
    /// <param name="app"></param>
    public static void MapEndpointsRoles(this IEndpointRouteBuilder app)
    {
        // Roles
        app.MapGet(ApiRouteConstants.Identity.Role.GetAll, GetAllRoles).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.Role.GetById, GetById).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.Identity.Role.Delete, DeleteRole).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.Role.Create, CreateRole).ApiVersionOne();
        app.MapPut(ApiRouteConstants.Identity.Role.Update, UpdateRole).ApiVersionOne();
        
        // Users
        app.MapGet(ApiRouteConstants.Identity.Role.GetRolesForUser, GetRolesForUser).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Identity.Role.IsUserInRole, IsUserInRole).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.Role.AddUserToRole, AddUserToRole).ApiVersionOne();
        app.MapPost(ApiRouteConstants.Identity.Role.RemoveUserFromRole, RemoveUserFromRole).ApiVersionOne();
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    /// <param name="roleService"></param>
    /// <returns>List of all roles</returns>
    [Authorize(Policy = PermissionConstants.Roles.View)]
    private static async Task<IResult<List<RoleResponse>>> GetAllRoles(IAppRoleService roleService)
    {
        try
        {
            var allRoles = await roleService.GetAllAsync();
            if (!allRoles.Succeeded)
                return await Result<List<RoleResponse>>.FailAsync(allRoles.Messages);

            return await Result<List<RoleResponse>>.SuccessAsync(allRoles.Data.ToResponses());
        }
        catch (Exception ex)
        {
            return await Result<List<RoleResponse>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get the specified role
    /// </summary>
    /// <param name="roleId">GUID ID of the role</param>
    /// <param name="roleService"></param>
    /// <returns>Detail regarding the specified role</returns>
    [Authorize(Policy = PermissionConstants.Roles.View)]
    private static async Task<IResult<RoleResponse>> GetById([FromQuery]Guid roleId, IAppRoleService roleService)
    {
        try
        {
            var role = await roleService.GetByIdAsync(roleId);
            if (!role.Succeeded)
                return await Result<RoleResponse>.FailAsync(role.Messages);

            if (role.Data is null)
                return await Result<RoleResponse>.FailAsync(ErrorMessageConstants.Generic.InvalidValueError);

            return await Result<RoleResponse>.SuccessAsync(role.Data.ToResponse());
        }
        catch (Exception ex)
        {
            return await Result<RoleResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a role
    /// </summary>
    /// <param name="roleRequest">Detail used to create a role</param>
    /// <param name="roleService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>GUID ID of the newly created role</returns>
    [Authorize(Policy = PermissionConstants.Roles.Create)]
    private static async Task<IResult<Guid>> CreateRole(CreateRoleRequest roleRequest, IAppRoleService roleService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = (await currentUserService.GetCurrentUserId()).GetFromNullable();
            var createRequest = roleRequest.ToCreateObject();

            return await roleService.CreateAsync(createRequest, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Update a role's properties
    /// </summary>
    /// <param name="roleRequest">Detail used to update a role, any properties left empty will not be updated</param>
    /// <param name="roleService"></param>
    /// <param name="currentUserService"></param>
    /// <returns></returns>
    [Authorize(Policy = PermissionConstants.Roles.Edit)]
    private static async Task<IResult> UpdateRole(UpdateRoleRequest roleRequest, IAppRoleService roleService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = (await currentUserService.GetCurrentUserId()).GetFromNullable();
            var updateRequest = await roleService.UpdateAsync(roleRequest.ToUpdate(), currentUserId);
            if (!updateRequest.Succeeded) return updateRequest;
            return await Result.SuccessAsync("Successfully updated role!");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete the specified role
    /// </summary>
    /// <param name="roleId">GUID ID of the role</param>
    /// <param name="roleService"></param>
    /// <param name="currentUserService"></param>
    /// <returns></returns>
    [Authorize(Policy = PermissionConstants.Roles.Delete)]
    private static async Task<IResult> DeleteRole(Guid roleId, IAppRoleService roleService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = (await currentUserService.GetCurrentUserId()).GetFromNullable();
            var deleteRequest = await roleService.DeleteAsync(roleId, currentUserId);
            if (!deleteRequest.Succeeded) return deleteRequest;
            return await Result.SuccessAsync("Successfully deleted role!");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Determine whether the specified user is in the specified role
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="roleId">GUID ID of the role</param>
    /// <param name="roleService"></param>
    /// <returns>Boolean indicating whether the specified user is a member of the specified role</returns>
    [Authorize(Policy = PermissionConstants.Roles.View)]
    private static async Task<IResult<bool>> IsUserInRole([FromQuery]Guid userId, [FromQuery]Guid roleId, IAppRoleService roleService)
    {
        try
        {
            return await roleService.IsUserInRoleAsync(userId, roleId);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Add the specified user to the specified role
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="roleId">GUID ID of the role</param>
    /// <param name="roleService"></param>
    /// <param name="currentUserService"></param>
    /// <returns></returns>
    [Authorize(Policy = PermissionConstants.Roles.Add)]
    private static async Task<IResult> AddUserToRole(Guid userId, Guid roleId, IAppRoleService roleService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = (await currentUserService.GetCurrentUserId()).GetFromNullable();
            var roleResponse = await roleService.AddUserToRoleAsync(userId, roleId, currentUserId);
            if (!roleResponse.Succeeded) return await Result<bool>.FailAsync(roleResponse.Messages);
            
            return await Result.SuccessAsync("Successfully added user to role!");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Remove the specified user from the specified role
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="roleId">GUID ID of the user</param>
    /// <param name="roleService"></param>
    /// <param name="currentUserService"></param>
    /// <returns></returns>
    [Authorize(Policy = PermissionConstants.Roles.Remove)]
    private static async Task<IResult> RemoveUserFromRole(Guid userId, Guid roleId, IAppRoleService roleService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = (await currentUserService.GetCurrentUserId()).GetFromNullable();
            var roleResponse = await roleService.RemoveUserFromRoleAsync(userId, roleId, currentUserId);
            if (!roleResponse.Succeeded) return await Result<bool>.FailAsync(roleResponse.Messages);
            
            return await Result.SuccessAsync("Successfully removed user from role!");
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get all roles for the specified user
    /// </summary>
    /// <param name="userId">GUID ID of the user</param>
    /// <param name="roleService"></param>
    /// <returns>List of roles assigned to the specified user</returns>
    [Authorize(Policy = PermissionConstants.Roles.View)]
    private static async Task<IResult<List<RoleResponse>>> GetRolesForUser([FromQuery]Guid userId, IAppRoleService roleService)
    {
        try
        {
            var roleResponse = await roleService.GetRolesForUser(userId);
            if (!roleResponse.Succeeded) return await Result<List<RoleResponse>>.FailAsync(roleResponse.Messages);
            
            return await Result<List<RoleResponse>>.SuccessAsync(roleResponse.Data.ToResponses());
        }
        catch (Exception ex)
        {
            return await Result<List<RoleResponse>>.FailAsync(ex.Message);
        }
    }
}