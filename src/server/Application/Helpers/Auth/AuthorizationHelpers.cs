using System.Security.Claims;
using Application.Auth.Default;
using Application.Auth.Dynamic;
using Application.Auth.DynamicGlobal;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Identity;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Enums.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Helpers.Auth;

public static class AuthorizationHelpers
{
    /// <summary>
    /// Check whether a claims principal has a specific permission
    /// </summary>
    /// <param name="authService"></param>
    /// <param name="currentUser"></param>
    /// <param name="permission"></param>
    /// <returns>Boolean indicating whether the principal has the provided permission</returns>
    public static async Task<bool> UserHasPermission(this IAuthorizationService authService, ClaimsPrincipal? currentUser, string permission)
    {
        return (await authService.AuthorizeAsync(currentUser!, null, permission)).Succeeded;
    }

    /// <summary>
    /// Check whether a claims principal has a specific dynamic permission
    /// </summary>
    /// <param name="authService"></param>
    /// <param name="currentUser"></param>
    /// <param name="group"></param>
    /// <param name="level"></param>
    /// <param name="entityId"></param>
    /// <returns>Boolean indicating whether the principal has the provided dynamic permission</returns>
    public static async Task<bool> UserHasDynamicPermission(this IAuthorizationService authService, ClaimsPrincipal? currentUser, DynamicPermissionGroup group,
        DynamicPermissionLevel level, Guid entityId)
    {
        return (await authService.AuthorizeAsync(currentUser!, null, new List<IAuthorizationRequirement>
        {
            new DynamicAuthorization { EntityId = entityId, Group = group, Level = level }
        })).Succeeded;
    }

    public static async Task<bool> UserHasGlobalOrDynamicPermission(this IAuthorizationService authService, ClaimsPrincipal? currentUser, string permission,
        DynamicPermissionGroup group, DynamicPermissionLevel level, Guid entityId)
    {
        return (await authService.AuthorizeAsync(currentUser!, null, new List<IAuthorizationRequirement>
        {
            new DynamicGlobalAuthorization(permission, entityId, group, level)
        })).Succeeded;
    }

    /// <summary>
    /// Get the login redirect URL for a given redirect reason
    /// </summary>
    /// <param name="appConfig"></param>
    /// <param name="redirectReason"></param>
    /// <returns>Full login redirect URL</returns>
    public static string GetLoginRedirect(this AppConfiguration appConfig, LoginRedirectReason redirectReason)
    {
        var loginUriBase = new Uri(string.Concat(appConfig.BaseUrl, AppRouteConstants.Identity.Login));
        return QueryHelpers.AddQueryString(loginUriBase.ToString(), LoginRedirectConstants.RedirectParameter, redirectReason.ToString());
    }

    /// <summary>
    /// Logout a user by ID and redirect with the proper notification reason, will also fail the current context for reliability
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="accountService"></param>
    /// <param name="navigationManager"></param>
    public static async Task LogoutUserWithRedirect(this IAppAccountService accountService, Guid userId, NavigationManager navigationManager)
    {
        await accountService.LogoutGuiAsync(userId);
        var currentAuthState = await accountService.GetCurrentAuthState(userId);
        var redirectReason = currentAuthState.Data switch
        {
            AuthState.LoginRequired => LoginRedirectReason.ReAuthenticationForce,
            AuthState.LockedOut => LoginRedirectReason.LockedOut,
            AuthState.Disabled => LoginRedirectReason.Disabled,
            _ => LoginRedirectReason.SessionExpired
        };
        var loginUriFull = QueryHelpers.AddQueryString(AppRouteConstants.Identity.Login, LoginRedirectConstants.RedirectParameter, redirectReason.ToString());
        navigationManager.NavigateTo(loginUriFull, true);
        // navigationManager.NavigateTo(appSettings.GetLoginRedirect(redirectReason), true);
    }

    /// <summary>
    /// Validate the context identity has permission then succeed or fail the context
    /// </summary>
    /// <param name="context"></param>
    /// <param name="authorization"></param>
    /// <returns>Boolean indicating whether the identity has permission</returns>
    public static bool ValidateHasPermission(AuthorizationHandlerContext context, PermissionAuthorization authorization)
    {
        // If active session is valid and not expired validate permissions via claims
        var permissions = context.User.Claims.Where(x =>
            x.Type == ClaimConstants.Permission && x.Value == authorization.Permission);
        if (permissions.Any())
        {
            context.Succeed(authorization);
            return true;
        }

        // Default explicit permission validation fail
        context.Fail();
        return false;
    }

    /// <summary>
    /// Attempt to reauthenticate the context identity using the JWT refresh token or logout the identity if required
    /// </summary>
    /// <param name="accountService"></param>
    /// <param name="navigationManager"></param>
    /// <param name="userId"></param>
    /// <returns>Boolean indicating whether the re-authentication was successful</returns>
    public static async Task<bool> AttemptReAuthentication(this IAppAccountService accountService, NavigationManager navigationManager, Guid? userId = null)
    {
        var response = await accountService.ReAuthUsingRefreshTokenAsync();
        if (response.Succeeded)
        {
            return true;
        }

        // Using refresh token failed, user must do a fresh login
        await accountService.LogoutUserWithRedirect(userId ?? Guid.Empty, navigationManager);
        return false;
    }

    /// <summary>
    /// Validate context identity authentication for expiration, API, forced re-authentication or missing permissions, then fail the context and force a logout if not valid
    /// </summary>
    /// <param name="context"></param>
    /// <param name="accountService"></param>
    /// <param name="navigationManager"></param>
    /// <returns>Boolean indicating whether the context identity is authenticated</returns>
    public static async Task<bool> VerifyAuthenticationIsValid(this AuthorizationHandlerContext context, IAppAccountService accountService, NavigationManager navigationManager)
    {
        var userId = context.User.Claims.GetId();
        if (context.User.Identity is null || context.User == UserConstants.UnauthenticatedPrincipal)
        {
            context.Fail();
            return await Task.FromResult(false);
        }

        // User has an expired session, let's try re-authenticating them using the refresh token
        if (context.User == UserConstants.ExpiredPrincipal)
        {
            var reAuthenticationSuccess = await accountService.AttemptReAuthentication(navigationManager, userId);
            if (!reAuthenticationSuccess)
            {
                context.Fail();
                await accountService.LogoutUserWithRedirect(userId, navigationManager);
                return await Task.FromResult(false);
            }
            // Re-authentication using the refresh token succeeded so we'll continue w/ the authentication validation in case a full re-auth is required
        }

        // Validate host and api authentications to short-circuit more decision-making if the auth is short-lived and wouldn't have local storage
        if (context.User.Claims.IsHostOrApiAuthenticated())
        {
            return await Task.FromResult(true);
        }

        // Validate if user is required to do a full re-authentication
        var userRequiredToFullAuth = (await accountService.IsRequiredToDoFullReAuthentication(userId)).Data;
        if (userRequiredToFullAuth)
        {
            await accountService.LogoutUserWithRedirect(userId, navigationManager);
            return await Task.FromResult(false);
        }

        // Validate or re-authenticate active session based on token expiration, this can happen if the token hasn't been validated recently
        var sessionNeedsReAuthenticated = (await accountService.DoesCurrentSessionNeedReAuthenticated()).Data;
        if (!sessionNeedsReAuthenticated) return await Task.FromResult(true);
        {
            var reAuthenticationSuccess = await accountService.AttemptReAuthentication(navigationManager, userId);
            if (reAuthenticationSuccess) return await Task.FromResult(true);
            context.Fail();
            await accountService.LogoutUserWithRedirect(userId, navigationManager);
            return await Task.FromResult(false);
        }
    }
}