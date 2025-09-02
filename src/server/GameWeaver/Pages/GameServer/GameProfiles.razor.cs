using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.GameServer;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Requests.GameServer.GameProfile;
using Application.Services.GameServer;
using Domain.Models.Identity;
using GameWeaver.Helpers;
using Microsoft.AspNetCore.Components.Forms;

namespace GameWeaver.Pages.GameServer;

public partial class GameProfiles : ComponentBase
{
    [Inject] private IGameServerService GameServerService { get; init; } = null!;
    [Inject] private IGameService GameService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private ISerializerService SerializerService { get; init; } = null!;

    private IEnumerable<GameProfileSlim> _pagedData = [];
    private AppUserPreferenceFull _userPreferences = new();

    private Guid _loggedInUserId = Guid.Empty;
    private string _searchText = "";
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = PaginationHelpers.GetPageSizes(true).First();
    private int _currentPage = 1;
    private string _importCurrentContext = string.Empty;
    private int _importProgressCurrent;
    private int _importProgressTotal;

    private bool _canCreateProfiles;
    private bool _canImportProfiles;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await GetUserPreferences();
            await RefreshData();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canCreateProfiles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.GameProfile.Create);
        _canImportProfiles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.GameProfile.Import);
    }

    private async Task GetUserPreferences()
    {
        var currentUserId = await CurrentUserService.GetCurrentUserId();
        if (currentUserId is null)
        {
            return;
        }

        _userPreferences = (await AccountService.GetPreferences(currentUserId.GetFromNullable())).Data;
    }

    private async Task RefreshData()
    {
        _pagedData = [];
        StateHasChanged();

        var response = await GameServerService.SearchGameProfilesPaginatedAsync(_searchText, _currentPage, _pageSize);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _pagedData = response.Data;
        _totalItems = response.TotalCount;
        _totalPages = response.EndPage;
        GetCurrentPageViewData();
        StateHasChanged();
    }

    private string GetCurrentPageViewData()
    {
        var currentStart = 1;
        var currentEnd = _pageSize * _currentPage;

        if (_totalItems == 0)
        {
            currentStart = 0;
        }

        if (_currentPage > 1)
        {
            currentStart += (_currentPage - 1) * _pageSize;
        }

        if (_pageSize * _currentPage > _totalItems)
            currentEnd = _totalItems;

        return $"{currentStart}-{currentEnd} of {_totalItems}";
    }

    private async Task PageChanged(int pageNumber)
    {
        _currentPage = pageNumber;
        await RefreshData();
    }

    private async Task CreateProfile()
    {
        if (!_canCreateProfiles)
        {
            return;
        }

        var dialogResult = await DialogService.CreateGameProfileDialog();
        if (dialogResult.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var createdProfileId = (Guid) dialogResult.Data;
        Snackbar.Add("Successfully created new configuration profile!", Severity.Success);

        var newProfileResponse = await GameServerService.GetGameProfileByIdAsync(createdProfileId);
        if (!newProfileResponse.Succeeded || newProfileResponse.Data is null)
        {
            newProfileResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        // We'll search for the newly created profile after it's created
        _searchText = newProfileResponse.Data.FriendlyName;
        await RefreshData();
    }

    private async Task ProfilesSelectedForImport(IReadOnlyList<IBrowserFile?>? importFiles)
    {
        if (!_canImportProfiles)
        {
            return;
        }

        if (importFiles is null || !importFiles.Any())
        {
            return;
        }

        List<IBrowserFile> filesToImport = [];
        foreach (var file in importFiles)
        {
            if (file is null)
            {
                continue;
            }

            if (file.Size > 10_000_000)
            {
                Snackbar.Add($"File is over the max size of 10MB: {file.Name}", Severity.Error);
                continue;
            }

            filesToImport.Add(file);
        }

        var confirmText = $"Are you sure you want to import these {filesToImport.Count} game profiles?{Environment.NewLine}" +
                          $"Any existing game profiles with the same name will be overwritten";
        var importConfirmation = await DialogService.ConfirmDialog($"Import {filesToImport.Count} game profiles", confirmText);
        if (importConfirmation.Canceled)
        {
            return;
        }

        Snackbar.Add("Importing files, this may take a moment or two depending on file count and size...", Severity.Info);
        _importProgressCurrent = 0;
        _importProgressTotal = filesToImport.Count;
        StateHasChanged();

        foreach (var file in filesToImport)
        {
            _importCurrentContext = file.Name;
            _importProgressCurrent++;
            StateHasChanged();

            var fileContent = await file.GetContent(); // The max import size per file is 10MB by default
            if (!fileContent.Succeeded || fileContent.Data is null)
            {
                fileContent.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            GameProfileExport deserializedProfile;
            try
            {
                deserializedProfile = SerializerService.DeserializeJson<GameProfileExport>(fileContent.Data);
            }
            catch (Exception ex)
            {
                _importCurrentContext = string.Empty;
                _importProgressCurrent = 0;
                _importProgressTotal = 0;
                StateHasChanged();
                Logger.Error(ex, "Game profile import from file failed: [{FileName}]{Error}", file.Name, ex.Message);
                Snackbar.Add($"File {file.Name} import failed due to being in an unsupported or corrupt format", Severity.Error);
                return;
            }

            await ImportProfile(deserializedProfile);
        }

        Snackbar.Add($"Finished importing {filesToImport.Count} game profile(s)", Severity.Success);
        _importCurrentContext = string.Empty;
        _importProgressCurrent = 0;
        _importProgressTotal = 0;
        StateHasChanged();
    }

    private async Task ImportProfile(GameProfileExport profileExport)
    {
        if (!_canImportProfiles)
        {
            return;
        }

        var matchingProfileName = await GameServerService.GetGameProfileByFriendlyNameAsync(profileExport.Name);
        if (matchingProfileName.Data is null)
        {
            await CreateGameProfileResourcesAndConfig(profileExport);
            return;
        }

        var updateProfileRequest = await GameServerService.UpdateGameProfileAsync(profileExport.ToUpdateRequest(matchingProfileName.Data.Id), _loggedInUserId);
        if (!updateProfileRequest.Succeeded)
        {
            updateProfileRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        await UpdateExistingConfig(profileExport, matchingProfileName.Data.Id);
    }

    private async Task CreateGameProfileResourcesAndConfig(GameProfileExport profile)
    {
        if (!_canImportProfiles)
        {
            return;
        }

        GameSlim matchingGame;

        var gameIdIsSteam = int.TryParse(profile.GameId, out var steamId);
        if (gameIdIsSteam)
        {
            var steamGameResponse = await GameService.GetBySteamToolIdAsync(steamId);
            if (!steamGameResponse.Succeeded || steamGameResponse.Data is null)
            {
                steamGameResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            matchingGame = steamGameResponse.Data;
        }
        else
        {
            var manualGameResponse = await GameService.GetByFriendlyNameAsync(profile.GameId);
            if (!manualGameResponse.Succeeded || manualGameResponse.Data is null)
            {
                manualGameResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            matchingGame = manualGameResponse.Data;
        }

        var profileCreateRequest = profile.ToCreateRequest(_loggedInUserId, matchingGame.Id);
        var createProfileResponse = await GameServerService.CreateGameProfileAsync(profileCreateRequest, _loggedInUserId);
        if (!createProfileResponse.Succeeded)
        {
            createProfileResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (!profile.AllowAutoDelete)
        {
            var updateAutoDeleteResponse = await GameServerService.UpdateGameProfileAsync(new GameProfileUpdateRequest
                {Id = createProfileResponse.Data, AllowAutoDelete = profile.AllowAutoDelete}, _loggedInUserId);
            if (!updateAutoDeleteResponse.Succeeded)
            {
                updateAutoDeleteResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
        }

        foreach (var resource in profile.Resources)
        {
            var resourceCreateRequest = resource.ToCreate();
            resourceCreateRequest.GameProfileId = createProfileResponse.Data;
            var createResourceResponse = await GameServerService.CreateLocalResourceAsync(resourceCreateRequest, _loggedInUserId);
            if (!createResourceResponse.Succeeded)
            {
                createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            foreach (var configCreateRequest in resource.Configuration.Select(configItem => configItem.ToCreate()))
            {
                configCreateRequest.LocalResourceId = createResourceResponse.Data;
                var createConfigResponse = await GameServerService.CreateConfigurationItemAsync(configCreateRequest, _loggedInUserId);
                if (createConfigResponse.Succeeded) continue;

                createConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
        }
    }

    private async Task UpdateExistingConfig(GameProfileExport profileExport, Guid existingProfileId)
    {
        if (!_canImportProfiles)
        {
            return;
        }

        var existingResourcesRequest = await GameServerService.GetLocalResourcesByGameProfileIdAsync(existingProfileId);
        if (!existingResourcesRequest.Succeeded)
        {
            existingResourcesRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var resource in profileExport.Resources)
        {
            var matchingResource = existingResourcesRequest.Data.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.PathWindows) && string.Equals(x.PathWindows, resource.PathWindows, StringComparison.CurrentCultureIgnoreCase) ||
                !string.IsNullOrWhiteSpace(x.PathLinux) && string.Equals(x.PathLinux, resource.PathLinux, StringComparison.CurrentCultureIgnoreCase) ||
                !string.IsNullOrWhiteSpace(x.PathMac) && string.Equals(x.PathMac, resource.PathMac, StringComparison.CurrentCultureIgnoreCase));

            // Create the resource if it doesn't exist
            if (matchingResource is null)
            {
                var createResourceRequest = await GameServerService.CreateLocalResourceAsync(resource.ToCreate(), _loggedInUserId);
                if (!createResourceRequest.Succeeded)
                {
                    createResourceRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    continue;
                }

                var createdResourceRequest = await GameServerService.GetLocalResourceByIdAsync(createResourceRequest.Data);
                if (!createdResourceRequest.Succeeded)
                {
                    createdResourceRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
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
                configItem.LocalResourceId = matchingResource.Id;
                if (matchingResource.ConfigSets.FirstOrDefault(x => x.Id == configItem.Id) is not null)
                {
                    var createConfigItemRequest = await GameServerService.CreateConfigurationItemAsync(configItem.ToCreate(), _loggedInUserId);
                    if (!createConfigItemRequest.Succeeded)
                    {
                        createConfigItemRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                        return;
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

                configItem.Id = updatedConfigItem.Id;
                var updateConfigItemRequest = await GameServerService.UpdateConfigurationItemAsync(configItem.ToUpdate(), _loggedInUserId);
                if (updateConfigItemRequest.Succeeded) continue;

                updateConfigItemRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            }
        }
    }

    private async Task SearchKeyDown(KeyboardEventArgs keyArgs)
    {
        if (keyArgs.Code is "Enter" or "NumpadEnter")
        {
            await RefreshData();
        }
    }

    private async Task PageSizeChanged()
    {
        await RefreshData();
        await WebClientService.InvokeScrollToTop();
    }
}