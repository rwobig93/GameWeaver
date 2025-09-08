using Application.Helpers.Identity;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Host;
using Application.Responses.v1.Identity;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Models.Identity;

namespace GameWeaver.Pages;

public partial class Index
{
    // MainLayout has a CascadingParameter of itself, this allows the refresh button on the AppBar to refresh all page state data
    //  If this parameter isn't cascaded to a page, then the refresh button won't affect that pages' state data
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;
    [Inject] private IGameServerService GameServerService { get; init; } = null!;
    [Inject] private IHostService HostService { get; init; } = null!;

    private UserBasicResponse _loggedInUser = new();
    private AppUserPreferenceFull _userPreferences = new();

    private string _cssBase = "rounded-lg py-3";
    private string _cssThemedBorder = "";
    private string _cssThemedText = "smaller";
    private List<GameServerSlim> _ownedGameServers = [];
    private List<GameServerSlim> _favoriteGameServers = [];
    private List<HostSlim> _ownedHosts = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateLoggedInUser();
            await GetFavoriteGameServers();
            await GetOwnedGameServers();
            await GetOwnedHosts();
            UpdateThemedElements();
        }
    }

    private async Task UpdateLoggedInUser()
    {
        var user = await CurrentUserService.GetCurrentUserBasic();
        if (user is null)
        {
            StateHasChanged();
            return;
        }

        _loggedInUser = user;

        var preferenceResponse = await AccountService.GetPreferences(_loggedInUser.Id);
        if (!preferenceResponse.Succeeded)
        {
            StateHasChanged();
            return;
        }

        _userPreferences = preferenceResponse.Data;
        StateHasChanged();
    }

    private void UpdateThemedElements()
    {
        if (!_userPreferences.GamerMode)
        {
            return;
        }

        _cssThemedBorder = " border-rainbow";
        _cssThemedText = "smaller rainbow-text";
        StateHasChanged();
    }

    private async Task GetOwnedGameServers()
    {
        var ownedGameServersRequest = await GameServerService.GetByOwnerIdAsync(_loggedInUser.Id, ServerState.SystemUserId);
        _ownedGameServers = ownedGameServersRequest.Data.ToList();
        StateHasChanged();
    }

    private async Task GetOwnedHosts()
    {
        var ownedHosts = await HostService.GetByOwnerIdAsync(_loggedInUser.Id);
        _ownedHosts = ownedHosts.Data.ToList();
        StateHasChanged();
    }

    private async Task GetFavoriteGameServers()
    {
        var favoriteGameServerIds = _userPreferences.GetFavoriteGameServerIds().ToList();
        if (favoriteGameServerIds.Count == 0)
        {
            return;
        }

        var gameServersRequest = await GameServerService.GetByIdMultipleAsync(favoriteGameServerIds, ServerState.SystemUserId);
        if (!gameServersRequest.Succeeded)
        {
            gameServersRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _favoriteGameServers = gameServersRequest.Data.ToList();
        StateHasChanged();
    }
}