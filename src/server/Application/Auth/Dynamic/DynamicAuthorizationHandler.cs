using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Services.Identity;
using Domain.Enums.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Application.Auth.Dynamic;

public class DynamicAuthorizationHandler : AuthorizationHandler<DynamicAuthorization>
{
    private readonly IAppAccountService _accountService;
    private readonly NavigationManager _navigationManager;

    public DynamicAuthorizationHandler(IAppAccountService accountService, NavigationManager navigationManager)
    {
        _accountService = accountService;
        _navigationManager = navigationManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DynamicAuthorization authorization)
    {
        var isAuthenticationValid = await context.VerifyAuthenticationIsValid(_accountService, _navigationManager);
        if (!isAuthenticationValid)
        {
            context.Fail();
            return;
        }

        var dynamicPermissions = context.User.Claims.Where(x => x.Type == ClaimConstants.DynamicPermission).ToArray();

        var adminPermission = $"{ClaimConstants.DynamicPermission}.{authorization.Group}.{authorization.EntityId}.{DynamicPermissionLevel.Admin}";
        var matchingAdminPermissions = dynamicPermissions.Where(x => x.Value == adminPermission);
        if (matchingAdminPermissions.Any())
        {
            context.Succeed(authorization);
            return;
        }

        if (authorization.Level is DynamicPermissionLevel.Admin) // We are checking for admin, we got past the above check which means we don't have admin
        {
            context.Fail();
            return;
        }

        var permission = $"{ClaimConstants.DynamicPermission}.{authorization.Group}.{authorization.EntityId}.{authorization.Level}";
        var matchingPermissions = dynamicPermissions.Where(x => x.Value == permission);
        if (matchingPermissions.Any())
        {
            context.Succeed(authorization);
            return;
        }

        // Default explicit permission validation fail
        context.Fail();
    }
}