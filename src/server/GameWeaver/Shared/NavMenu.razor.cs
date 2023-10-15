using System.Security.Claims;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Services.Identity;
using Domain.Models.Identity;
using Microsoft.AspNetCore.Components;

namespace GameWeaver.Shared;

public partial class NavMenu
{
    private bool _canViewApi;
    private bool _canViewJobs;
    private bool _canViewCounter;
    private bool _canViewWeather;
    private bool _canViewUsers;
    private bool _canViewRoles;
    private bool _canViewAuditTrails;
    private bool _isDeveloper;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = await CurrentUserService.GetCurrentUserPrincipal();
        _canViewApi = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Api.View);
        _canViewJobs = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Jobs.View);
        _canViewCounter = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Example.Counter);
        _canViewWeather = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Example.Weather);
        _canViewUsers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Users.View);
        _canViewRoles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Roles.View);
        _canViewAuditTrails = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Audit.View);
        _isDeveloper = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Developer.Dev);
    }
}