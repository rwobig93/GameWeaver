using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.ConfigResourceTreeItem;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Services.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class GameServerView : ComponentBase
{
    [Parameter] public Guid GameServerId { get; set; } = Guid.Empty;

    [Inject] public IGameServerService GameServerService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    private bool _validIdProvided = true;
    private Guid _loggedInUserId = Guid.Empty;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private GameServerSlim _gameServer = new() { Id = Guid.Empty };
    private List<LocalResourceSlim> _localResources = [];
    private List<TreeItemData<ConfigResourceTreeItem>> _localResourceTreeData = [];
    private bool _editMode;
    private string _editButtonText = "Enable Edit Mode";
    private string _searchText = string.Empty;
    private readonly List<MudTreeView<ConfigResourceTreeItem>> _resourceTreeViews = [];
    public MudTreeView<ConfigResourceTreeItem> ResourceTreeView
    {
        set => _resourceTreeViews.Add(value);
    }

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
                await GetGameServerResources();
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
        var response = await GameServerService.GetByIdAsync(GameServerId);
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

    private async Task GetGameServerResources()
    {
        if (!_validIdProvided)
        {
            return;
        }
        
        var response = await GameServerService.GetLocalResourcesForGameServerIdAsync(_gameServer.Id);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        _localResources = response.Data.ToList();
        foreach (var resource in _localResources)
        {
            var resourceTreeItem = new TreeItemData<ConfigResourceTreeItem>
            {
                Children = [], Expanded = false, Expandable = true, Icon = Icons.Material.Outlined.InsertDriveFile, Text = resource.Name,
                Visible = true, Selected = false, Value = resource.ToTreeItem()
            };

            foreach (var config in resource.ConfigSets)
            {
                resourceTreeItem.Children.Add(new TreeItemData<ConfigResourceTreeItem>()
                {
                    Children = [], Expanded = false, Expandable = true, Icon = Icons.Material.Outlined.DriveFileRenameOutline, Text = config.FriendlyName,
                    Visible = true, Selected = false, Value = config.ToTreeItem()
                });
            }
            
            _localResourceTreeData.Add(resourceTreeItem);
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

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Delete Gameserver", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
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

    private async Task SearchChanged()
    {
        foreach (var treeView in _resourceTreeViews)
        {
            await treeView.FilterAsync();
        }
    }

    private Task<bool> ConfigurationFilter(TreeItemData<ConfigResourceTreeItem> item)
    {
        if (item.Value is null)
        {
            return Task.FromResult(false);
        }
        
        if (string.IsNullOrEmpty(item.Value?.Name) && string.IsNullOrEmpty(item.Value?.Value))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(
            item.Value!.Key.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
            item.Value!.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
            item.Value!.Value.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
            );
    }
}