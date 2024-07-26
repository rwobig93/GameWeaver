using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Host;
using Application.Services.GameServer;
using Infrastructure.Services.GameServer;
using Microsoft.AspNetCore.Components;

namespace GameWeaver.Pages.GameServer;

public partial class HostView : ComponentBase
{
    [Parameter] public Guid HostId { get; set; } = Guid.Empty;

    [Inject] public IHostService HostService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    private bool _validIdProvided = true;
    private Guid _loggedInUserId = Guid.Empty;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private HostSlim _host = new() { Id = Guid.Empty };
    private bool _editMode;
    private string _editButtonText = "Enable Edit Mode";
    private List<GameServerSlim> _runningGameservers = [];

    private bool _canEditHost;
    private bool _canViewGameServers;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await GetClientTimezone();
                await GetViewingHost();
                await GetPermissions();
                StateHasChanged();
            }
        }
        catch
        {
            StateHasChanged();
        }
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
        {
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private async Task GetViewingHost()
    {
        var response = await HostService.GetByIdAsync(HostId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (response.Data is null)
        {
            Snackbar.Add(ErrorMessageConstants.Hosts.NotFound);
            _validIdProvided = false;
            StateHasChanged();
            return;
        }

        _host = response.Data;
        
        if (_host.Id == Guid.Empty)
        {
            _validIdProvided = false;
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canEditHost = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.Update);
        _canViewGameServers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get);
    }
    
    private async Task Save()
    {
        if (!_canEditHost) return;
        
        var response = await HostService.UpdateAsync(_host.ToUpdateRequest(), _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        ToggleEditMode();
        await GetViewingHost();
        Snackbar.Add("Host successfully updated!", Severity.Success);
        StateHasChanged();
    }

    private void ToggleEditMode()
    {
        _editMode = !_editMode;

        _editButtonText = _editMode ? "Disable Edit Mode" : "Enable Edit Mode";
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.Hosts.HostsDashboard);
    }
}