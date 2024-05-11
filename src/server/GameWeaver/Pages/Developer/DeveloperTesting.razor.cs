using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.Events;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Host;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Network;
using Application.Models.Identity.User;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Enums.GameServer;

namespace GameWeaver.Pages.Developer;

public partial class DeveloperTesting : IAsyncDisposable
{
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;
    [Inject] private INetworkService NetworkService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private IGameService GameService { get; init; } = null!;
    [Inject] private IGameServerService GameServerService { get; init; } = null!;
    [Inject] private IHostService HostService { get; init; } = null!;
    
    private AppUserFull _loggedInUser = new();
    private bool _isContributor;
    private bool _isTester;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");

    private string _serverIp = "";
    private int _serverPort = 0;
    private string _serverStatus = "";
    private NetworkProtocol _serverProtocol = NetworkProtocol.Tcp;
    private GameSource _serverSource = GameSource.Steam;
    private List<GameServerSlim> _gameServers = [];
    private GameServerSlim? _selectedGameServer = null;
    private List<HostSlim> _hosts = [];
    private HostSlim? _selectedHost = null;

    private GameSlim _desiredGame = new()
    {
        FriendlyName = "Conan Exiles Dedicated Server",
        SteamName = "Conan Exiles - Dedicated Server",
        SteamGameId = 440900,
        SteamToolId = 443030
    };
    private GameProfileSlim _desiredProfile = new()
    {
        FriendlyName = "Conan Exiles - Profile",
        ServerProcessName = "ConanSandboxServer.exe"
    };
    private GameServerSlim _desiredGameServer = new()
    {
        ServerName = "Test Conan Exiles Server",
        Password = "dietpassword1",
        PasswordRcon = "dietrcon1",
        PasswordAdmin = "dietadmin1",
        PortGame = 32000,
        PortQuery = 42000,
        PortRcon = 52000,
        Modded = false,
        Private = false
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateLoggedInUser();
            await GetPermissions();
            await GetClientTimezone();
            await GatherGameServers();
            await GatherHosts();

            GameServerService.GameServerStatusChanged += GameServerStatusChanged;
            
            StateHasChanged();
        }
    }

    private void GameServerStatusChanged(object? sender, GameServerStatusEvent args)
    {
        Snackbar.Add($"Game server '{args.ServerName}' status change: {args.ServerState}");
        InvokeAsync(StateHasChanged);
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

    private async Task GatherGameServers()
    {
        _gameServers = (await GameServerService.GetAllAsync()).Data.ToList();
        Snackbar.Add($"Gathered {_gameServers.Count} game servers", Severity.Success);
    }

    private async Task GatherHosts()
    {
        _hosts = (await HostService.GetAllAsync()).Data.ToList();
        Snackbar.Add($"Gathered {_hosts.Count} hosts", Severity.Success);
    }

    private async Task EnforceGame()
    {
        var gameCreate = new GameCreate
        {
            FriendlyName = _desiredGame.FriendlyName,
            SteamName = _desiredGame.SteamName,
            SteamGameId = _desiredGame.SteamGameId,
            SteamToolId = _desiredGame.SteamToolId
        };
        
        var matchingGameRequest = await GameService.GetBySteamToolIdAsync(gameCreate.SteamToolId);
        if (matchingGameRequest.Succeeded)
        {
            Snackbar.Add($"Found Game: [{matchingGameRequest.Data.Id}]{matchingGameRequest.Data.FriendlyName}");
            _desiredGame.Id = matchingGameRequest.Data.Id;
            return;
        }

        var createGameRequest = await GameService.CreateAsync(gameCreate);
        if (!createGameRequest.Succeeded)
        {
            foreach (var message in createGameRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Created game: [{createGameRequest.Data}]{gameCreate.FriendlyName}", Severity.Success);
        _desiredGame.Id = matchingGameRequest.Data.Id;
    }

    private async Task EnforceGameProfile()
    {
        if (_selectedGameServer is null)
        {
            Snackbar.Add("You must select a game server to enforce a game profile!", Severity.Error);
            return;
        }
        
        var matchingGame = await GameService.GetBySteamToolIdAsync(_desiredGame.SteamToolId);
        if (!matchingGame.Succeeded)
        {
            Snackbar.Add($"Game '{_desiredGame.FriendlyName}' wasn't found, please create it first", Severity.Error);
            return;
        }

        var profileCreate = new GameProfileCreate
        {
            FriendlyName = _desiredProfile.FriendlyName,
            OwnerId = _loggedInUser.Id,
            GameId = matchingGame.Data.Id,
            ServerProcessName = _desiredProfile.ServerProcessName
        };
        var matchingProfile = await GameServerService.GetGameProfileByFriendlyNameAsync(profileCreate.FriendlyName);
        if (!matchingProfile.Succeeded)
        {
            var createProfileRequest = await GameServerService.CreateGameProfileAsync(profileCreate);
            if (!createProfileRequest.Succeeded)
            {
                foreach (var message in createProfileRequest.Messages)
                {
                    Snackbar.Add(message, Severity.Error);
                }
                return;
            }
            Snackbar.Add($"Created game profile: [{createProfileRequest.Data}]{profileCreate.FriendlyName}", Severity.Success);
            matchingProfile = await GameServerService.GetGameProfileByIdAsync(createProfileRequest.Data);
            _desiredProfile.Id = matchingProfile.Data.Id;
        }

        var resourceCreate = new LocalResourceCreate
        {
            GameProfileId = matchingProfile.Data.Id,
            GameServerId = _selectedGameServer?.Id ?? Guid.Empty,
            Name = "Dedicated Server Executable",
            Path = "ConanSandbox/Binaries/Win64/ConanSandboxServer-Win64-Shipping",
            Startup = true,
            StartupPriority = 0,
            Type = ResourceType.Executable,
            Extension = "exe",
            Args = "-log"
        };
        var profileResources = await GameServerService.GetLocalResourcesByGameProfileIdAsync(matchingProfile.Data.Id);
        var matchingResource = profileResources.Data.FirstOrDefault(x => x.Name == resourceCreate.Name && x.Type == resourceCreate.Type);
        if (matchingResource is not null)
        {
            Snackbar.Add($"Found game profile: [{matchingProfile.Data.Id}]{matchingProfile.Data.FriendlyName}");
            _desiredProfile.Id = matchingProfile.Data.Id;
            return;
        }

        var createResourceRequest = await GameServerService.CreateLocalResourceAsync(resourceCreate, _loggedInUser.Id);
        if (!createResourceRequest.Succeeded)
        {
            foreach (var message in createResourceRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add("Updated and enforced game profile", Severity.Success);
        _desiredProfile.Id = matchingProfile.Data.Id;
    }

    private async Task CreateGameServer()
    {
        var matchingGameServer = _gameServers.FirstOrDefault(x => x.PortGame == _desiredGameServer.PortGame);
        if (matchingGameServer is not null)
        {
            Snackbar.Add($"Game server already exists: [{matchingGameServer.Id}]{matchingGameServer.ServerName}", Severity.Warning);
            return;
        }

        if (_selectedHost is null)
        {
            Snackbar.Add("You must select a host to bind this game server to!", Severity.Error);
            return;
        }
        
        var matchingGame = await GameService.GetBySteamToolIdAsync(_desiredGame.SteamToolId);
        if (!matchingGame.Succeeded)
        {
            Snackbar.Add($"Game '{_desiredGame.FriendlyName}' wasn't found, please create it first", Severity.Error);
            return;
        }
        
        var matchingProfile = await GameServerService.GetGameProfileByFriendlyNameAsync(_desiredProfile.FriendlyName);
        if (!matchingGame.Succeeded)
        {
            Snackbar.Add($"Game profile '{matchingProfile.Data.FriendlyName}' wasn't found, please create it first", Severity.Error);
            return;
        }
        
        var gameServerCreate = new GameServerCreate
        {
            OwnerId = _loggedInUser.Id,
            HostId = _selectedHost.Id,
            GameId = matchingGame.Data.Id,
            GameProfileId = matchingProfile.Data.Id,
            ServerName = $"{_desiredGameServer.ServerName} - {DateTimeService.NowDatabaseTime.ToLongDateString()}",
            Password = _desiredGameServer.Password,
            PasswordRcon = _desiredGameServer.PasswordRcon,
            PasswordAdmin = _desiredGameServer.PasswordAdmin,
            PortGame = _desiredGameServer.PortGame,
            PortQuery = _desiredGameServer.PortQuery,
            PortRcon = _desiredGameServer.PortRcon,
            Modded = _desiredGameServer.Modded,
            Private = _desiredGameServer.Private,
            CreatedBy = _loggedInUser.Id
        };
        var createServerRequest = await GameServerService.CreateAsync(gameServerCreate);
        if (!createServerRequest.Succeeded)
        {
            foreach (var message in createServerRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Created game server, now installing! [{createServerRequest.Data}]{gameServerCreate.ServerName}");
        await GatherGameServers();
        StateHasChanged();
    }

    private async Task StartGameServer()
    {
        if (_selectedGameServer is null)
        {
            Snackbar.Add("You must select a game server first!", Severity.Error);
            return;
        }

        var startRequest = await GameServerService.StartServerAsync(_selectedGameServer.Id, _loggedInUser.Id);
        if (!startRequest.Succeeded)
        {
            foreach (var message in startRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Sent game server start request!");
    }

    private async Task StopGameServer()
    {
        if (_selectedGameServer is null)
        {
            Snackbar.Add("You must select a game server first!", Severity.Error);
            return;
        }

        var stopRequest = await GameServerService.StopServerAsync(_selectedGameServer.Id, _loggedInUser.Id);
        if (!stopRequest.Succeeded)
        {
            foreach (var message in stopRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Sent game server stop request!");
    }

    private async Task RestartGameServer()
    {
        if (_selectedGameServer is null)
        {
            Snackbar.Add("You must select a game server first!", Severity.Error);
            return;
        }

        var restartRequest = await GameServerService.RestartServerAsync(_selectedGameServer.Id, _loggedInUser.Id);
        if (!restartRequest.Succeeded)
        {
            foreach (var message in restartRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Sent game server restart request!");
    }

    public async ValueTask DisposeAsync()
    {
        GameServerService.GameServerStatusChanged -= GameServerStatusChanged;
        await Task.CompletedTask;
    }
}