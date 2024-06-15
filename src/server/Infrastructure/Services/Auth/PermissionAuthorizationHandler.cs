using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Identity;
using Application.Helpers.Runtime;
using Application.Models.Identity.Permission;
using Application.Services.Identity;
using Application.Settings;
using Application.Settings.AppSettings;
using Domain.Enums.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAppAccountService _accountService;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger _logger;
    private readonly IOptions<AppConfiguration> _appSettings;
    
    public PermissionAuthorizationHandler(IAppAccountService accountService, NavigationManager navigationManager, ILogger logger, IOptions<AppConfiguration> appSettings)
    {
        _accountService = accountService;
        _navigationManager = navigationManager;
        _logger = logger;
        _appSettings = appSettings;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // If there are no claims the user isn't authenticated as we always have at least a NameIdentifier in our generated JWT
        if (context.User == UserConstants.UnauthenticatedPrincipal)
        {
            context.Fail();
            return;
        }
        
        // Validate if user is required to do a full re-authentication
        var userId = context.User.Claims.GetId();
        var userRequiredToFullAuth = (await _accountService.IsRequiredToDoFullReAuthentication(userId)).Data;
        if (userRequiredToFullAuth)
        {
            await _accountService.LogoutGuiAsync(userId);
            context.Fail();
            var currentAuthState = await _accountService.GetCurrentAuthState(userId);
            var redirectReason = currentAuthState.Data switch
            {
                AuthState.LoginRequired => LoginRedirectReason.SessionExpired,
                _ => LoginRedirectReason.FullLoginTimeout
            };
            _navigationManager.NavigateTo(_appSettings.Value.GetLoginRedirect(redirectReason), true);
            return;
        }

        // User has an expired session, let's try re-authenticating them using the refresh token
        if (context.User == UserConstants.ExpiredPrincipal)
        {
            var reAuthenticationSuccess = await AttemptReAuthentication();
            if (!reAuthenticationSuccess)
            {
                context.Fail();
                await Task.CompletedTask;
                _navigationManager.NavigateTo(_appSettings.Value.GetLoginRedirect(LoginRedirectReason.SessionExpired), true);
                return;
            }
            
            _navigationManager.NavigateTo(_navigationManager.Uri, true);
            context.Fail();
            return;
        }
        
        // Validate or re-authenticate active session based on token expiration, this can happen if the token hasn't been validated recently
        var sessionNeedsReAuthenticated = (await _accountService.DoesCurrentSessionNeedReAuthenticated()).Data;
        if (sessionNeedsReAuthenticated)
        {
            var reAuthenticationSuccess = await AttemptReAuthentication();
            if (!reAuthenticationSuccess)
            {
                context.Fail();
                await Task.CompletedTask;
                _navigationManager.NavigateTo(_appSettings.Value.GetLoginRedirect(LoginRedirectReason.SessionExpired), true);
                return;
            }
            
            _navigationManager.NavigateTo(_navigationManager.Uri, true);
            context.Fail();
            return;
        }

        if (!context.User.Claims.Any())
        {
            _logger.Warning("User doesn't have any claims during authorization checks: [id]{UserId}", userId);
            context.Fail();
            return;
        }
        
        // If active session is valid and not expired validate permissions via claims
        var permissions = context.User.Claims.Where(x =>
            x.Type == ApplicationClaimTypes.Permission && x.Value == requirement.Permission);
        if (permissions.Any())
        {
            context.Succeed(requirement);
            await Task.CompletedTask;
            return;
        }
        
        // Default explicit permission validation fail
        context.Fail();
    }

    private async Task<bool> AttemptReAuthentication()
    {
        var response = await _accountService.ReAuthUsingRefreshTokenAsync();
        if (response.Succeeded) return true;
        
        // Using refresh token failed, user must do a fresh login
        await LogoutAndClearCache();
        return false;
    }

    private async Task LogoutAndClearCache()
    {
        await _accountService.LogoutGuiAsync(Guid.Empty);
        var loginUriFull = QueryHelpers.AddQueryString(
            AppRouteConstants.Identity.Login, LoginRedirectConstants.RedirectParameter, nameof(LoginRedirectReason.SessionExpired));
        
        _navigationManager.NavigateTo(loginUriFull, true);
    }
}
