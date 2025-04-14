using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Application.Auth.Default;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorization>
{
    private readonly IAppAccountService _accountService;
    private readonly NavigationManager _navigationManager;

    public PermissionAuthorizationHandler(IAppAccountService accountService, NavigationManager navigationManager)
    {
        _accountService = accountService;
        _navigationManager = navigationManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorization authorization)
    {
        var isAuthenticationValid = await context.VerifyAuthenticationIsValid(_accountService, _navigationManager);
        if (!isAuthenticationValid)
        {
            context.Fail();
            return;
        }

        var matchingPermissions = context.User.Claims.Where(x => x.Type == ClaimConstants.Permission && x.Value == authorization.Permission);
        if (matchingPermissions.Any())
        {
            context.Succeed(authorization);
            return;
        }

        // Default explicit permission validation fail
        context.Fail();
    }
}
