using Application.Constants.Identity;
using Application.Helpers.Identity;
using Application.Repositories.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Application.Auth;

public class DynamicAuthorizationHandler : AuthorizationHandler<DynamicRequirement>
{
    private readonly IAppPermissionRepository _permissionRepository;

    public DynamicAuthorizationHandler(IAppPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DynamicRequirement requirement)
    {
        var userId = context.User.Claims.GetId();
        var permission = $"{ClaimConstants.DynamicPermission}.{requirement.Group}.{requirement.EntityId}.{requirement.Level}";
        var hasPermission = await _permissionRepository.UserIncludingRolesHasPermission(userId, permission);
        if (hasPermission.Result)
        {
            context.Succeed(requirement);
            return;
        }
        
        context.Fail();
    }
}