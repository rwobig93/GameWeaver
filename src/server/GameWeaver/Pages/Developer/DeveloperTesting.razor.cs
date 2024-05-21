using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.Events;
using Application.Models.GameServer.ConfigurationItem;
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
using GameWeaver.Helpers;

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
    [Inject] private IEventService EventService { get; init; } = null!;
    
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
    private GameServerSlim? _selectedGameServer;
    private List<HostSlim> _hosts = [];
    private HostSlim? _selectedHost;
    private string _latestWorkState = "";
    private string _registrationToken = "";

    private readonly GameSlim _desiredGame = new()
    {
        FriendlyName = "Conan Exiles Dedicated Server",
        SteamName = "Conan Exiles - Dedicated Server",
        SteamGameId = 440900,
        SteamToolId = 443030
    };
    private readonly GameProfileSlim _defaultProfile = new()
    {
        FriendlyName = "Conan Exiles - Profile",
        ServerProcessName = "ConanSandboxServer.exe"
    };
    private readonly GameServerSlim _desiredGameServer = new()
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
    private readonly LocalResourceSlim _desiredResourceEngine = new()
    {
        Name = "Config - Engine",
        Path = "ConanSandbox/Saved/Config/WindowsServer/Engine",
        Startup = false,
        StartupPriority = 0,
        Type = ResourceType.ConfigFile,
        ContentType = ContentType.Ini,
        Extension = "ini",
        ConfigSets =
        [
            new ConfigurationItemSlim { Category = "Core.System", Key = "Paths", Value = "../../../Engine/Content", DuplicateKey = true },
            new ConfigurationItemSlim { Category = "Core.System", Key = "Paths", Value = "%GAMEDIR%Content", DuplicateKey = true },
            new ConfigurationItemSlim { Category = "Core.System", Key = "Paths", Value = "../../../Engine/Plugins/2D/Paper2D/Content", DuplicateKey = true },
            new ConfigurationItemSlim { Category = "Core.System", Key = "Paths", Value = "../../../Engine/Plugins/Runtime/HoudiniEngine/Content", DuplicateKey = true },
            new ConfigurationItemSlim { Category = "Core.System", Key = "Paths", Value = "../../../ConanSandbox/Plugins/DialoguePlugin/Content", DuplicateKey = true },
            new ConfigurationItemSlim { Category = "Core.System", Key = "Paths", Value = "../../../ConanSandbox/Plugins/FuncomLiveServices/Content", DuplicateKey = true },
            new ConfigurationItemSlim { Category = "OnlineSubsystemSteam", Key = "ServerName", Value = "%%%SERVER_NAME%%%"},
            new ConfigurationItemSlim { Category = "OnlineSubsystemSteam", Key = "ServerPassword", Value = "%%%PASSWORD%%%"},
            new ConfigurationItemSlim { Category = "OnlineSubsystemSteam", Key = "AsyncTaskTimeout", Value = "360"},
            new ConfigurationItemSlim { Category = "OnlineSubsystemSteam", Key = "GameServerQueryPort", Value = "%%%QUERY_PORT%%%"},
            new ConfigurationItemSlim { Category = "url", Key = "Port", Value = "%%%GAME_PORT%%%"},
            new ConfigurationItemSlim { Category = "url", Key = "PeerPort", Value = "%%%GAME_PORT_PEER%%%"},
            new ConfigurationItemSlim { Category = "/script/onlinesubsystemutils.ipnetdriver", Key = "NetServerMaxTickRate", Value = "30"},
        ]
    };
    private readonly LocalResourceSlim _desiredResourceGame = new()
    {
        Name = "Config - Game",
        Path = "ConanSandbox/Saved/Config/WindowsServer/Game",
        Startup = false,
        StartupPriority = 0,
        Type = ResourceType.ConfigFile,
        ContentType = ContentType.Ini,
        Extension = "ini",
        ConfigSets =
        [
            new ConfigurationItemSlim { Category = "/script/engine.gamesession", Key = "MaxPlayers", Value = "70"},
        ]
    };
    private readonly LocalResourceSlim _desiredResourceServerSettings = new()
    {
        Name = "Config - ServerSettings",
        Path = "ConanSandbox/Saved/Config/WindowsServer/ServerSettings",
        Startup = false,
        StartupPriority = 0,
        Type = ResourceType.ConfigFile,
        ContentType = ContentType.Ini,
        Extension = "ini",
        ConfigSets =
        [
            new ConfigurationItemSlim { Category = "ServerSettings", Key = "AdminPassword", Value = "%%%PASSWORD_ADMIN%%%"},
            new ConfigurationItemSlim { Category = "ServerSettings", Key = "MaxNudity", Value = "2"},
            new ConfigurationItemSlim { Category = "ServerSettings", Key = "PVPBlitzServer", Value = "False"},
            new ConfigurationItemSlim { Category = "ServerSettings", Key = "PVPEnabled", Value = "True"},
            new ConfigurationItemSlim { Category = "ServerSettings", Key = "serverRegion", Value = "1"},
            new ConfigurationItemSlim { Category = "ServerSettings", Key = "ServerCommunity", Value = "3"},
            new ConfigurationItemSlim { Category = "ServerSettings", Key = "IsBattlEyeEnabled", Value = "False"},
        ]
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

            EventService.GameServerStatusChanged += GameServerStatusChanged;
            EventService.WeaverWorkStatusChanged += WorkStatusChanged;
            
            await AttemptDebugSetup();
            
            StateHasChanged();
        }
    }

    private async Task AttemptDebugSetup()
    {
        if (_hosts.Count > 0)
        {
            _selectedHost = _hosts.First();
        }

        if (_gameServers.Count > 0)
        {
            _selectedGameServer = _gameServers.First();
        }
        
        var matchingGameRequest = await GameService.GetBySteamToolIdAsync(_desiredGame.SteamToolId);
        if (matchingGameRequest.Succeeded)
        {
            _desiredGame.Id = matchingGameRequest.Data.Id;
        }
        
        if (matchingGameRequest.Succeeded)
        {
            var matchingProfile = await GameServerService.GetGameProfileByIdAsync(matchingGameRequest.Data.DefaultGameProfileId);
            if (matchingProfile.Succeeded)
            {
                _defaultProfile.Id = matchingProfile.Data.Id;
            }
        }
    } 

    private void WorkStatusChanged(object? sender, WeaverWorkStatusEvent e)
    {
        _latestWorkState = e.Status.ToString();
        InvokeAsync(StateHasChanged);
    }

    private void GameServerStatusChanged(object? sender, GameServerStatusEvent args)
    {
        Snackbar.Add($"Game server {args.ServerName} state: {args.ServerState}", Severity.Info);
        var matchingServer = _gameServers.FirstOrDefault(x => x.Id == args.Id);
        if (matchingServer is not null)
        {
            matchingServer.ServerState = args.ServerState;
        }
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

    private async Task EnforceGameProfileResources()
    {
        if (_selectedGameServer is null)
        {
            Snackbar.Add("You must select a game server first!", Severity.Error);
            return;
        }

        var profileResources = await GameServerService.GetLocalResourcesForGameServerIdAsync(_selectedGameServer.Id);
        var desiredResources = new List<LocalResourceSlim>
        {
            _desiredResourceEngine,
            _desiredResourceGame,
            _desiredResourceServerSettings
        };

        foreach (var resource in desiredResources)
        {
            var matchingResource = profileResources.Data.FirstOrDefault(x => x.Path == resource.Path && x.Type == resource.Type);
            if (matchingResource is not null)
            {
                resource.Id = matchingResource.Id;
                continue;
            }
            
            var resourceCreate = resource.ToCreate();
            resourceCreate.GameProfileId = _selectedGameServer.GameProfileId;
            resourceCreate.GameServerId = _selectedGameServer.Id;
            var createRequest = await GameServerService.CreateLocalResourceAsync(resourceCreate, _loggedInUser.Id);
            if (!createRequest.Succeeded)
            {
                createRequest.Messages.ForEach(x => Snackbar.Add($"Failed to create resource for profile: {x}", Severity.Error));
                return;
            }

            resource.Id = createRequest.Data;
        }

        foreach (var resource in desiredResources)
        {
            var resourcesRequest = await GameServerService.GetConfigurationItemsByLocalResourceIdAsync(resource.Id);
            var resourceConfigItems = resourcesRequest.Succeeded ? resourcesRequest.Data.ToList() : [];
            foreach (var configItem in resource.ConfigSets)
            {
                var matchingItem = configItem.DuplicateKey ? resourceConfigItems.FirstOrDefault(x =>
                    x.Category == configItem.Category &&
                    x.Key == configItem.Key &&
                    x.Value == configItem.Value) :
                    resourceConfigItems.FirstOrDefault(x => x.Category == configItem.Category && x.Key == configItem.Key);
                if (matchingItem is not null && configItem.DuplicateKey)
                {
                    continue;
                }
                if (matchingItem is not null && configItem.Value == matchingItem.Value)
                {
                    continue;
                }
                
                if (matchingItem is not null)
                {
                    var itemUpdate = new ConfigurationItemUpdate {Id = matchingItem.Id, Value = configItem.Value, ModifyingUserId = _loggedInUser.Id};
                    var updateRequest = await GameServerService.UpdateConfigurationItemAsync(itemUpdate);
                    if (!updateRequest.Succeeded)
                    {
                        updateRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                        return;
                    }
                    
                    continue;
                }
            
                var itemCreate = configItem.ToCreate();
                itemCreate.LocalResourceId = resource.Id;
                var createRequest = await GameServerService.CreateConfigurationItemAsync(itemCreate);
                if (createRequest.Succeeded) continue;
                
                createRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            }
        }

        var hostUpdateRequest = await GameServerService.UpdateAllLocalResourcesOnGameServerAsync(_selectedGameServer.Id, _loggedInUser.Id);
        if (!hostUpdateRequest.Succeeded)
        {
            hostUpdateRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        Snackbar.Add("Successfully enforced profile resources on the server and host", Severity.Success);
    }

    private async Task EnforceDefaultGameProfile()
    {
        var matchingGame = await GameService.GetBySteamToolIdAsync(_desiredGame.SteamToolId);
        if (!matchingGame.Succeeded)
        {
            Snackbar.Add($"Game '{_desiredGame.FriendlyName}' wasn't found, please create it first", Severity.Error);
            return;
        }

        var profileCreate = new GameProfileCreate
        {
            FriendlyName = _defaultProfile.FriendlyName,
            OwnerId = _loggedInUser.Id,
            GameId = matchingGame.Data.Id,
            ServerProcessName = _defaultProfile.ServerProcessName,
            
        };
        var matchingProfile = await GameServerService.GetGameProfileByIdAsync(matchingGame.Data.DefaultGameProfileId);
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

            var updateGameRequest = await GameService.UpdateAsync(new GameUpdate {Id = matchingGame.Data.Id, DefaultGameProfileId = createProfileRequest.Data});
            if (!updateGameRequest.Succeeded)
            {
                foreach (var message in updateGameRequest.Messages)
                {
                    Snackbar.Add(message, Severity.Error);
                }
                return;
            }
            Snackbar.Add($"Created default game profile: [{createProfileRequest.Data}]{profileCreate.FriendlyName}", Severity.Success);
            matchingProfile = await GameServerService.GetGameProfileByIdAsync(createProfileRequest.Data);
            _defaultProfile.Id = matchingProfile.Data.Id;
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
            ContentType = ContentType.Raw,
            Extension = "exe",
            Args = "-log"
        };
        var profileResources = await GameServerService.GetLocalResourcesByGameProfileIdAsync(matchingProfile.Data.Id);
        var matchingResource = profileResources.Data.FirstOrDefault(x => x.Name == resourceCreate.Name && x.Type == resourceCreate.Type);
        if (matchingResource is not null)
        {
            Snackbar.Add($"Default game profile is correct: [{matchingProfile.Data.Id}]{matchingProfile.Data.FriendlyName}");
            _defaultProfile.Id = matchingProfile.Data.Id;
            return;
        }

        var resourceCreateRequest = await GameServerService.CreateLocalResourceAsync(resourceCreate, _loggedInUser.Id);
        if (!resourceCreateRequest.Succeeded)
        {
            foreach (var message in resourceCreateRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }
        
        Snackbar.Add("Updated and enforced default game profile", Severity.Success);
        _defaultProfile.Id = matchingProfile.Data.Id;
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
        
        var gameServerCreate = new GameServerCreate
        {
            OwnerId = _loggedInUser.Id,
            HostId = _selectedHost.Id,
            GameId = matchingGame.Data.Id,
            GameProfileId = Guid.Empty,
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
        
        matchingGameServer = _gameServers.FirstOrDefault(x => x.PortGame == _desiredGameServer.PortGame);
        if (matchingGameServer is not null)
        {
            _selectedGameServer = matchingGameServer;
            StateHasChanged();
        }
    }

    private async Task UninstallGameServer()
    {
        if (_selectedGameServer is null)
        {
            return;
        }

        var dialogResponse = await DialogService.ConfirmDialog(
            "Are you sure you want to uninstall this game server?", $"Server Name: {_selectedGameServer.ServerName}");
        if (dialogResponse.Canceled) return;

        var uninstallRequest = await GameServerService.DeleteAsync(_selectedGameServer.Id, _loggedInUser.Id);
        if (!uninstallRequest.Succeeded)
        {
            foreach (var message in uninstallRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Game server {_selectedGameServer.ServerName} is being uninstalled!", Severity.Success);
        _gameServers.Remove(_selectedGameServer);
        _selectedGameServer = null;
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

        Snackbar.Add($"Sent game server start request!", Severity.Success);
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

        Snackbar.Add($"Sent game server stop request!", Severity.Success);
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

        Snackbar.Add($"Sent game server restart request!", Severity.Success);
    }

    private async Task UpdateGameServer()
    {
        if (_selectedGameServer is null)
        {
            Snackbar.Add("You must select a game server first!", Severity.Error);
            return;
        }

        var updateRequest = await GameServerService.UpdateServerAsync(_selectedGameServer.Id, _loggedInUser.Id);
        if (!updateRequest.Succeeded)
        {
            foreach (var message in updateRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Sent game server update request!", Severity.Success);
    }

    private async Task GenerateHostRegistration()
    {
        // TODO: ERROR: Implicit conversion from data type nvarchar to varbinary is not allowed. Use the CONVERT function to run this query.
        var description = $"Test Host - {DateTimeService.NowFromTimeZone(_localTimeZone.Id).ToLongTimeString()}";
        var registerRequest = await HostService.RegistrationGenerateNew(description, _loggedInUser.Id, _loggedInUser.Id);
        if (!registerRequest.Succeeded)
        {
            foreach (var message in registerRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }
        
        // TODO: Add host registration token cleanup after configured time period if it hasn't been used
        await WebClientService.InvokeClipboardCopy(registerRequest.Data.RegisterUrl);
        Snackbar.Add($"Generated host registration token and copied it to your clipboard!", Severity.Success);
    }
    
    public async ValueTask DisposeAsync()
    {
        EventService.GameServerStatusChanged -= GameServerStatusChanged;
        EventService.WeaverWorkStatusChanged -= WorkStatusChanged;
        await Task.CompletedTask;
    }
}