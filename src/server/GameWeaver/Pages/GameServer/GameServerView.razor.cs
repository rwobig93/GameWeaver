using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.GameServer;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Mappers.Identity;
using Application.Models.Events;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.Identity.Permission;
using Application.Models.Lifecycle;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Enums.Identity;
using GameWeaver.Components.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class GameServerView : ComponentBase, IAsyncDisposable
{
    [Parameter] public Guid GameServerId { get; init; } = Guid.Empty;

    [Inject] public IAppRoleService RoleService { get; init; } = null!;
    [Inject] public IGameServerService GameServerService { get; init; } = null!;
    [Inject] public IGameService GameService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private IEventService EventService { get; init; } = null!;
    [Inject] private INotifyRecordService NotifyRecordService { get; init; } = null!;
    [Inject] private IAppPermissionService PermissionService { get; init; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;

    private bool _validIdProvided = true;
    private Guid _loggedInUserId = Guid.Empty;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private GameServerSlim _gameServer = new() { Id = Guid.Empty };
    private GameProfileSlim? _parentProfile;
    private GameSlim _game = new() { Id = Guid.Empty };
    private List<LocalResourceSlim> _localResources = [];
    private bool _editMode;
    private string _editButtonText = "Enable Edit Mode";
    private string _configSearchText = string.Empty;
    private readonly List<ConfigurationItemSlim> _createdConfigItems = [];
    private readonly List<ConfigurationItemSlim> _updatedConfigItems = [];
    private readonly List<ConfigurationItemSlim> _deletedConfigItems = [];
    private bool _updateIsAvailable;
    private MudTable<NotifyRecordSlim> _notifyTable = new();
    private IEnumerable<NotifyRecordSlim> _notifyPagedData = new List<NotifyRecordSlim>();
    private string _notifySearchText = string.Empty;
    private int _totalNotifyRecords;
    private int _selectedNotifyViewDetail;
    private List<AppPermissionDisplay> _assignedUserPermissions = [];
    private List<AppPermissionDisplay> _assignedRolePermissions = [];
    private HashSet<AppPermissionDisplay> _deleteUserPermissions = [];
    private HashSet<AppPermissionDisplay> _deleteRolePermissions = [];
    private Color _serverStatusColor = Color.Error;

    private bool _canViewGameServer;
    private bool _canPermissionGameServer;
    private bool _canEditGameServer;
    private bool _canConfigureGameServer;
    private bool _canStartGameServer;
    private bool _canStopGameServer;
    private bool _canDeleteGameServer;
    private bool _canChangeOwnership;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await GetViewingGameServer();
                await GetServerParentProfile();
                await GetServerGame();
                await GetPermissions();
                await GetClientTimezone();
                await GetGameServerResources();
                await GetGameServerPermissions();
                
                EventService.GameVersionUpdated += GameVersionUpdated;
                EventService.GameServerStatusChanged += GameServerStatusChanged;
                EventService.NotifyTriggered += NotifyTriggered;
                
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

    private async Task GetServerParentProfile()
    {
        if (_gameServer.ParentGameProfileId is null)
        {
            return;
        }

        var response = await GameServerService.GetGameProfileByIdAsync(_gameServer.ParentGameProfileId.GetFromNullable());
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        _parentProfile = response.Data;
    }

    private async Task GetServerGame()
    {
        var gameResponse = await GameService.GetByIdAsync(_gameServer.GameId);
        if (!gameResponse.Succeeded || gameResponse.Data is null)
        {
            Snackbar.Add("Failed to find game for server, please reach out to an administrator", Severity.Error);
            return;
        }
        
        _game = gameResponse.Data;
        _updateIsAvailable = _gameServer.ServerBuildVersion != _game.LatestBuildVersion;
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
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        
        _canViewGameServer = !_gameServer.Private ||
                             await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get) ||
                             await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Dynamic(_gameServer.Id, DynamicPermissionLevel.View));

        var isServerAdmin = (await RoleService.IsUserAdminAsync(_loggedInUserId)).Data;
        
        // Game server owner and admin gets full permissions
        if (_gameServer.OwnerId == _loggedInUserId || isServerAdmin)
        {
            _canViewGameServer = true;
            _canPermissionGameServer = true;
            _canEditGameServer = true;
            _canConfigureGameServer = true;
            _canStartGameServer = true;
            _canStopGameServer = true;
            _canDeleteGameServer = true;
            _canChangeOwnership = true;
            return;
        }
        
        // Moderator gets most permissions other than delete
        var isServerModerator = (await RoleService.IsUserModeratorAsync(_loggedInUserId)).Data;
        if (!isServerModerator)
        {
            isServerModerator = await AuthorizationService.UserHasPermission(currentUser, 
                PermissionConstants.GameServer.Gameserver.Dynamic(_gameServer.Id, DynamicPermissionLevel.Moderator));
        }

        if (isServerModerator)
        {
            _canViewGameServer = true;
            _canPermissionGameServer = true;
            _canEditGameServer = true;
            _canConfigureGameServer = true;
            _canStartGameServer = true;
            _canStopGameServer = true;
            _canDeleteGameServer = false;
            _canChangeOwnership = true;
        }
        
        _canPermissionGameServer =  await AuthorizationService.UserHasPermission(currentUser,
                                        PermissionConstants.GameServer.Gameserver.Dynamic(_gameServer.Id, DynamicPermissionLevel.Permission));
        _canEditGameServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Update);
        _canConfigureGameServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Update) ||
                             await AuthorizationService.UserHasPermission(currentUser,
                                 PermissionConstants.GameServer.Gameserver.Dynamic(_gameServer.Id, DynamicPermissionLevel.Configure));
        _canStartGameServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.StartServer) ||
                                  await AuthorizationService.UserHasPermission(currentUser,
                                      PermissionConstants.GameServer.Gameserver.Dynamic(_gameServer.Id, DynamicPermissionLevel.Start));
        _canStopGameServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.StopServer) ||
                              await AuthorizationService.UserHasPermission(currentUser,
                                  PermissionConstants.GameServer.Gameserver.Dynamic(_gameServer.Id, DynamicPermissionLevel.Stop));
        _canDeleteGameServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Delete);
        _canChangeOwnership = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.ChangeOwnership);
    }

    private async Task GetGameServerPermissions()
    {
        var assignedServerPermissions = await PermissionService.GetDynamicByTypeAndNameAsync(DynamicPermissionGroup.GameServers, _gameServer.Id);
        _assignedUserPermissions.Clear();
        _assignedRolePermissions.Clear();
        
        var filteredUserPermissions = assignedServerPermissions.Data.Where(x => x.UserId != Guid.AllBitsSet).ToDisplays();
        foreach (var permission in filteredUserPermissions.OrderBy(x => x.UserId))
        {
            var matchingUser = await UserService.GetByIdAsync(permission.UserId);
            permission.UserName = matchingUser.Data?.Username ?? "Unknown";
            _assignedUserPermissions.Add(permission);
        }
        
        var filteredRolePermissions = assignedServerPermissions.Data.Where(x => x.RoleId != Guid.AllBitsSet).ToDisplays();
        foreach (var permission in filteredRolePermissions.OrderBy(x => x.RoleId))
        {
            var matchingRole = await RoleService.GetByIdAsync(permission.RoleId);
            permission.RoleName = matchingRole.Data?.Name ?? "Unknown";
            _assignedRolePermissions.Add(permission);
        }
    }
    
    private async Task Save()
    {
        if (!_canConfigureGameServer && !_canEditGameServer)
        {
            return;
        }
        
        var response = await GameServerService.UpdateAsync(_gameServer.ToUpdate(), _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (_createdConfigItems.Count != 0 || _updatedConfigItems.Count != 0 || _deletedConfigItems.Count != 0)
        {
            if (await SaveNewConfigItems())
            {
                return;
            }
            if (await SaveUpdatedConfigItems())
            {
                return;
            }
            if (await SaveDeletedConfigItems())
            {
                return;
            }

            var hostUpdateResponse = await GameServerService.UpdateAllLocalResourcesOnGameServerAsync(_gameServer.Id, _loggedInUserId);
            if (!hostUpdateResponse.Succeeded)
            {
                hostUpdateResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
        }
        
        ToggleEditMode();
        await GetViewingGameServer();
        await GetGameServerResources();
        
        Snackbar.Add("Gameserver successfully updated!", Severity.Success);
        StateHasChanged();
    }

    private async Task<bool> SaveDeletedConfigItems()
    {
        foreach (var configItem in _deletedConfigItems)
        {
            // Config item comes from inherited profile resource, create server profile resource for the config item to be ignored
            if (configItem.LocalResourceId == Guid.Empty)
            {
                var matchingLocalResource = _localResources.First(x => x.ConfigSets.Any(c => c.Id == configItem.Id));
                var createResourceResponse = await GameServerService.CreateLocalResourceAsync(matchingLocalResource.ToCreateRequest(), _loggedInUserId);
                if (!createResourceResponse.Succeeded)
                {
                    createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return true;
                }
                
                configItem.LocalResourceId = createResourceResponse.Data;
                configItem.Ignore = true;
                
                var ignoreCreateResponse = await GameServerService.CreateConfigurationItemAsync(configItem.ToCreate(), _loggedInUserId);
                if (!ignoreCreateResponse.Succeeded)
                {
                    ignoreCreateResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return true;
                }
            }

            var deleteConfigResponse = await GameServerService.DeleteConfigurationItemAsync(configItem.Id, _loggedInUserId);
            if (deleteConfigResponse.Succeeded) continue;
            
            deleteConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return true;
        }

        _deletedConfigItems.Clear();
        return false;
    }

    private async Task<bool> SaveUpdatedConfigItems()
    {
        foreach (var configItem in _updatedConfigItems)
        {
            // Config item comes from inherited profile resource, create server profile resource for the config item
            if (configItem.LocalResourceId == Guid.Empty)
            {
                var matchingLocalResource = _localResources.First(x => x.ConfigSets.Any(c => c.Id == configItem.Id));
                var existingLocalResource = _localResources.FirstOrDefault(x => x.GameProfileId == _gameServer.GameProfileId &&
                                                                                (x.PathWindows.Length != 0 && x.PathWindows == matchingLocalResource.PathWindows) ||
                                                                                (x.PathLinux.Length != 0 && x.PathLinux == matchingLocalResource.PathLinux) ||
                                                                                (x.PathMac.Length != 0 && x.PathMac == matchingLocalResource.PathMac));

                if (existingLocalResource is null)
                {
                    var resourceCreateRequest = matchingLocalResource.ToCreateRequest();
                    resourceCreateRequest.GameProfileId = _gameServer.GameProfileId;
                    var createResourceResponse = await GameServerService.CreateLocalResourceAsync(resourceCreateRequest, _loggedInUserId);
                    if (!createResourceResponse.Succeeded)
                    {
                        createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                        return true;
                    }
                
                    configItem.LocalResourceId = createResourceResponse.Data;
                }
                else
                {
                    configItem.LocalResourceId = existingLocalResource.Id;
                }
            }
            
            var updateConfigResponse = await GameServerService.UpdateConfigurationItemAsync(configItem.ToUpdate(), _loggedInUserId);
            if (updateConfigResponse.Succeeded) continue;
            
            updateConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return true;
        }

        _updatedConfigItems.Clear();
        return false;
    }

    private async Task<bool> SaveNewConfigItems()
    {
        foreach (var configItem in _createdConfigItems)
        {
            // Check if there is a matching-ignored item (meaning deleted and to ignore from inherited config), if there is update it instead of creating a new item
            var matchingIgnoreItem = _localResources
                .Where(x => x.Id == configItem.LocalResourceId)
                .Select(x => x.ConfigSets.FirstOrDefault(c => c.Ignore && c.Key == configItem.Key)).FirstOrDefault();
            if (matchingIgnoreItem is not null)
            {
                matchingIgnoreItem.Ignore = false;
                matchingIgnoreItem.FriendlyName = configItem.FriendlyName;
                matchingIgnoreItem.Value = configItem.Value;
                matchingIgnoreItem.Category = configItem.Category;
                matchingIgnoreItem.Path = configItem.Path;
                _updatedConfigItems.Add(matchingIgnoreItem);
                continue;
            }
            
            var createConfigResponse = await GameServerService.CreateConfigurationItemAsync(configItem.ToCreate(), _loggedInUserId);
            if (createConfigResponse.Succeeded) continue;
            
            createConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return true;
        }

        _createdConfigItems.Clear();
        return false;
    }

    private void ToggleEditMode()
    {
        // TODO: Add file / resource creation in the advanced section (also rename to expert) along w/ showing file path, also allow resource delete
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
        
        // TODO: Delete is failing w/: Failed to delete server profile before game server deletion
        // TODO: Error detail for tshoot log is also showing: "Error": "System.Collections.Generic.List\u00601[System.String]"
        var response = await GameServerService.DeleteAsync(_gameServer.Id, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        Snackbar.Add("Gameserver successfully deleted!", Severity.Success);
        GoBack();
    }
    
    private bool ConfigShouldBeShown(ConfigurationItemSlim item)
    {
        var shouldBeShown = item.FriendlyName.Contains(_configSearchText, StringComparison.OrdinalIgnoreCase) ||
                            item.Key.Contains(_configSearchText, StringComparison.OrdinalIgnoreCase) ||
                            item.Value.Contains(_configSearchText, StringComparison.OrdinalIgnoreCase);
        
        return shouldBeShown;
    }

    private async Task ConfigAdd(LocalResourceSlim localResource)
    {
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialogParameters = new DialogParameters() {{"ReferenceResource", localResource}};
        var dialog = await DialogService.ShowAsync<ConfigAddDialog>("Add Config Item", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var newConfigItem = (ConfigurationItemSlim) dialogResult.Data;
        
        _createdConfigItems.Add(newConfigItem);
        localResource.ConfigSets = localResource.ConfigSets.ToList().Prepend(newConfigItem);
    }
    
    private void ConfigUpdated(ConfigurationItemSlim item)
    {
        var matchingNewConfig = _createdConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingNewConfig is not null)
        {
            matchingNewConfig.FriendlyName = item.FriendlyName;
            matchingNewConfig.Key = item.Key;
            matchingNewConfig.Value = item.Value;
            matchingNewConfig.Category = item.Category;
            matchingNewConfig.Path = item.Path;
            matchingNewConfig.DuplicateKey = item.DuplicateKey;
            return;
        }
        
        var matchingUpdateConfig = _updatedConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingUpdateConfig is null)
        {
            _updatedConfigItems.Add(item);
            return;
        }
        
        matchingUpdateConfig.FriendlyName = item.FriendlyName;
        matchingUpdateConfig.Key = item.Key;
        matchingUpdateConfig.Value = item.Value;
        matchingUpdateConfig.Category = item.Category;
        matchingUpdateConfig.Path = item.Path;
        matchingUpdateConfig.DuplicateKey = item.DuplicateKey;
    }

    private void ConfigDeleted(ConfigurationItemSlim item)
    {
        if (_createdConfigItems.Contains(item))
        {
            _createdConfigItems.Remove(item);
            return;
        }
        
        // Go through each resource config set and remove the targeted config item
        foreach (var resource in _localResources)
        {
            // TODO: Only remove from resource if it is not a parent profile, add as an ignore if it is a parent profile
            var matchingActiveConfig = resource.ConfigSets.FirstOrDefault(x => x.Id == item.Id);
            if (matchingActiveConfig is null) continue;
            
            resource.ConfigSets = resource.ConfigSets.Where(x => x.Id != item.Id).ToList();
        }
        
        // Remove this config item from updated if it was updated and is now being deleted
        var matchingUpdateConfig = _updatedConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingUpdateConfig is not null)
        {
            _updatedConfigItems.Remove(item);
        }
        
        // Add the config item to the update list to delete when saved
        var matchingDeleteConfig = _deletedConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingDeleteConfig is null)
        {
            _deletedConfigItems.Add(item);
        }
    }

    private async Task AddPermissions(bool isForRolesNotUsers)
    {
        // TODO: Look at solutions for granting permissions with users, roles and permission access needed in the current state
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialogParameters = new DialogParameters() {{"GameServerId", _gameServer.Id}, {"IsForRolesNotUsers", isForRolesNotUsers}};
        var dialog = await DialogService.ShowAsync<GameServerPermissionAddDialog>("Add Gameserver Permissions", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult is null || dialogResult.Canceled)
        {
            return;
        }

        Snackbar.Add("Successfully updated permissions to the game server!", Severity.Success);
        await GetGameServerPermissions();
    }

    private async Task DeletePermissions()
    {
        if (_deleteRolePermissions.Count == 0 && _deleteUserPermissions.Count == 0)
        {
            return;
        }

        foreach (var permission in _deleteRolePermissions)
        {
            var deleteRolePermResponse = await PermissionService.DeleteAsync(permission.Id, _loggedInUserId);
            if (!deleteRolePermResponse.Succeeded)
            {
                deleteRolePermResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
        }

        foreach (var permission in _deleteUserPermissions)
        {
            var deleteUserPermResponse = await PermissionService.DeleteAsync(permission.Id, _loggedInUserId);
            if (!deleteUserPermResponse.Succeeded)
            {
                deleteUserPermResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
        }
        
        Snackbar.Add("Successfully deleted permissions to the game server!", Severity.Success);
        await GetGameServerPermissions();
    }

    private async Task StartGameServer()
    {
        if (_gameServer.ServerState.IsRunning())
        {
            Snackbar.Add("Unable to start an already running game server", Severity.Error);
            return;
        }

        if (_gameServer.ServerState.IsDoingSomething())
        {
            Snackbar.Add($"The server is currently {_gameServer.ServerState}", Severity.Error);
            return;
        }
        
        var startRequest = await GameServerService.StartServerAsync(_gameServer.Id, _loggedInUserId);
        if (!startRequest.Succeeded)
        {
            foreach (var message in startRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Starting the server now!", Severity.Success);
    }

    private async Task StopGameServer()
    {
        if (!_gameServer.ServerState.IsRunning())
        {
            Snackbar.Add("Unable to stop an already stopped game server", Severity.Error);
            return;
        }

        if (_gameServer.ServerState.IsDoingSomething())
        {
            Snackbar.Add($"The server is currently {_gameServer.ServerState}", Severity.Error);
            return;
        }

        var stopRequest = await GameServerService.StopServerAsync(_gameServer.Id, _loggedInUserId);
        if (!stopRequest.Succeeded)
        {
            foreach (var message in stopRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Stopping the server now!", Severity.Success);
    }

    private async Task RestartGameServer()
    {
        if (_gameServer.ServerState.IsDoingSomething())
        {
            Snackbar.Add($"The server is currently {_gameServer.ServerState}", Severity.Error);
            return;
        }
        
        var restartRequest = await GameServerService.RestartServerAsync(_gameServer.Id, _loggedInUserId);
        if (!restartRequest.Succeeded)
        {
            foreach (var message in restartRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Restarting the server now!", Severity.Success);
    }

    private async Task UpdateGameServer()
    {
        if (_gameServer.ServerState.IsDoingSomething())
        {
            Snackbar.Add($"The server is currently {_gameServer.ServerState}, unable to deploy an update", Severity.Error);
            return;
        }

        var updateRequest = await GameServerService.UpdateServerAsync(_gameServer.Id, _loggedInUserId);
        if (!updateRequest.Succeeded)
        {
            foreach (var message in updateRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Updating the server now!", Severity.Success);
    }

    private async Task ChangeOwnership()
    {
        if (!_canChangeOwnership)
        {
            return;
        }
        
        var dialogParameters = new DialogParameters()
        {
            {"ConfirmButtonText", "Change Gameserver Owner"},
            {"Title", "Transfer Gameserver Ownership"},
            {"OwnerId", _gameServer.OwnerId}
        };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };

        var dialog = await DialogService.ShowAsync<ChangeOwnershipDialog>("Transfer Gameserver Ownership", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var responseOwnerId = (Guid) dialogResult.Data;
        if (_gameServer.OwnerId == responseOwnerId)
        {
            Snackbar.Add("Selected owner is already the owner, everything is as it was", Severity.Info);
            return;
        }

        var updateRequest = _gameServer.ToUpdate();
        updateRequest.OwnerId = responseOwnerId;

        var response = await GameServerService.UpdateAsync(updateRequest, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Successfully transferred ownership!", Severity.Success);
        // Since ownership has changed we want to re-verify permissions
        await GetPermissions();
        StateHasChanged();
    }

    private async Task CopyToClipboard(string text)
    {
        await WebClientService.InvokeClipboardCopy(text);
        Snackbar.Add("Text copied to your clipboard!", Severity.Success);
    }
    
    private async Task<TableData<NotifyRecordSlim>> ServerReload(TableState state, CancellationToken token)
    {
        var recordResponse = await NotifyRecordService.SearchPaginatedAsync(_notifySearchText, state.Page + 1, state.PageSize);
        if (!recordResponse.Succeeded)
        {
            recordResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return new TableData<NotifyRecordSlim>();
        }

        _notifyPagedData = recordResponse.Data.ToArray();
        _totalNotifyRecords = recordResponse.TotalCount;

        _notifyPagedData = state.SortLabel switch
        {
            "Timestamp" => _notifyPagedData.OrderByDirection(state.SortDirection, o => o.Timestamp),
            "Message" => _notifyPagedData.OrderByDirection(state.SortDirection, o => o.Message),
            _ => _notifyPagedData
        };

        return new TableData<NotifyRecordSlim>() {TotalItems = _totalNotifyRecords, Items = _notifyPagedData};
    }

    private void InjectDynamicValue(ConfigurationItemSlim item, string value)
    {
        item.Value = value;
    }

    private void NotifyTriggered(object? sender, NotifyTriggeredEvent e)
    {
        if (_gameServer.Id != e.EntityId)
        {
            return;
        }
        
        _notifyTable.ReloadServerData();
    }

    private void GameServerStatusChanged(object? sender, GameServerStatusEvent args)
    {
        if (_gameServer.Id != args.Id)
        {
            return;
        }
        
        _gameServer.ServerState = args.ServerState;

        if (args.BuildVersionUpdated)
        {
            _gameServer.ServerBuildVersion = _game.LatestBuildVersion;
            _updateIsAvailable = _gameServer.ServerBuildVersion != _game.LatestBuildVersion;
        }
        
        InvokeAsync(StateHasChanged);
    }

    private void GameVersionUpdated(object? sender, GameVersionUpdatedEvent args)
    {
        if (_game.Id != args.GameId)
        {
            return;
        }

        _game.LatestBuildVersion = args.VersionBuild;
        _updateIsAvailable = _gameServer.ServerBuildVersion != _game.LatestBuildVersion;
        
        InvokeAsync(StateHasChanged);
    }

    private void SelectNotifyDetailView(NotifyRecordSlim record)
    {
        if (_selectedNotifyViewDetail == record.Id)
        {
            _selectedNotifyViewDetail = 0;
            return;
        }
        
        _selectedNotifyViewDetail = record.Id;
        
        StateHasChanged();
    }

    private void NotifySearchChanged()
    {
        _notifyTable.ReloadServerData();
    }
    
    public async ValueTask DisposeAsync()
    {
        EventService.GameVersionUpdated -= GameVersionUpdated;
        EventService.GameServerStatusChanged -= GameServerStatusChanged;
        EventService.NotifyTriggered -= NotifyTriggered;
        
        await Task.CompletedTask;
    }
}   