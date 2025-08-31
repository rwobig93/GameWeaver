using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Models.Identity.User;
using Application.Services.GameServer;
using Domain.Models.Identity;

namespace GameWeaver.Pages.Admin;

public partial class AdminPanel : ComponentBase
{
    private bool _canGetGameCount;
    private bool _canGetGameProfileCount;
    private bool _canGetGameServerCount;
    private bool _canGetHostCount;

    private bool _canGetUserCount;
    private int _gameCount;
    private int _gameProfileCount;
    private int _gameServerCount;
    private int _hostCount;
    private int _userCount;
    private AppUserPreferenceFull _userPreferences = new();
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IGameService GameService { get; init; } = null!;
    [Inject] private IGameServerService GameServerService { get; init; } = null!;
    [Inject] private IHostService HostService { get; init; } = null!;

    private AppUserFull CurrentUser { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetCurrentUser();
            await GetPermissions();
            await GetCounts();
            StateHasChanged();
        }
    }

    private async Task GetCurrentUser()
    {
        var userId = await CurrentUserService.GetCurrentUserId();
        if (userId is null)
            return;

        CurrentUser = (await UserService.GetByIdFullAsync((Guid) userId)).Data!;
        _userPreferences = (await AccountService.GetPreferences(CurrentUser.Id)).Data;
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canGetUserCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Users.View);
        _canGetHostCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.Get);
        _canGetGameCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Get);
        _canGetGameProfileCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.GameProfile.Get);
        _canGetGameServerCount = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get);
    }

    private async Task GetCounts()
    {
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
    }
}