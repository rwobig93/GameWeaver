using System.Xml.Linq;
using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.External;
using Application.Helpers.GameServer;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Mappers.Identity;
using Application.Models.Events;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Host;
using Application.Models.GameServer.LocalResource;
using Application.Models.Identity.Permission;
using Application.Models.Lifecycle;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Enums.GameServer;
using Domain.Enums.Identity;
using Domain.Enums.Integrations;
using GameWeaver.Components.GameServer;
using GameWeaver.Helpers;
using GameWeaverShared.Parsers;
using Microsoft.AspNetCore.Components.Forms;

namespace GameWeaver.Pages.GameServer;

public partial class GameServerView : ComponentBase, IAsyncDisposable
{
    private readonly List<AppPermissionDisplay> _assignedRolePermissions = [];
    private readonly List<AppPermissionDisplay> _assignedUserPermissions = [];
    private readonly List<ConfigurationItemSlim> _createdConfigItems = [];
    private readonly List<LocalResourceSlim> _createdLocalResources = [];
    private readonly List<ConfigurationItemSlim> _deletedConfigItems = [];
    private readonly List<LocalResourceSlim> _deletedLocalResources = [];
    private readonly List<ConfigurationItemSlim> _updatedConfigItems = [];
    private readonly List<LocalResourceSlim> _updatedLocalResources = [];
    private List<GameProfileSlim>? _availableParentProfiles;
    private bool _canChangeOwnership;
    private bool _canConfigServer;
    private bool _canDeleteServer;
    private bool _canEditServer;
    private bool _canPermissionServer;
    private bool _canStartServer;
    private bool _canStopServer;

    private bool _canViewGameServer;
    private string _configSearchText = string.Empty;
    private HashSet<AppPermissionDisplay> _deleteRolePermissions = [];
    private HashSet<AppPermissionDisplay> _deleteUserPermissions = [];
    private string _editButtonText = "Enable Edit Mode";
    private bool _editMode;
    private GameSlim _game = new() {Id = Guid.Empty};
    private GameServerSlim _gameServer = new() {Id = Guid.Empty};
    private HostSlim _host = new() {Id = Guid.Empty};
    private List<LocalResourceSlim> _localResources = [];
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private Guid _loggedInUserId = Guid.Empty;
    private IEnumerable<NotifyRecordSlim> _notifyPagedData = new List<NotifyRecordSlim>();
    private string _notifySearchText = string.Empty;
    private MudTable<NotifyRecordSlim> _notifyTable = new();
    private GameProfileSlim? _parentProfile;
    private int _selectedNotifyViewDetail;
    private int _totalNotifyRecords;
    private bool _updateIsAvailable;

    private bool _validIdProvided = true;
    [Parameter] public Guid GameServerId { get; init; } = Guid.Empty;

    [Inject] public IAppRoleService RoleService { get; init; } = null!;
    [Inject] public IGameServerService GameServerService { get; init; } = null!;
    [Inject] public IGameService GameService { get; init; } = null!;
    [Inject] public IHostService HostService { get; init; } = null!;
    [Inject] public IWebClientService WebClientService { get; init; } = null!;
    [Inject] public IEventService EventService { get; init; } = null!;
    [Inject] public INotifyRecordService NotifyRecordService { get; init; } = null!;
    [Inject] public IAppPermissionService PermissionService { get; init; } = null!;
    [Inject] public IAppUserService UserService { get; init; } = null!;
    [Inject] public ISerializerService SerializerService { get; init; } = null!;

    public async ValueTask DisposeAsync()
    {
        EventService.GameVersionUpdated -= GameVersionUpdated;
        EventService.GameServerStatusChanged -= GameServerStatusChanged;
        EventService.NotifyTriggered -= NotifyTriggered;

        await Task.CompletedTask;
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await GetViewingGameServer();
                await GetServerParentProfile();
                await GetServerGame();
                await GetServerHost();
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
        var response = await GameServerService.GetByIdAsync(GameServerId, _loggedInUserId);
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
            _parentProfile = new GameProfileSlim {Id = Guid.Empty, FriendlyName = "None"};
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

    private async Task GetServerHost()
    {
        var hostResponse = await HostService.GetByIdAsync(_gameServer.HostId);
        if (!hostResponse.Succeeded || hostResponse.Data is null)
        {
            Snackbar.Add("Failed to find host for server, please reach out to an administrator", Severity.Error);
            return;
        }

        _host = hostResponse.Data;
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

        _canViewGameServer = !_gameServer.Private || await AuthorizationService.UserHasGlobalOrDynamicPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get,
            DynamicPermissionGroup.GameServers, DynamicPermissionLevel.View, _gameServer.Id);

        var isServerAdmin = (await RoleService.IsUserAdminAsync(_loggedInUserId)).Data ||
                            await AuthorizationService.UserHasDynamicPermission(currentUser, DynamicPermissionGroup.GameServers, DynamicPermissionLevel.Admin, _gameServer.Id);
        // Game server owner and admin will get full permissions
        if (_gameServer.OwnerId == _loggedInUserId || isServerAdmin)
        {
            _canViewGameServer = true;
            _canPermissionServer = true;
            _canEditServer = true;
            _canConfigServer = true;
            _canStartServer = true;
            _canStopServer = true;
            _canDeleteServer = true;
            _canChangeOwnership = true;
            return;
        }

        // Moderators get most access except deletion and ownership changing
        var isServerModerator = (await RoleService.IsUserModeratorAsync(_loggedInUserId)).Data ||
                                await AuthorizationService.UserHasDynamicPermission(currentUser, DynamicPermissionGroup.GameServers, DynamicPermissionLevel.Moderator,
                                    _gameServer.Id);
        if (isServerModerator)
        {
            _canViewGameServer = true;
            _canPermissionServer = true;
            _canEditServer = true;
            _canConfigServer = true;
            _canStartServer = true;
            _canStopServer = true;
            _canDeleteServer = false;
            _canChangeOwnership = false;
            return;
        }

        _canPermissionServer =
            await AuthorizationService.UserHasDynamicPermission(currentUser, DynamicPermissionGroup.GameServers, DynamicPermissionLevel.Permission, _gameServer.Id);
        _canEditServer = await AuthorizationService.UserHasGlobalOrDynamicPermission(currentUser, PermissionConstants.GameServer.Gameserver.Update,
            DynamicPermissionGroup.GameServers, DynamicPermissionLevel.Edit, _gameServer.Id);
        _canConfigServer = await AuthorizationService.UserHasGlobalOrDynamicPermission(currentUser, PermissionConstants.GameServer.Gameserver.Update,
            DynamicPermissionGroup.GameServers, DynamicPermissionLevel.Configure, _gameServer.Id);
        _canStartServer = await AuthorizationService.UserHasGlobalOrDynamicPermission(currentUser, PermissionConstants.GameServer.Gameserver.StartServer,
            DynamicPermissionGroup.GameServers, DynamicPermissionLevel.Start, _gameServer.Id);
        _canStopServer = await AuthorizationService.UserHasGlobalOrDynamicPermission(currentUser, PermissionConstants.GameServer.Gameserver.StopServer,
            DynamicPermissionGroup.GameServers, DynamicPermissionLevel.Stop, _gameServer.Id);
        _canDeleteServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Delete);
        _canChangeOwnership = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.ChangeOwnership);
    }

    private async Task GetGameServerPermissions()
    {
        var assignedPermissions = await PermissionService.GetDynamicByTypeAndNameAsync(DynamicPermissionGroup.GameServers, _gameServer.Id);
        _assignedUserPermissions.Clear();
        _assignedRolePermissions.Clear();

        var filteredUserPermissions = assignedPermissions.Data.Where(x => x.UserId != Guid.AllBitsSet).ToDisplays();
        foreach (var permission in filteredUserPermissions.OrderBy(x => x.UserId))
        {
            var matchingUser = await UserService.GetByIdAsync(permission.UserId);
            permission.UserName = matchingUser.Data?.Username ?? "Unknown";
            _assignedUserPermissions.Add(permission);
        }

        var filteredRolePermissions = assignedPermissions.Data.Where(x => x.RoleId != Guid.AllBitsSet).ToDisplays();
        foreach (var permission in filteredRolePermissions.OrderBy(x => x.RoleId))
        {
            var matchingRole = await RoleService.GetByIdAsync(permission.RoleId);
            permission.RoleName = matchingRole.Data?.Name ?? "Unknown";
            _assignedRolePermissions.Add(permission);
        }
    }

    private async Task Save()
    {
        if (!_canConfigServer && !_canEditServer)
        {
            return;
        }

        await GetServerHost();
        if (!_host.CurrentState.IsRunning())
        {
            Snackbar.Add("The host for this gameserver is currently offline, changes will occur once the host is online again", Severity.Warning);
            StateHasChanged();
        }

        _gameServer.ParentGameProfileId = _parentProfile?.Id == Guid.Empty || _parentProfile?.Id == null ? null : _parentProfile?.Id;
        if (_gameServer.ParentGameProfileId is null)
        {
            var parentUpdateResponse = await GameServerService.UpdateParentProfileAsync(_gameServer.ToParentUpdate(), _loggedInUserId);
            if (!parentUpdateResponse.Succeeded)
            {
                parentUpdateResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
        }

        var response = await GameServerService.UpdateAsync(_gameServer.ToUpdate(), _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var resource in _deletedLocalResources)
        {
            resource.PathWindows = FileHelpers.SanitizeSecureFilename(resource.PathWindows);
            resource.PathLinux = FileHelpers.SanitizeSecureFilename(resource.PathLinux);
            resource.PathMac = FileHelpers.SanitizeSecureFilename(resource.PathMac);
            // Create a resource as ignored if deleted and is from the parent or default game profile
            if (resource.Id == Guid.Empty)
            {
                resource.GameProfileId = _gameServer.GameProfileId;
                // TODO: Difference between ignore and deleted, update client, ignore should never make it to the client
                // TODO: Update dialog for resource not existing to be delete instead, ignore should only be for inherited resources
                resource.ContentType = ContentType.Ignore;
                var createResourceResponse = await GameServerService.CreateLocalResourceAsync(resource.ToCreate(), _loggedInUserId);
                if (createResourceResponse.Succeeded)
                {
                    resource.Id = createResourceResponse.Data;
                    _localResources.Add(resource);
                    continue;
                }

                createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            var deleteResourceResponse = await GameServerService.DeleteLocalResourceAsync(resource.Id, _loggedInUserId);
            if (deleteResourceResponse.Succeeded) continue;

            deleteResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _deletedLocalResources.Clear();

        foreach (var resource in _createdLocalResources)
        {
            resource.PathWindows = FileHelpers.SanitizeSecureFilename(resource.PathWindows);
            resource.PathLinux = FileHelpers.SanitizeSecureFilename(resource.PathLinux);
            resource.PathMac = FileHelpers.SanitizeSecureFilename(resource.PathMac);
            var createResourceResponse = await GameServerService.CreateLocalResourceAsync(resource.ToCreate(), _loggedInUserId);
            if (createResourceResponse.Succeeded) continue;

            createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _createdLocalResources.Clear();

        foreach (var resource in _updatedLocalResources)
        {
            resource.PathWindows = FileHelpers.SanitizeSecureFilename(resource.PathWindows);
            resource.PathLinux = FileHelpers.SanitizeSecureFilename(resource.PathLinux);
            resource.PathMac = FileHelpers.SanitizeSecureFilename(resource.PathMac);
            var updateResourceResponse = await GameServerService.UpdateLocalResourceAsync(resource.ToUpdate(), _loggedInUserId);
            if (updateResourceResponse.Succeeded) continue;

            updateResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _updatedLocalResources.Clear();

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
        }

        var hostUpdateResponse = await GameServerService.UpdateAllLocalResourcesOnGameServerAsync(_gameServer.Id, _loggedInUserId);
        if (!hostUpdateResponse.Succeeded)
        {
            hostUpdateResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _createdConfigItems.Clear();
        _updatedConfigItems.Clear();
        _deletedConfigItems.Clear();

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
                var createResourceResponse = await GameServerService.CreateLocalResourceAsync(matchingLocalResource.ToCreate(), _loggedInUserId);
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
                    var resourceCreateRequest = matchingLocalResource.ToCreate();
                    resourceCreateRequest.PathWindows = FileHelpers.SanitizeSecureFilename(resourceCreateRequest.PathWindows);
                    resourceCreateRequest.PathLinux = FileHelpers.SanitizeSecureFilename(resourceCreateRequest.PathLinux);
                    resourceCreateRequest.PathMac = FileHelpers.SanitizeSecureFilename(resourceCreateRequest.PathMac);
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

            // Config item comes from inherited profile resource and that resource doesn't exist on the game server so we'll create it then assign our item to it
            if (configItem.LocalResourceId == Guid.Empty)
            {
                var matchingLocalResource = _localResources.First(x => x.ConfigSets.Any(c => c.Id == configItem.Id));
                var resourceCreateRequest = matchingLocalResource.ToCreate();
                resourceCreateRequest.GameProfileId = _gameServer.GameProfileId;
                var createResourceResponse = await GameServerService.CreateLocalResourceAsync(resourceCreateRequest, _loggedInUserId);
                if (!createResourceResponse.Succeeded)
                {
                    createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return true;
                }

                configItem.LocalResourceId = createResourceResponse.Data;
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
        _editMode = !_editMode;

        _editButtonText = _editMode ? "Disable Edit Mode" : "Enable Edit Mode";
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewAll);
    }

    private async Task DeleteGameServer()
    {
        if (!_canDeleteServer)
        {
            Snackbar.Add(ErrorMessageConstants.Permissions.PermissionError, Severity.Error);
            return;
        }

        await GetServerHost();
        if (!_host.CurrentState.IsRunning())
        {
            Snackbar.Add("The host for this gameserver is currently offline", Severity.Error);
            StateHasChanged();
            return;
        }

        var dialogParameters = new DialogParameters
        {
            {"Title", "Are you sure you want to delete this gameserver?"},
            {"Content", $"Server Name: {_gameServer.ServerName}"}
        };
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};

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

    private async Task ConfigAdd(LocalResourceSlim localResource)
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters {{"ReferenceResource", localResource}};
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

        // If the config item is from a parent profile, add as a new config item instead
        if (item.Id == Guid.Empty)
        {
            item.Id = Guid.CreateVersion7();
            _createdConfigItems.Add(item);
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
        _createdConfigItems.Remove(item);

        // Go through each resource config set and remove the targeted config item
        foreach (var resource in _localResources)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var resourceConfigSets = resource.ConfigSets.ToList();
            // If we have an item ID we will use that, otherwise we find the item by all other properties that combine to be a 'unique' config item
            var matchingActiveConfig = item.Id != Guid.Empty
                ? resourceConfigSets.FirstOrDefault(x => x.Id == item.Id)
                : resourceConfigSets.FirstOrDefault(x =>
                    x.Key == item.Key &&
                    x.Path == item.Path &&
                    x.DuplicateKey == item.DuplicateKey &&
                    x.Value == item.Value &&
                    x.Category == item.Category);
            if (matchingActiveConfig is null) continue;

            // If the config item is from a parent profile, add as an ignore to the existing resource on the direct profile instead
            if (matchingActiveConfig.LocalResourceId == Guid.Empty)
            {
                matchingActiveConfig.Id = Guid.CreateVersion7();
                matchingActiveConfig.LocalResourceId = resource.Id;
                matchingActiveConfig.Ignore = true;
                _createdConfigItems.Add(matchingActiveConfig);
                resource.ConfigSets = resourceConfigSets.ToList().Prepend(matchingActiveConfig);
                continue;
            }

            resource.ConfigSets = resourceConfigSets.Where(x => x.Id != item.Id).ToList();
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

    private async Task LocalResourceAdd(ResourceType resourceType)
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters {{"GameProfileId", _gameServer.GameProfileId}, {"ResourceType", resourceType}};
        var dialog = await DialogService.ShowAsync<LocalResourceAddDialog>("New Local Resource", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var newResource = (LocalResourceSlim) dialogResult.Data;

        _localResources.Add(newResource);
        _createdLocalResources.Add(newResource);
    }

    private void LocalResourceUpdate(LocalResourceSlim localResource)
    {
        // If the local resource is from a parent profile, add as a new resource instead
        if (localResource.Id == Guid.Empty)
        {
            localResource.Id = Guid.CreateVersion7();
            foreach (var configItem in localResource.ConfigSets) // Update the config items to point to our new resource ID
            {
                configItem.LocalResourceId = localResource.Id;
            }

            _createdLocalResources.Add(localResource);
            return;
        }

        var matchingNewResource = _createdLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingNewResource is not null)
        {
            matchingNewResource.Name = localResource.Name;
            matchingNewResource.PathWindows = localResource.PathWindows;
            matchingNewResource.PathLinux = localResource.PathLinux;
            matchingNewResource.PathMac = localResource.PathMac;
            matchingNewResource.Args = localResource.Args;
            return;
        }

        var matchingUpdatedResource = _updatedLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingUpdatedResource is null)
        {
            _updatedLocalResources.Add(localResource);
            return;
        }

        matchingUpdatedResource.Name = localResource.Name;
        matchingUpdatedResource.PathWindows = localResource.PathWindows;
        matchingUpdatedResource.PathLinux = localResource.PathLinux;
        matchingUpdatedResource.PathMac = localResource.PathMac;
        matchingUpdatedResource.Args = localResource.Args;
    }

    private async Task LocalResourceDelete(LocalResourceSlim localResource)
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters
        {
            {"Title", "Are you sure you want to delete this local resource?"},
            {"Content", $"You want to delete the resource '{localResource.Name}'?"}
        };
        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Delete Local Resource", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var deleteResource = (bool) dialogResult.Data;
        if (!deleteResource)
        {
            return;
        }

        _createdLocalResources.Remove(localResource);
        _updatedLocalResources.Remove(localResource);
        _localResources.Remove(localResource);
        _deletedLocalResources.Add(localResource);
        foreach (var configItem in localResource.ConfigSets)
        {
            _createdConfigItems.Remove(configItem);
            _updatedConfigItems.Remove(configItem);
            _deletedConfigItems.Remove(configItem);
        }
    }

    private async Task AddPermissions(bool isForRolesNotUsers)
    {
        var dialogResult = await DialogService.DynamicPermissionsAddDialog("Add Gameserver Permissions", _gameServer.Id, DynamicPermissionGroup.GameServers,
            _canPermissionServer, isForRolesNotUsers);
        if (dialogResult.Data is null || dialogResult.Canceled)
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

        await GetServerHost();
        if (!_host.CurrentState.IsRunning())
        {
            Snackbar.Add("The host for this gameserver is currently offline", Severity.Error);
            StateHasChanged();
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

        await GetServerHost();
        if (!_host.CurrentState.IsRunning())
        {
            Snackbar.Add("The host for this gameserver is currently offline", Severity.Error);
            StateHasChanged();
            return;
        }

        if (_gameServer.ServerState.IsDoingSomething() && _gameServer.ServerState != ConnectivityState.Discovering)
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

        await GetServerHost();
        if (!_host.CurrentState.IsRunning())
        {
            Snackbar.Add("The host for this gameserver is currently offline", Severity.Error);
            StateHasChanged();
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

        await GetServerHost();
        if (!_host.CurrentState.IsRunning())
        {
            Snackbar.Add("The host for this gameserver is currently offline", Severity.Error);
            StateHasChanged();
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

        var dialogResult = await DialogService.ChangeOwnershipDialog("Transfer Gameserver Ownership", _gameServer.OwnerId, "Change Gameserver Owner");
        if (dialogResult.Data is null || dialogResult.Canceled)
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

    private void ViewGame()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.Games.ViewId(_game.Id));
    }

    private void ViewHost()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.Hosts.ViewId(_host.Id));
    }

    private async Task ConnectToServer()
    {
        var urlOpened = await WebClientService.OpenExternalUrl(SteamHelpers.ConnectToServerUri(_gameServer.PublicIp, _gameServer.PortGame, _gameServer.Password));
        if (!urlOpened.Succeeded)
        {
            urlOpened.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Ran steam connect to server command successfully! Have fun!", Severity.Success);
    }

    private async Task InstallGameFromSteam()
    {
        var urlOpened = await WebClientService.OpenExternalUrl(SteamHelpers.InstallGameUri(_game.SteamGameId));
        if (!urlOpened.Succeeded)
        {
            urlOpened.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Ran steam install command successfully!", Severity.Success);
    }

    private void ViewParentProfile()
    {
        if (_parentProfile is null || _parentProfile.Id == Guid.Empty)
        {
            return;
        }

        NavManager.NavigateTo(AppRouteConstants.GameServer.GameProfiles.ViewId(_parentProfile.Id));
    }

    private async Task OpenScriptInEditor(LocalResourceSlim resource)
    {
        if (resource.Type != ResourceType.ScriptFile)
        {
            return;
        }

        var usablePath = resource switch
        {
            _ when !string.IsNullOrWhiteSpace(resource.PathWindows) => resource.PathWindows,
            _ when !string.IsNullOrWhiteSpace(resource.PathLinux) => resource.PathLinux,
            _ when !string.IsNullOrWhiteSpace(resource.PathMac) => resource.PathMac,
            _ => string.Empty
        };

        var scriptContent = string.Join(Environment.NewLine, resource.ConfigSets.ToRaw());
        var fileLanguage = FileHelpers.GetLanguageFromName(usablePath);
        var dialogResult = await DialogService.FileEditorDialog(usablePath, scriptContent, fileLanguage, _editMode, true);
        if (dialogResult.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var updatedFileContent = (string) dialogResult.Data;
        var fileContentLines = updatedFileContent.Split(Environment.NewLine);
        var editorConfigItems = resource.ContentType switch
        {
            ContentType.Raw => fileContentLines.ToConfigItems(resource.Id),
            ContentType.Ini => new IniData(fileContentLines).ToConfigItems(resource.Id),
            ContentType.Json => SerializerService.DeserializeJson<Dictionary<string, string>>(updatedFileContent).ToConfigItems(resource.Id),
            ContentType.Xml => XDocument.Parse(updatedFileContent).ToConfigItems(resource.Id),
            _ => fileContentLines.ToConfigItems(resource.Id)
        };

        // Updated raw file has desired state, we'll use existing ID's for existing items then add/delete based on provided state
        editorConfigItems.UpdateEditorConfigFromExisting(resource.ConfigSets);
        var newConfigItems = editorConfigItems.Where(x => resource.ConfigSets.FirstOrDefault(c => c.Id == x.Id) is null).ToList();
        var updateConfigItems = editorConfigItems.Where(x => resource.ConfigSets.FirstOrDefault(c => c.Id == x.Id) is not null).ToList();
        var deleteConfigItems = resource.ConfigSets.Where(x => editorConfigItems.FirstOrDefault(c => c.Id == x.Id) is null).ToList();
        resource.ConfigSets = editorConfigItems;

        AddOrUpdateConfiguration(_createdConfigItems, newConfigItems);
        AddOrUpdateConfiguration(_updatedConfigItems, updateConfigItems);
        AddOrUpdateConfiguration(_deletedConfigItems, deleteConfigItems);
        Snackbar.Add("Script updated, changes won't be made until you save", Severity.Warning);
        StateHasChanged();
    }

    private async Task OpenConfigInEditor(LocalResourceSlim resource)
    {
        if (resource.Type != ResourceType.ConfigFile)
        {
            return;
        }

        var usablePath = resource switch
        {
            _ when !string.IsNullOrWhiteSpace(resource.PathWindows) => resource.PathWindows,
            _ when !string.IsNullOrWhiteSpace(resource.PathLinux) => resource.PathLinux,
            _ when !string.IsNullOrWhiteSpace(resource.PathMac) => resource.PathMac,
            _ => string.Empty
        };

        var fileLanguage = resource.ContentType switch
        {
            ContentType.Ini => FileEditorLanguage.Ini,
            ContentType.Json => FileEditorLanguage.Json,
            ContentType.Xml => FileEditorLanguage.Xml,
            _ => FileHelpers.GetLanguageFromName(usablePath)
        };

        var configContent = resource.ContentType switch
        {
            ContentType.Raw => string.Join(Environment.NewLine, resource.ConfigSets.ToRaw()),
            ContentType.Ini => resource.ConfigSets.ToIni().ToString(),
            ContentType.Xml => resource.ConfigSets.ToXml()?.ToString() ?? string.Empty,
            _ => string.Join(Environment.NewLine, resource.ConfigSets.ToRaw())
        };
        var dialogResult = await DialogService.FileEditorDialog(usablePath, configContent, fileLanguage, _editMode, true);
        if (dialogResult.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var updatedFileContent = (string) dialogResult.Data;
        var fileContentLines = updatedFileContent.Split(Environment.NewLine);
        var configItems = resource.ContentType switch
        {
            ContentType.Raw => fileContentLines.ToConfigItems(resource.Id),
            ContentType.Ini => new IniData(fileContentLines).ToConfigItems(resource.Id),
            ContentType.Json => SerializerService.DeserializeJson<Dictionary<string, string>>(updatedFileContent).ToConfigItems(resource.Id),
            ContentType.Xml => XDocument.Parse(updatedFileContent).ToConfigItems(resource.Id),
            _ => fileContentLines.ToConfigItems(resource.Id)
        };

        // Updated raw file has the desired state, we'll use existing ID's for existing items then add/delete based on provided state
        configItems.UpdateEditorConfigFromExisting(resource.ConfigSets);
        var newConfigItems = configItems.Where(x => resource.ConfigSets.FirstOrDefault(c => c.Id == x.Id) is null).ToList();
        var updateConfigItems = configItems.Where(x => resource.ConfigSets.FirstOrDefault(c => c.Id == x.Id) is not null).ToList();
        var deleteConfigItems = resource.ConfigSets.Where(x => configItems.FirstOrDefault(c => c.Id == x.Id) is null).ToList();
        resource.ConfigSets = configItems;

        AddOrUpdateConfiguration(_createdConfigItems, newConfigItems);
        AddOrUpdateConfiguration(_updatedConfigItems, updateConfigItems);
        AddOrUpdateConfiguration(_deletedConfigItems, deleteConfigItems);
        Snackbar.Add("Configuration file updated, changes won't be made until you save", Severity.Warning);
        StateHasChanged();
    }

    private static void AddOrUpdateConfiguration(List<ConfigurationItemSlim> existingItems, List<ConfigurationItemSlim> updatedItems)
    {
        foreach (var updatedItem in updatedItems)
        {
            var matchingItem = existingItems.FirstOrDefault(x => x.Id == updatedItem.Id);
            if (matchingItem is null)
            {
                existingItems.Add(updatedItem);
                continue;
            }

            matchingItem.FriendlyName = updatedItem.FriendlyName;
            matchingItem.Key = updatedItem.Key;
            matchingItem.Value = updatedItem.Value;
            matchingItem.Category = updatedItem.Category;
            matchingItem.Path = updatedItem.Path;
            matchingItem.DuplicateKey = updatedItem.DuplicateKey;
        }
    }

    private async Task ConfigSelectedForImport(IReadOnlyList<IBrowserFile?>? importFiles)
    {
        if (importFiles is null || !importFiles.Any())
        {
            return;
        }

        List<IBrowserFile> configToImport = [];
        foreach (var file in importFiles)
        {
            if (file is null)
            {
                continue;
            }

            if (file.Size > 10_000_000)
            {
                Snackbar.Add($"File is over the max size of 10MB: {file.Name}");
                continue;
            }

            configToImport.Add(file);
        }

        var confirmText = $"Are you sure you want to import these {configToImport.Count} files?{Environment.NewLine}" +
                          $"File paths can't be assumed so each will need to be updated manually before you save";
        var importConfirmation = await DialogService.ConfirmDialog($"Import {configToImport.Count} config files", confirmText);
        if (importConfirmation.Canceled)
        {
            return;
        }

        await ImportConfigFiles(configToImport);
    }

    private async Task ImportConfigFiles(IEnumerable<IBrowserFile> importFiles)
    {
        var importCount = 0;

        foreach (var file in importFiles)
        {
            var isNewConfigFile = true;
            var matchingResource = _localResources.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.PathWindows) && x.PathWindows.ToLower().EndsWith(file.Name.ToLower()) ||
                !string.IsNullOrWhiteSpace(x.PathLinux) && x.PathLinux.ToLower().EndsWith(file.Name.ToLower()) ||
                !string.IsNullOrWhiteSpace(x.PathMac) && x.PathMac.ToLower().EndsWith(file.Name.ToLower()));
            if (matchingResource is not null)
            {
                var confirmText = $"The existing resource {matchingResource.Name} will be replaced if you import this file: {file.Name}, are you sure you want to continue?";
                var replaceConfirmation = await DialogService.ConfirmDialog("Replace existing config file?", confirmText);
                if (replaceConfirmation.Canceled)
                {
                    Snackbar.Add($"File {file.Name} import was cancelled", Severity.Warning);
                    continue;
                }

                isNewConfigFile = false;
            }

            if (isNewConfigFile)
            {
                matchingResource = new LocalResourceSlim
                {
                    Id = Guid.CreateVersion7(),
                    GameProfileId = _gameServer.GameProfileId,
                    Name = file.Name.Split('.').First(),
                    PathWindows = _game.SupportsWindows ? file.Name : string.Empty,
                    PathLinux = _game.SupportsLinux ? file.Name : string.Empty,
                    PathMac = _game.SupportsMac ? file.Name : string.Empty,
                    Startup = false,
                    StartupPriority = 0,
                    Type = ResourceType.ConfigFile,
                    ContentType = FileHelpers.GetContentTypeFromName(file.Name),
                    Args = string.Empty,
                    LoadExisting = false
                };
            }

            var fileContent = await file.GetContent(); // The max import size per file is 10MB by default
            if (!fileContent.Succeeded || fileContent.Data is null)
            {
                fileContent.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                continue;
            }

            var fileContentLines = fileContent.Data.Split(Environment.NewLine);
            List<ConfigurationItemSlim> configItems;
            try
            {
                configItems = matchingResource!.ContentType switch
                {
                    ContentType.Raw => fileContentLines.ToConfigItems(matchingResource.Id),
                    ContentType.Ini => new IniData(fileContentLines).ToConfigItems(matchingResource.Id),
                    ContentType.Json => SerializerService.DeserializeJson<List<ConfigurationItemExport>>(fileContent.Data).ToConfigItems(matchingResource.Id),
                    ContentType.Xml => XDocument.Parse(fileContent.Data).ToConfigItems(matchingResource.Id),
                    _ => fileContentLines.ToConfigItems(matchingResource.Id)
                };
            }
            catch (Exception)
            {
                Snackbar.Add($"File {file.Name} import failed due to being in an unsupported or corrupt format", Severity.Error);
                continue;
            }

            matchingResource.ConfigSets = matchingResource.ConfigSets.MergeConfiguration(configItems, true);
            foreach (var configItem in configItems)
            {
                // Set any config items not merged into existing items to be created
                if (matchingResource.ConfigSets.FirstOrDefault(x => x.Id == configItem.Id) is not null)
                {
                    _createdConfigItems.Add(configItem);
                    continue;
                }

                // Any config items where the id doesn't exist are updated
                var updatedConfigItem = matchingResource.ConfigSets.FirstOrDefault(x =>
                    (x.DuplicateKey && x.Category == configItem.Category && x.Key == configItem.Key && x.Path == configItem.Path && x.Value == configItem.Value) ||
                    (x.Category == configItem.Category && x.Key == configItem.Key && x.Path == configItem.Path));
                if (updatedConfigItem is null)
                {
                    continue;
                }

                _updatedConfigItems.Add(updatedConfigItem);
            }

            importCount++;

            if (isNewConfigFile)
            {
                _createdLocalResources.Add(matchingResource);
                _localResources.Add(matchingResource);
                continue;
            }

            _updatedLocalResources.Add(matchingResource);
        }

        if (importCount <= 0)
        {
            Snackbar.Add("No files were imported", Severity.Info);
            return;
        }

        Snackbar.Add($"Successfully imported {importCount} configuration file(s), changes won't be made until you save", Severity.Success);
    }

    private async Task ExportServerConfig()
    {
        var profileExport = new GameProfileExport
        {
            Name = $"{_gameServer.ServerName} Profile",
            GameId = _game.SourceType is GameSource.Steam ? _game.SteamToolId.ToString() : _game.FriendlyName,
            AllowAutoDelete = true,
            Resources = []
        };
        foreach (var resource in _localResources)
        {
            var resourceExport = resource.ToExport();
            resourceExport.Configuration = resource.ConfigSets.Select(x => x.ToExport()).ToList();
            profileExport.Resources.Add(resourceExport);
        }

        var serializedProfile = SerializerService.SerializeJson(profileExport);
        var profileExportName = $"{FileHelpers.SanitizeSecureFilename(profileExport.Name)}.json";
        await WebClientService.InvokeFileDownload(serializedProfile, profileExportName, DataConstants.MimeTypes.Json);

        Snackbar.Add($"Successfully Exported Configuration: {profileExportName}");
    }

    private async Task<IEnumerable<GameProfileSlim>> FilterProfiles(string filterText, CancellationToken token)
    {
        if (_availableParentProfiles is null)
        {
            var response = await GameServerService.GetGameProfilesByGameIdAsync(_game.Id);
            if (!response.Succeeded)
            {
                response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                _availableParentProfiles = [];
                return [];
            }

            _availableParentProfiles = response.Data.ToList();
        }

        if (string.IsNullOrWhiteSpace(filterText) || filterText.Length < 3 || _game.Id == Guid.Empty)
        {
            return _availableParentProfiles?.Take(100) ?? [];
        }

        // The default game profile for each game is always inherited from, and we shouldn't be able to assign to the server profile, so we won't allow double inheritance
        return _availableParentProfiles?.Where(x => x.Id != _game.DefaultGameProfileId && x.Id != _gameServer.GameProfileId).ToList() ?? [];
    }

    private async Task<TableData<NotifyRecordSlim>> ServerEventsReload(TableState state, CancellationToken token)
    {
        var recordResponse = await NotifyRecordService.SearchPaginatedByEntityIdAsync(_gameServer.Id, _notifySearchText, state.Page + 1, state.PageSize);
        if (!recordResponse.Succeeded)
        {
            recordResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return new TableData<NotifyRecordSlim>();
        }

        var notifyRecords = recordResponse.Data.ToArray();
        _notifyPagedData = notifyRecords;
        _totalNotifyRecords = recordResponse.TotalCount;

        _notifyPagedData = state.SortLabel switch
        {
            "Timestamp" => _notifyPagedData.OrderByDirection(state.SortDirection, o => o.Timestamp),
            "Message" => _notifyPagedData.OrderByDirection(state.SortDirection, o => o.Message),
            _ => _notifyPagedData
        };

        return new TableData<NotifyRecordSlim> {TotalItems = _totalNotifyRecords, Items = _notifyPagedData};
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

        _gameServer.ServerState = args.ServerState ?? _gameServer.ServerState;
        _gameServer.RunningConfigHash = args.RunningConfigHash;
        _gameServer.StorageConfigHash = args.StorageConfigHash;

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
}