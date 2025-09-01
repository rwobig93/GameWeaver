using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.GameServer;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.LocalResource;
using Application.Models.Identity.User;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Enums.GameServer;
using Domain.Enums.Lifecycle;
using Domain.Models.Identity;
using GameWeaver.Helpers;
using Microsoft.AspNetCore.Components.Forms;

namespace GameWeaver.Pages.Admin;

public partial class AdminPanel : ComponentBase
{
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IGameService GameService { get; init; } = null!;
    [Inject] private IGameServerService GameServerService { get; init; } = null!;
    [Inject] private IHostService HostService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;
    [Inject] private ISerializerService SerializerService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private ITroubleshootingRecordService TshootService { get; init; } = null!;
    [Inject] private IAppRoleService RoleService { get; init; } = null!;

    private AppUserFull CurrentUser { get; set; } = new();
    private AppUserPreferenceFull _userPreferences = new();

    private bool _loadedData;
    private int _gameCount;
    private int _gameProfileCount;
    private int _gameServerCount;
    private int _hostCount;
    private int _userCount;
    private bool _backupRestoreExpanded;
    private bool _configurationExpanded;
    private int _importProgressCurrent;
    private int _importProgressTotal;
    private string _importCurrentContext = string.Empty;

    private bool _canGetGameCount;
    private bool _canGetGameProfileCount;
    private bool _canGetGameServerCount;
    private bool _canGetHostCount;
    private bool _canGetUserCount;
    private bool _isAdmin;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetCurrentUser();
            await GetPermissions();
            await GetCounts();
        }
    }

    private async Task GetCurrentUser()
    {
        var userId = await CurrentUserService.GetCurrentUserId();
        if (userId is null)
            return;

        CurrentUser = (await UserService.GetByIdFullAsync((Guid) userId)).Data!;
        _userPreferences = (await AccountService.GetPreferences(CurrentUser.Id)).Data;
        StateHasChanged();
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canGetUserCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Users.View);
        _canGetHostCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.Get);
        _canGetGameCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Get);
        _canGetGameProfileCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.GameProfile.Get);
        _canGetGameServerCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get);
        _isAdmin = (await RoleService.IsUserAdminAsync(CurrentUser.Id)).Data;
        StateHasChanged();
    }

    private async Task GetCounts()
    {
        _loadedData = false;
        StateHasChanged();

        if (_canGetUserCount)
        {
            _userCount = (await UserService.GetCountAsync()).Data;
        }

        if (_canGetHostCount)
        {
            _hostCount = (await HostService.GetCountAsync()).Data;
        }

        if (_canGetGameCount)
        {
            _gameCount = (await GameService.GetCountAsync()).Data;
        }

        if (_canGetGameProfileCount)
        {
            _gameProfileCount = (await GameServerService.GetGameProfileCountAsync()).Data;
        }

        if (_canGetGameServerCount)
        {
            _gameServerCount = (await GameServerService.GetCountAsync()).Data;
        }

        _loadedData = true;
        StateHasChanged();
    }

    private async Task ExportGameProfiles()
    {
        if (!_canGetGameProfileCount)
        {
            return;
        }

        var allGameProfilesRequest = await GameServerService.GetAllGameProfilesAsync();
        if (!allGameProfilesRequest.Succeeded)
        {
            allGameProfilesRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        var allGamesRequest = await GameService.GetAllAsync();
        if (!allGamesRequest.Succeeded)
        {
            allGamesRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        var profileBackupExport = new GameProfilesBackup
        {
            InstanceName = ServerState.ApplicationName,
            InstanceVersion = ServerState.ApplicationVersion.ToString(),
            ExportedTimestampUtc = DateTimeService.NowDatabaseTime,
            GameProfiles = []
        };

        var troubleshootRecordId = Guid.Empty;
        foreach (var profile in allGameProfilesRequest.Data)
        {
            var matchingGame = allGamesRequest.Data.FirstOrDefault(x => x.Id == profile.GameId);
            var profileExport = profile.ToExport(matchingGame!.SourceType is GameSource.Steam ? matchingGame.SteamToolId.ToString() : matchingGame.FriendlyName);
            var profileResourcesRequest = await GameServerService.GetLocalResourcesByGameProfileIdAsync(profile.Id);
            if (!profileResourcesRequest.Succeeded)
            {
                if (troubleshootRecordId == Guid.Empty)
                {
                    troubleshootRecordId = Guid.CreateVersion7();
                }

                var tshootDetails = new Dictionary<string, string>
                {
                    {"ProfileId", profile.Id.ToString()},
                    {"ProfileName", profile.FriendlyName},
                    {"GameId", matchingGame.Id.ToString()},
                    {"GameName", matchingGame.FriendlyName},
                    {"Error", "Was unable to get the resources for the game profile"}
                };
                for (var i = 0; i < profileResourcesRequest.Messages.Count; i++)
                {
                    tshootDetails.Add($"Error{i + 1}", profileResourcesRequest.Messages[i]);
                }

                await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Backup, troubleshootRecordId, CurrentUser.Id,
                    "Failed to export game profile to backup export", tshootDetails);
                continue;
            }

            foreach (var resource in profileResourcesRequest.Data)
            {
                var configItemsRequest = await GameServerService.GetConfigurationItemsByLocalResourceIdAsync(resource.Id);
                if (!configItemsRequest.Succeeded)
                {
                    if (troubleshootRecordId == Guid.Empty)
                    {
                        troubleshootRecordId = Guid.CreateVersion7();
                    }

                    var tshootDetails = new Dictionary<string, string>
                    {
                        {"ProfileId", profile.Id.ToString()},
                        {"ProfileName", profile.FriendlyName},
                        {"GameId", matchingGame.Id.ToString()},
                        {"GameName", matchingGame.FriendlyName},
                        {"ResourceId", resource.Id.ToString()},
                        {"ResourceName", resource.Name},
                        {"ResourcePathWindows", resource.PathWindows},
                        {"ResourcePathLinux", resource.PathLinux},
                        {"ResourcePathMac", resource.PathMac},
                        {"ResourceType", resource.Type.ToString()},
                        {"ResourceContentType", resource.ContentType.ToString()},
                        {"Error", "Was unable to get config items for the resource of the game profile"}
                    };
                    for (var i = 0; i < configItemsRequest.Messages.Count; i++)
                    {
                        tshootDetails.Add($"Error{i + 1}", configItemsRequest.Messages[i]);
                    }

                    await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Backup, troubleshootRecordId, CurrentUser.Id,
                        "Failed to export game profile to backup export", tshootDetails);
                    continue;
                }

                var resourceExport = resource.ToExport();
                resourceExport.Configuration = configItemsRequest.Data.Select(x => x.ToExport()).ToList();
                profileExport.Resources.Add(resourceExport);
            }

            profileBackupExport.GameProfiles.Add(profileExport);
        }

        var serializedBackup = SerializerService.SerializeJson(profileBackupExport);
        var backupExportName = $"{FileHelpers.SanitizeSecureFilename(ServerState.ApplicationName)}_All_Game_Profiles_{DateTimeService.NowDatabaseTime.ToFileTimestamp()}.json";
        await WebClientService.InvokeFileDownload(serializedBackup, backupExportName, DataConstants.MimeTypes.Json);

        if (troubleshootRecordId != Guid.Empty)
        {
            Snackbar.Add($"Failed to export all game profiles due to errors, please see backup troubleshooting record(s) for entity id: {troubleshootRecordId}",
                Severity.Error);
            return;
        }

        Snackbar.Add($"Successfully Exported All Game Profiles: {backupExportName}");
    }

    private async Task GameProfilesSelectedForImport(IReadOnlyList<IBrowserFile?>? importFiles)
    {
        if (_importProgressTotal != 0)
        {
            Snackbar.Add("Another import is already in progress, please wait for it to finish before importing another file", Severity.Error);
            return;
        }

        if (importFiles is null || !importFiles.Any())
        {
            return;
        }

        var profileToImport = importFiles[0];
        if (profileToImport is null)
        {
            return;
        }

        if (profileToImport.Size > 100_000_000)
        {
            Snackbar.Add($"File is over the max size of 100MB: {profileToImport.Name}");
            return;
        }

        var confirmText = $"Are you sure you want to import these game profiles?{Environment.NewLine}" +
                          $"Any existing profiles with the same name will be overwritten";
        var importConfirmation = await DialogService.ConfirmDialog("Import game profiles", confirmText);
        if (importConfirmation.Canceled)
        {
            return;
        }

        await ImportGameProfiles(profileToImport!);
    }

    private async Task ImportGameProfiles(IBrowserFile importFile)
    {
        _importProgressCurrent = 0;
        _importProgressTotal = 100;
        StateHasChanged();

        var fileContents = await importFile.GetContent();
        if (!fileContents.Succeeded || fileContents.Data is null)
        {
            fileContents.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        GameProfilesBackup deserializedBackup;
        try
        {
            deserializedBackup = SerializerService.DeserializeJson<GameProfilesBackup>(fileContents.Data);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Game profile backup import failed: [{FileName}]{Error}", importFile.Name, ex.Message);
            Snackbar.Add($"Failed to import game profiles from {importFile.Name} due to being in an unsupported or corrupt format", Severity.Error);
            _importProgressTotal = 100;
            StateHasChanged();
            return;
        }

        _importProgressCurrent = 0;
        _importProgressTotal = deserializedBackup.GameProfiles.Count;
        StateHasChanged();

        var allGamesRequest = await GameService.GetAllAsync();
        if (!allGamesRequest.Succeeded)
        {
            allGamesRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        var troubleshootRecordId = Guid.Empty;

        foreach (var profile in deserializedBackup.GameProfiles)
        {
            _importCurrentContext = profile.Name;
            _importProgressCurrent++;
            StateHasChanged();

            var gameIdIsSteam = int.TryParse(profile.GameId, out var steamId);
            var matchingGame = gameIdIsSteam
                ? allGamesRequest.Data.FirstOrDefault(x => x.SteamToolId == steamId)
                : allGamesRequest.Data.FirstOrDefault(x => x.FriendlyName == profile.GameId);

            if (matchingGame is null)
            {
                if (troubleshootRecordId == Guid.Empty)
                {
                    troubleshootRecordId = Guid.CreateVersion7();
                }

                await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Restore, troubleshootRecordId, CurrentUser.Id,
                    "Failed to import game profile from backup export", new Dictionary<string, string>
                    {
                        {"ExportInstanceName", deserializedBackup.InstanceName},
                        {"ExportInstanceVersion", deserializedBackup.InstanceVersion},
                        {"ExportTimestampUtc", deserializedBackup.ExportedTimestampUtc.ToFriendlyDisplay()},
                        {"ProfileName", profile.Name},
                        {"ProfileGameId", profile.GameId},
                        {"ProfileResourceCount", profile.Resources.Count.ToString()},
                        {"Error", "No matching game could be found for the game id on the game profile"}
                    });
                continue;
            }

            var matchingProfile = await GameServerService.GetGameProfileByFriendlyNameAsync(profile.Name);
            if (matchingProfile.Data is null)
            {
                if (profile.HasInvalidName())
                {
                    continue;
                }

                var createProfileRequest = await GameServerService.CreateGameProfileAsync(profile.ToCreateRequest(CurrentUser.Id, matchingGame.Id), CurrentUser.Id);
                if (!createProfileRequest.Succeeded)
                {
                    if (troubleshootRecordId == Guid.Empty)
                    {
                        troubleshootRecordId = Guid.CreateVersion7();
                    }

                    var tshootDetails = new Dictionary<string, string>
                    {
                        {"ExportInstanceName", deserializedBackup.InstanceName},
                        {"ExportInstanceVersion", deserializedBackup.InstanceVersion},
                        {"ExportTimestampUtc", deserializedBackup.ExportedTimestampUtc.ToFriendlyDisplay()},
                        {"ProfileName", profile.Name},
                        {"ProfileGameId", profile.GameId},
                        {"ProfileResourceCount", profile.Resources.Count.ToString()},
                        {"Error", "Failed to create new profile for non-existent game profile"}
                    };
                    for (var i = 0; i < createProfileRequest.Messages.Count; i++)
                    {
                        tshootDetails.Add($"Error{i + 1}", createProfileRequest.Messages[i]);
                    }

                    await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Restore, troubleshootRecordId, CurrentUser.Id,
                        "Failed to import game profile from backup export", tshootDetails);
                    continue;
                }

                matchingProfile = await GameServerService.GetGameProfileByIdAsync(createProfileRequest.Data);
            }
            else
            {
                var updateProfileRequest = await GameServerService.UpdateGameProfileAsync(profile.ToUpdateRequest(matchingProfile.Data.Id), CurrentUser.Id);
                if (!updateProfileRequest.Succeeded)
                {
                    if (troubleshootRecordId == Guid.Empty)
                    {
                        troubleshootRecordId = Guid.CreateVersion7();
                    }

                    var tshootDetails = new Dictionary<string, string>
                    {
                        {"ExportInstanceName", deserializedBackup.InstanceName},
                        {"ExportInstanceVersion", deserializedBackup.InstanceVersion},
                        {"ExportTimestampUtc", deserializedBackup.ExportedTimestampUtc.ToFriendlyDisplay()},
                        {"ProfileId", matchingProfile.Data.Id.ToString()},
                        {"ProfileName", profile.Name},
                        {"ProfileGameId", profile.GameId},
                        {"ProfileResourceCount", profile.Resources.Count.ToString()},
                        {"Error", "Failed to update existing game profile"}
                    };
                    for (var i = 0; i < updateProfileRequest.Messages.Count; i++)
                    {
                        tshootDetails.Add($"Error{i + 1}", updateProfileRequest.Messages[i]);
                    }

                    await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Restore, troubleshootRecordId, CurrentUser.Id,
                        "Failed to import game profile from backup export", tshootDetails);
                    continue;
                }
            }

            await ImportConfigResources(matchingProfile.Data!.Id, profile.Resources, troubleshootRecordId);
        }

        _importProgressCurrent = 0;
        _importProgressTotal = 0;
        StateHasChanged();

        if (troubleshootRecordId != Guid.Empty)
        {
            Snackbar.Add($"Failed to import all game profiles due to errors, please see restore troubleshooting record(s) for entity id: {troubleshootRecordId}",
                Severity.Error);
            return;
        }

        Snackbar.Add($"Finished game profile backup import for {deserializedBackup.GameProfiles.Count} profiles", Severity.Success);
    }

    private async Task ImportConfigResources(Guid profileId, List<LocalResourceExport> importResources, Guid troubleshootRecordId)
    {
        var existingResourcesRequest = await GameServerService.GetLocalResourcesByGameProfileIdAsync(profileId);
        if (!existingResourcesRequest.Succeeded)
        {
            if (troubleshootRecordId == Guid.Empty)
            {
                troubleshootRecordId = Guid.CreateVersion7();
            }

            var tshootDetails = new Dictionary<string, string>
            {
                {"ProfileId", profileId.ToString()},
                {"ProfileResourceCount", importResources.Count.ToString()},
                {"Error", "Failed to get resources for game profile to import resources"}
            };
            for (var i = 0; i < existingResourcesRequest.Messages.Count; i++)
            {
                tshootDetails.Add($"Error{i + 1}", existingResourcesRequest.Messages[i]);
            }

            await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Restore, troubleshootRecordId, CurrentUser.Id,
                "Failed to import config items for game profile from backup export", tshootDetails);
            return;
        }

        foreach (var resource in importResources)
        {
            var matchingResource = existingResourcesRequest.Data.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.PathWindows) && x.PathWindows.ToLower().EndsWith(resource.Name.ToLower()) ||
                !string.IsNullOrWhiteSpace(x.PathLinux) && x.PathLinux.ToLower().EndsWith(resource.Name.ToLower()) ||
                !string.IsNullOrWhiteSpace(x.PathMac) && x.PathMac.ToLower().EndsWith(resource.Name.ToLower()));

            // Create the resource if it doesn't exist
            if (matchingResource is null)
            {
                var createResourceRequest = await GameServerService.CreateLocalResourceAsync(resource.ToCreate(), CurrentUser.Id);
                if (!createResourceRequest.Succeeded)
                {
                    if (troubleshootRecordId == Guid.Empty)
                    {
                        troubleshootRecordId = Guid.CreateVersion7();
                    }

                    var tshootDetails = new Dictionary<string, string>
                    {
                        {"ProfileId", profileId.ToString()},
                        {"ResourceName", resource.Name},
                        {"ResourcePathWindows", resource.PathWindows},
                        {"ResourcePathLinux", resource.PathLinux},
                        {"ResourcePathMac", resource.PathMac},
                        {"ResourceType", resource.Type.ToString()},
                        {"ResourceContentType", resource.ContentType.ToString()},
                        {"ResourceConfigCount", resource.Configuration.Count.ToString()},
                        {"Error", "Failed to create resource for game profile to import resources"}
                    };
                    for (var i = 0; i < createResourceRequest.Messages.Count; i++)
                    {
                        tshootDetails.Add($"Error{i + 1}", createResourceRequest.Messages[i]);
                    }

                    await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Restore, troubleshootRecordId, CurrentUser.Id,
                        "Failed to import local resource for game profile from backup export", tshootDetails);
                    continue;
                }

                var createdResourceRequest = await GameServerService.GetLocalResourceByIdAsync(createResourceRequest.Data);
                if (!createdResourceRequest.Succeeded)
                {
                    if (troubleshootRecordId == Guid.Empty)
                    {
                        troubleshootRecordId = Guid.CreateVersion7();
                    }

                    var tshootDetails = new Dictionary<string, string>
                    {
                        {"ProfileId", profileId.ToString()},
                        {"CreatedResourceId", createResourceRequest.Data.ToString()},
                        {"ResourceName", resource.Name},
                        {"ResourcePathWindows", resource.PathWindows},
                        {"ResourcePathLinux", resource.PathLinux},
                        {"ResourcePathMac", resource.PathMac},
                        {"ResourceType", resource.Type.ToString()},
                        {"ResourceContentType", resource.ContentType.ToString()},
                        {"ResourceConfigCount", resource.Configuration.Count.ToString()},
                        {"Error", "Failed to get created resource for game profile to import resources"}
                    };
                    for (var i = 0; i < createdResourceRequest.Messages.Count; i++)
                    {
                        tshootDetails.Add($"Error{i + 1}", createdResourceRequest.Messages[i]);
                    }

                    await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Restore, troubleshootRecordId, CurrentUser.Id,
                        "Failed to import local resource for game profile from backup export", tshootDetails);
                    continue;
                }

                matchingResource = createdResourceRequest.Data;
            }

            // Create or update all config items from the resource import
            var importConfigItems = resource.Configuration.Select(x => x.ToSlim()).ToList();
            matchingResource.ConfigSets = matchingResource.ConfigSets.MergeConfiguration(importConfigItems, true);
            foreach (var configItem in importConfigItems)
            {
                // Set any config items not merged into existing items to be created
                if (matchingResource.ConfigSets.FirstOrDefault(x => x.Id == configItem.Id) is not null)
                {
                    var createConfigItemRequest = await GameServerService.CreateConfigurationItemAsync(configItem.ToCreate(), CurrentUser.Id);
                    if (!createConfigItemRequest.Succeeded)
                    {
                        if (troubleshootRecordId == Guid.Empty)
                        {
                            troubleshootRecordId = Guid.CreateVersion7();
                        }

                        var tshootDetails = new Dictionary<string, string>
                        {
                            {"ProfileId", profileId.ToString()},
                            {"ResourceName", resource.Name},
                            {"ResourcePathWindows", resource.PathWindows},
                            {"ResourcePathLinux", resource.PathLinux},
                            {"ResourcePathMac", resource.PathMac},
                            {"ResourceType", resource.Type.ToString()},
                            {"ResourceContentType", resource.ContentType.ToString()},
                            {"ResourceConfigCount", resource.Configuration.Count.ToString()},
                            {"ConfigItemPath", configItem.Path},
                            {"ConfigItemKey", configItem.Key},
                            {"ConfigItemValue", configItem.Value},
                            {"ConfigItemCategory", configItem.Category},
                            {"ConfigItemDuplicateKey", configItem.DuplicateKey.ToString()},
                            {"Error", "Failed to create config item on resource for game profile to import resources"}
                        };
                        for (var i = 0; i < createConfigItemRequest.Messages.Count; i++)
                        {
                            tshootDetails.Add($"Error{i + 1}", createConfigItemRequest.Messages[i]);
                        }

                        await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Restore, troubleshootRecordId, CurrentUser.Id,
                            "Failed to import config item on local resource for game profile from backup export", tshootDetails);
                    }

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

                var updateConfigItemRequest = await GameServerService.UpdateConfigurationItemAsync(updatedConfigItem.ToUpdate(), CurrentUser.Id);
                if (updateConfigItemRequest.Succeeded) continue;
                {
                    if (troubleshootRecordId == Guid.Empty)
                    {
                        troubleshootRecordId = Guid.CreateVersion7();
                    }

                    var tshootDetails = new Dictionary<string, string>
                    {
                        {"ProfileId", profileId.ToString()},
                        {"ResourceName", resource.Name},
                        {"ResourcePathWindows", resource.PathWindows},
                        {"ResourcePathLinux", resource.PathLinux},
                        {"ResourcePathMac", resource.PathMac},
                        {"ResourceType", resource.Type.ToString()},
                        {"ResourceContentType", resource.ContentType.ToString()},
                        {"ResourceConfigCount", resource.Configuration.Count.ToString()},
                        {"ConfigItemId", updatedConfigItem.Id.ToString()},
                        {"ConfigItemPath", configItem.Path},
                        {"ConfigItemKey", configItem.Key},
                        {"ConfigItemValue", configItem.Value},
                        {"ConfigItemCategory", configItem.Category},
                        {"ConfigItemDuplicateKey", configItem.DuplicateKey.ToString()},
                        {"Error", "Failed to update config item on resource for game profile to import resources"}
                    };
                    for (var i = 0; i < updateConfigItemRequest.Messages.Count; i++)
                    {
                        tshootDetails.Add($"Error{i + 1}", updateConfigItemRequest.Messages[i]);
                    }

                    await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Restore, troubleshootRecordId, CurrentUser.Id,
                        "Failed to import config item on local resource for game profile from backup export", tshootDetails);
                    return;
                }
            }
        }
    }
}