using Application.Constants.Identity;
using Application.Helpers.Auth;
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

    private bool _canCreateProfiles;
    private bool _canDeleteProfiles;

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
        _canDeleteProfiles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.GameProfile.Delete);
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
        if (importFiles is null || !importFiles.Any())
        {
            return;
        }

        List<IBrowserFile> profilesToImport = [];
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

            profilesToImport.Add(file);
        }

        Snackbar.Add("Importing files, this may take a moment or two depending on file count and size...", Severity.Info);
        await ImportProfiles(profilesToImport);
    }

    private async Task ImportProfiles(IEnumerable<IBrowserFile> importFiles)
    {
        List<Guid> createdProfiles = [];
        List<GameProfileExport> overlappingProfiles = [];
        List<GameProfileSlim> existingOverlappingProfiles = [];
        List<GameProfileExport> profilesToCreate = [];
        List<string> errors = [];

        foreach (var file in importFiles)
        {
            var fileContent = await file.GetContent();  // The max import size per file is 10MB by default
            if (!fileContent.Succeeded || fileContent.Data is null)
            {
                errors.AddRange(fileContent.Messages);
                continue;
            }

            GameProfileExport deserializedProfile;
            try
            {
                deserializedProfile = SerializerService.DeserializeJson<GameProfileExport>(fileContent.Data);
            }
            catch (Exception)
            {
                errors.Add($"Failed to parse profile from file due to invalid data: {file.Name}");
                continue;
            }

            var matchingProfileName = await GameServerService.GetGameProfileByFriendlyNameAsync(deserializedProfile.Name);
            if (matchingProfileName.Data is not null)
            {
                overlappingProfiles.Add(deserializedProfile);
                existingOverlappingProfiles.Add(matchingProfileName.Data);
                continue;
            }

            profilesToCreate.Add(deserializedProfile);
        }

        var replaceOverlapping = false;
        if (overlappingProfiles.Count > 0)
        {
            var actionText = _canDeleteProfiles ? "do you want to replace them?" : "these will be skipped due to not having permission to delete them";
            var confirmText = _canDeleteProfiles ? "Replace" : "Ok";
            var cancelText = _canDeleteProfiles ? "Skip" : "Got it";
            var overlapContent = $"The following profiles overlap with existing profiles, {actionText}{Environment.NewLine}{Environment.NewLine}" +
                                 $"{string.Join(Environment.NewLine, overlappingProfiles.Select(x => x.Name))}";
            var replaceProfilesDialog = await DialogService.ConfirmDialog("Overlapping Profiles Found", overlapContent, confirmText, cancelText);
            if (_canDeleteProfiles && !replaceProfilesDialog.Canceled)
            {
                replaceOverlapping = true;
            }
        }

        if (replaceOverlapping)
        {
            foreach (var profile in existingOverlappingProfiles)
            {
                var deleteResponse = await GameServerService.DeleteGameProfileAsync(profile.Id, _loggedInUserId);
                if (deleteResponse.Succeeded) continue;

                errors.AddRange(deleteResponse.Messages);
            }
        }

        foreach (var profile in profilesToCreate)
        {
            await CreateGameProfileResourcesAndConfig(profile, errors, createdProfiles);
        }

        if (errors.Count > 0)
        {
            var errorContent = $"The following errors occurred while importing profiles:{Environment.NewLine}{Environment.NewLine}" +
                               $"{string.Join(Environment.NewLine, errors)}{Environment.NewLine}";
            await DialogService.MessageDialog("Profile Import Errors", errorContent);
        }

        if (createdProfiles.Count == 0)
        {
            Snackbar.Add($"No profiles were imported", Severity.Info);
            return;
        }

        Snackbar.Add($"Successfully imported {createdProfiles.Count} profile(s)", Severity.Success);
    }

    private async Task CreateGameProfileResourcesAndConfig(GameProfileExport profile, List<string> errors, List<Guid> createdProfiles)
    {
        var profileCreateRequest = profile.ToCreateRequest();
        GameSlim matchingGame;

        var gameIdIsSteam = int.TryParse(profile.GameId, out var steamId);
        if (gameIdIsSteam)
        {
            var steamGameResponse = await GameService.GetBySteamToolIdAsync(steamId);
            if (!steamGameResponse.Succeeded || steamGameResponse.Data is null)
            {
                errors.AddRange(steamGameResponse.Messages.Select(x => $"Profile '{profile.Name}': {x}"));
                return;
            }
            matchingGame = steamGameResponse.Data;
        }
        else
        {
            var manualGameResponse = await GameService.GetByFriendlyNameAsync(profile.GameId);
            if (!manualGameResponse.Succeeded || manualGameResponse.Data is null)
            {
                errors.AddRange(manualGameResponse.Messages.Select(x => $"Profile '{profile.Name}': {x}"));
                return;
            }
            matchingGame = manualGameResponse.Data;
        }

        profileCreateRequest.GameId = matchingGame.Id;
        profileCreateRequest.OwnerId = _loggedInUserId;
        var createProfileResponse = await GameServerService.CreateGameProfileAsync(profileCreateRequest, _loggedInUserId);
        if (!createProfileResponse.Succeeded)
        {
            errors.AddRange(createProfileResponse.Messages.Select(x => $"Profile '{profile.Name}': {x}"));
            return;
        }

        if (!profile.AllowAutoDelete)
        {
            var updateAutoDeleteResponse = await GameServerService.UpdateGameProfileAsync(new GameProfileUpdateRequest
            { Id = createProfileResponse.Data, AllowAutoDelete = profile.AllowAutoDelete }, _loggedInUserId);
            if (!updateAutoDeleteResponse.Succeeded)
            {
                errors.AddRange(updateAutoDeleteResponse.Messages.Select(x => $"Profile '{profile.Name}': {x}"));
            }
        }

        createdProfiles.Add(createProfileResponse.Data);
        foreach (var resource in profile.Resources)
        {
            var resourceCreateRequest = resource.ToCreate();
            resourceCreateRequest.GameProfileId = createProfileResponse.Data;
            var createResourceResponse = await GameServerService.CreateLocalResourceAsync(resourceCreateRequest, _loggedInUserId);
            if (!createResourceResponse.Succeeded)
            {
                errors.AddRange(createResourceResponse.Messages.Select(x => $"Profile '{profile.Name}': {x}"));
                continue;
            }

            foreach (var configCreateRequest in resource.Configuration.Select(configItem => configItem.ToCreate()))
            {
                configCreateRequest.LocalResourceId = createResourceResponse.Data;
                var createConfigResponse = await GameServerService.CreateConfigurationItemAsync(configCreateRequest, _loggedInUserId);
                if (!createConfigResponse.Succeeded)
                {
                    errors.AddRange(createConfigResponse.Messages.Select(x => $"Profile '{profile.Name}': {x}"));
                }
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