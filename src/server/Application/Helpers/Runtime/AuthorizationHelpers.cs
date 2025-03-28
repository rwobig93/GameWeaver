﻿using System.Security.Claims;
using Application.Auth;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Settings.AppSettings;
using Domain.Enums.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Helpers.Runtime;

public static class AuthorizationHelpers
{
    public static async Task<bool> UserHasPermission(this IAuthorizationService authService, ClaimsPrincipal? currentUser, string permission)
    {
        return (await authService.AuthorizeAsync(currentUser!, null, permission)).Succeeded;
    }
    
    public static async Task<bool> UserHasDynamicPermission(this IAuthorizationService authService, ClaimsPrincipal? currentUser, DynamicPermissionGroup group, 
        DynamicPermissionLevel level, Guid entityId)
    {
        return (await authService.AuthorizeAsync(currentUser!, null, new List<IAuthorizationRequirement>()
        {
            new DynamicRequirement { EntityId = entityId, Group = group, Level = level }
        })).Succeeded;
    }

    public static string GetLoginRedirect(this AppConfiguration appConfig, LoginRedirectReason redirectReason)
    {
        var loginUriBase = new Uri(string.Concat(appConfig.BaseUrl, AppRouteConstants.Identity.Login));
        return QueryHelpers.AddQueryString(loginUriBase.ToString(), LoginRedirectConstants.RedirectParameter, redirectReason.ToString());
    }
}