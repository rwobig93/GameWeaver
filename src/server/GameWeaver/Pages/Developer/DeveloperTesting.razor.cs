using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.GameServer.Network;
using Application.Models.Identity.User;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Enums.GameServer;

namespace GameWeaver.Pages.Developer;

public partial class DeveloperTesting
{
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;
    [Inject] private INetworkService NetworkService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    
    private AppUserFull _loggedInUser = new();
    private bool _isContributor;
    private bool _isTester;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");

    private string _serverIp = "";
    private int _serverPort = 0;
    private string _serverStatus = "";
    private NetworkProtocol _serverProtocol = NetworkProtocol.Tcp;
    private GameSource _serverSource = GameSource.Steam;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateLoggedInUser();
            await GetPermissions();
            await GetClientTimezone();
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = await CurrentUserService.GetCurrentUserPrincipal();
        _isContributor = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Developer.Contributor);
        _isTester = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Developer.Tester);
    }

    private async Task UpdateLoggedInUser()
    {
        var user = await CurrentUserService.GetCurrentUserFull();
        if (user is null)
            return;

        _loggedInUser = user;
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private string GetDevType()
    {
        if (_isContributor)
            return "Contributor";
        if (_isTester)
            return "Tester";

        return "Guest";
    }
    
    private async Task TestServerConnectivity()
    {
        var checkResponse = await NetworkService.IsGameServerConnectableAsync(new GameServerConnectivityCheck
        {
            HostIp = _serverIp,
            PortGame = _serverPort,
            PortQuery = _serverPort,
            Protocol = _serverProtocol,
            TimeoutMilliseconds = 1000,
            Source = _serverSource
        });

        var status = checkResponse.Data ? "Online" : "Offline";
        _serverStatus = $"{status} at {DateTimeService.NowFromTimeZone(_localTimeZone.Id).ToLongTimeString()}";
    }

    private async Task TestPortOpen()
    {
        var checkResponse = await NetworkService.IsPortOpenAsync(_serverIp, _serverPort, _serverProtocol, 1000);
        
        var status = checkResponse.Data ? "Open" : "Closed";
        _serverStatus = $"{status} at {DateTimeService.NowFromTimeZone(_localTimeZone.Id).ToLongTimeString()}";
    }
}