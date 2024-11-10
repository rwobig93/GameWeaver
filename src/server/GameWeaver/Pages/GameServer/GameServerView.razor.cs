using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.GameServer;
using Application.Services.GameServer;
using Domain.Enums.GameServer;
using Infrastructure.Services.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class GameServerView : ComponentBase
{
    [Parameter] public Guid GameId { get; set; } = Guid.Empty;

    [Inject] public IGameServerService GameServerService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    private bool _validIdProvided = true;
    private Guid _loggedInUserId = Guid.Empty;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private GameServerSlim _gameServer = new() { Id = Guid.Empty };
    private bool _editMode;
    private string _editButtonText = "Enable Edit Mode";

    private bool _canEditGameServer;
    private bool _canDeleteGameServer;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await GetPermissions();
                await GetClientTimezone();
                await GetViewingGameServer();
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

    private async Task GetViewingGameServer()
    {
        var response = await GameServerService.GetByIdAsync(GameId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (response.Data is null)
        {
            Snackbar.Add(ErrorMessageConstants.GameServers.NotFound);
            _validIdProvided = false;
            return;
        }

        _gameServer = response.Data;
        
        if (_gameServer.Id == Guid.Empty)
        {
            _validIdProvided = false;
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canEditGameServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Update);
        _canDeleteGameServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Delete);
    }
    
    private async Task Save()
    {
        if (!_canEditGameServer)
        {
            return;
        }
        
        var response = await GameServerService.UpdateAsync(_gameServer.ToUpdate(), _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        ToggleEditMode();
        await GetViewingGameServer();
        Snackbar.Add("Gameserver successfully updated!", Severity.Success);
        StateHasChanged();
    }

    private void ToggleEditMode()
    {
        _editMode = !_editMode;

        _editButtonText = _editMode ? "Disable Edit Mode" : "Enable Edit Mode";
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewAll);
    }

    private async Task DeleteGameServer()
    {
        if (!_canDeleteGameServer)
        {
            Snackbar.Add(ErrorMessageConstants.Permissions.PermissionError, Severity.Error);
            return;
        }
        
        var dialogParameters = new DialogParameters()
        {
            {"Title", "Are you sure you want to delete this gameserver?"},
            {"Content", $"Server Name: {_gameServer.ServerName}"}
        };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };

        var dialogResponse = await DialogService.Show<ConfirmationDialog>("Delete Gameserver", dialogParameters, dialogOptions).Result;
        if (dialogResponse?.Data is null || dialogResponse.Canceled)
        {
            return;
        }

        var response = await GameServerService.DeleteAsync(_gameServer.Id, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        Snackbar.Add("Gameserver successfully deleted!", Severity.Success);
        GoBack();
    }
}