using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.Events;
using Application.Models.External.Steam;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Network;
using Application.Models.Identity.User;
using Application.Models.Integrations;
using Application.Requests.GameServer.Game;
using Application.Requests.GameServer.GameProfile;
using Application.Requests.GameServer.GameServer;
using Application.Requests.GameServer.Host;
using Application.Requests.Integrations;
using Application.Services.External;
using Application.Services.GameServer;
using Application.Services.Integrations;
using Application.Services.Lifecycle;
using Domain.Enums.GameServer;
using Domain.Enums.Integrations;
using Domain.Enums.Lifecycle;
using GameWeaver.Helpers;
using Microsoft.AspNetCore.Components.Forms;

namespace GameWeaver.Pages.Developer;

public partial class DeveloperTesting : IAsyncDisposable
{
    [Inject] private INetworkService NetworkService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private IGameService GameService { get; init; } = null!;
    [Inject] private IGameServerService GameServerService { get; init; } = null!;
    [Inject] private IHostService HostService { get; init; } = null!;
    [Inject] private IEventService EventService { get; init; } = null!;
    [Inject] private ISteamApiService SteamService { get; init; } = null!;
    [Inject] private ITroubleshootingRecordService TshootService { get; init; } = null!;
    [Inject] private IFileStorageRecordService FileService { get; init; } = null!;

    private AppUserFull _loggedInUser = new();
    private bool _isContributor;
    private bool _isTester;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");

    private string _serverIp = "atmos-games-01";
    private int _serverPort = 40520;
    private string _serverStatus = "";
    private NetworkProtocol _serverProtocol = NetworkProtocol.Udp;
    private GameSource _serverSource = GameSource.Steam;
    private List<GameServerSlim> _gameServers = [];
    private GameServerSlim? _selectedGameServer;
    private List<HostSlim> _hosts = [];
    private HostSlim? _selectedHost;
    private List<GameSlim> _games = [];
    private GameSlim? _selectedGame;
    private bool _gameServerUpToDate;
    private string _latestWorkState = "Idle";
    private string _registrationToken = "";
    private Timer? _timer;
    private HostCheckInFull? _latestHostCheckin;
    private bool _workInProgress;
    private SteamAppInfo _steamAppInfo = new();
    private MudTable<FileStorageRecordSlim> _fileRecordsTable = new();
    private IEnumerable<FileStorageRecordSlim> _fileRecords = [];
    private int _totalFileRecords;
    private bool _fileUploading;
    private bool _tableDense = true;
    private bool _tableHover = true;
    private bool _tableStriped = true;
    private bool _tableBordered;

    private readonly GameSlim _desiredGame = new()
    {
        Id = Guid.CreateVersion7(),
        FriendlyName = "Conan Exiles Dedicated Server",
        SteamName = "Conan Exiles - Dedicated Server",
        SteamGameId = 440900,
        SteamToolId = 443030
    };
    private readonly GameProfileSlim _defaultProfile = new()
    {
        Id = Guid.CreateVersion7(),
        FriendlyName = "Conan Exiles - Profile",
    };
    private readonly GameServerSlim _desiredGameServer = new()
    {
        Id = Guid.CreateVersion7(),
        ServerName = "Test Conan Exiles Server",
        Password = "dietpassword1",
        PasswordRcon = "dietrcon1",
        PasswordAdmin = "dietadmin1",
        PortGame = 30010,
        PortPeer = 30011,
        PortQuery = 40010,
        PortRcon = 50010,
        Modded = false,
        Private = false
    };
    private readonly LocalResourceSlim _desiredResourceExecutable = new()
    {
        Id = Guid.CreateVersion7(),
        Name = "Dedicated Server Executable",
        PathWindows = "ConanSandbox/Binaries/Win64/ConanSandboxServer-Win64-Shipping.exe",
        Startup = true,
        StartupPriority = 0,
        Type = ResourceType.Executable,
        ContentType = ContentType.Raw,
        Args = "-log"
    };
    private readonly LocalResourceSlim _desiredResourceEngine = new()
    {
        Id = Guid.CreateVersion7(),
        Name = "Config - Engine",
        PathWindows = "ConanSandbox/Saved/Config/WindowsServer/Engine.ini",
        Startup = false,
        StartupPriority = 0,
        Type = ResourceType.ConfigFile,
        ContentType = ContentType.Ini,
        ConfigSets =
        [
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "Core.System", Key = "Paths", Value = "../../../Engine/Content", DuplicateKey = true },
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "Core.System", Key = "Paths", Value = "%GAMEDIR%Content", DuplicateKey = true },
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "Core.System", Key = "Paths", Value = "../../../Engine/Plugins/2D/Paper2D/Content",
                DuplicateKey = true },
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "Core.System", Key = "Paths", Value = "../../../Engine/Plugins/Runtime/HoudiniEngine/Content",
                DuplicateKey = true },
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "Core.System", Key = "Paths", Value = "../../../ConanSandbox/Plugins/DialoguePlugin/Content",
                DuplicateKey = true },
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "Core.System", Key = "Paths", Value = "../../../ConanSandbox/Plugins/FuncomLiveServices/Content",
                DuplicateKey = true },
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "OnlineSubsystemSteam", Key = "ServerName", Value = "%%%SERVER_NAME%%%"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "OnlineSubsystemSteam", Key = "ServerPassword", Value = "%%%PASSWORD%%%"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "OnlineSubsystemSteam", Key = "AsyncTaskTimeout", Value = "360"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "OnlineSubsystemSteam", Key = "GameServerQueryPort", Value = "%%%QUERY_PORT%%%"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "url", Key = "Port", Value = "%%%GAME_PORT%%%"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "url", Key = "PeerPort", Value = "%%%GAME_PORT_PEER%%%"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "/script/onlinesubsystemutils.ipnetdriver", Key = "NetServerMaxTickRate", Value = "30"},
        ]
    };
    private readonly LocalResourceSlim _desiredResourceGame = new()
    {
        Id = Guid.CreateVersion7(),
        Name = "Config - Game",
        PathWindows = "ConanSandbox/Saved/Config/WindowsServer/Game.ini",
        Startup = false,
        StartupPriority = 0,
        Type = ResourceType.ConfigFile,
        ContentType = ContentType.Ini,
        ConfigSets =
        [
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "/script/engine.gamesession", Key = "MaxPlayers", Value = "70"},
        ]
    };
    private readonly LocalResourceSlim _desiredResourceServerSettings = new()
    {
        Id = Guid.CreateVersion7(),
        Name = "Config - ServerSettings",
        PathWindows = "ConanSandbox/Saved/Config/WindowsServer/ServerSettings.ini",
        Startup = false,
        StartupPriority = 0,
        Type = ResourceType.ConfigFile,
        ContentType = ContentType.Ini,
        ConfigSets =
        [
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "ServerSettings", Key = "AdminPassword", Value = "%%%PASSWORD_ADMIN%%%"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "ServerSettings", Key = "MaxNudity", Value = "2"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "ServerSettings", Key = "PVPBlitzServer", Value = "False"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "ServerSettings", Key = "PVPEnabled", Value = "True"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "ServerSettings", Key = "serverRegion", Value = "1"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "ServerSettings", Key = "ServerCommunity", Value = "3"},
            new ConfigurationItemSlim { Id = Guid.CreateVersion7(), Category = "ServerSettings", Key = "IsBattlEyeEnabled", Value = "False"},
        ]
    };


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateLoggedInUser();
            await GetPermissions();
            await GetClientTimezone();
            await GatherGames();
            await GatherGameServers();
            await GatherHosts();

            EventService.GameVersionUpdated += GameVersionUpdated;
            EventService.GameServerStatusChanged += GameServerStatusChanged;
            EventService.WeaverWorkStatusChanged += WorkStatusChanged;
            _timer = new Timer(async _ => { await UpdateHostUsage(); }, null, 0, 1000);

            await AttemptDebugSetup();

            StateHasChanged();
        }
    }

    private async Task UpdateHostUsage()
    {
        if (_selectedHost is null)
        {
            return;
        }

        var usageRequest = await HostService.GetCheckInsLatestByHostIdAsync(_selectedHost.Id, 100);
        if (!usageRequest.Succeeded)
        {
            Snackbar.Add($"Host usage gather failed: {usageRequest.Messages}", Severity.Error);
            return;
        }

        var latestCheckin = usageRequest.Data.FirstOrDefault();
        if (latestCheckin is null)
        {
            return;
        }

        _latestHostCheckin = latestCheckin;
        await InvokeAsync(StateHasChanged);
    }

    private async Task AttemptDebugSetup()
    {
        if (_games.Count > 0)
        {
            _selectedGame = _games.First();
        }

        if (_hosts.Count > 0)
        {
            _selectedHost = _hosts.First();
        }

        if (_gameServers.Count > 0)
        {
            _selectedGameServer = _gameServers.First();
        }

        var matchingGameRequest = await GameService.GetBySteamToolIdAsync(_desiredGame.SteamToolId);
        if (matchingGameRequest.Data is not null)
        {
            _desiredGame.Id = matchingGameRequest.Data.Id;
        }

        if (matchingGameRequest.Data is not null)
        {
            var matchingProfile = await GameServerService.GetGameProfileByIdAsync(matchingGameRequest.Data.DefaultGameProfileId);
            if (!matchingProfile.Succeeded)
            {
                Snackbar.Add("Failure occurred finding default game profile", Severity.Error);
            }
        }
    }

    private void WorkStatusChanged(object? sender, WeaverWorkStatusEvent e)
    {
        _workInProgress = e.Status switch
        {
            WeaverWorkState.WaitingToBePickedUp => true,
            WeaverWorkState.PickedUp => true,
            WeaverWorkState.InProgress => true,
            WeaverWorkState.Completed => false,
            WeaverWorkState.Cancelled => false,
            WeaverWorkState.Failed => false,
            _ => false
        };
        _latestWorkState = e.Status.ToString();
        InvokeAsync(StateHasChanged);
    }

    private void GameServerStatusChanged(object? sender, GameServerStatusEvent args)
    {
        Snackbar.Add($"Game server state change: {args.ServerState}", Severity.Info);
        var matchingServer = _gameServers.FirstOrDefault(x => x.Id == args.Id);
        if (matchingServer is null)
        {
            return;
        }

        matchingServer.ServerState = args.ServerState ?? matchingServer.ServerState;
        matchingServer.RunningConfigHash = args.RunningConfigHash;
        matchingServer.StorageConfigHash = args.StorageConfigHash;

        if (args.BuildVersionUpdated)
        {
            var matchingGame = _games.FirstOrDefault(x => x.Id == matchingServer.GameId);
            matchingServer.ServerBuildVersion = matchingGame?.LatestBuildVersion ?? matchingServer.ServerBuildVersion;

            if (_selectedGameServer != null && matchingServer.Id == _selectedGameServer.Id)
            {
                _gameServerUpToDate = matchingGame?.LatestBuildVersion == _selectedGameServer.ServerBuildVersion;
            }
        }

        InvokeAsync(StateHasChanged);
    }

    private void CheckGameServerVersion()
    {
        if (_selectedGameServer is null)
        {
            return;
        }

        var matchingGame = _games.FirstOrDefault(x => x.Id == _selectedGameServer.GameId);
        if (matchingGame is null)
        {
            return;
        }

        _gameServerUpToDate = matchingGame.LatestBuildVersion == _selectedGameServer.ServerBuildVersion;
        InvokeAsync(StateHasChanged);
    }

    private void GameVersionUpdated(object? sender, GameVersionUpdatedEvent args)
    {
        Snackbar.Add($"Game updated! [{args.AppId}]{args.AppName}", Severity.Info);
    }

    private async Task GetPermissions()
    {
        var currentUser = await CurrentUserService.GetCurrentUserPrincipal();
        _isContributor = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.AppDevelopment.Contributor);
        _isTester = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.AppDevelopment.Tester);
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

    private async Task GatherGames()
    {
        _games = (await GameService.GetAllAsync()).Data.ToList();
    }

    private async Task GatherGameServers()
    {
        _gameServers = (await GameServerService.GetAllAsync(_loggedInUser.Id)).Data.ToList();
    }

    private async Task GatherHosts()
    {
        _hosts = (await HostService.GetAllAsync()).Data.ToList();
    }

    private async Task EnforceGame()
    {
        var gameCreate = new GameCreateRequest
        {
            Name = _desiredGame.FriendlyName,
            SteamGameId = _desiredGame.SteamGameId,
            SteamToolId = _desiredGame.SteamToolId
        };

        var matchingGameRequest = await GameService.GetBySteamToolIdAsync(gameCreate.SteamToolId);
        if (matchingGameRequest.Data is not null)
        {
            Snackbar.Add($"Found Game: [{matchingGameRequest.Data.Id}]{matchingGameRequest.Data.FriendlyName}");
            _desiredGame.Id = matchingGameRequest.Data.Id;
            return;
        }

        var createGameRequest = await GameService.CreateAsync(gameCreate.ToCreate(), _loggedInUser.Id);
        if (!createGameRequest.Succeeded)
        {
            foreach (var message in createGameRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Created game: [{createGameRequest.Data}]{gameCreate.Name}", Severity.Success);
        _desiredGame.Id = createGameRequest.Data;
    }

    private async Task EnforceGameProfileResources()
    {
        if (_selectedGameServer is null)
        {
            Snackbar.Add("You must select a game server first!", Severity.Error);
            return;
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
        if (matchingGame.Data is null)
        {
            Snackbar.Add($"Game '{_desiredGame.FriendlyName}' wasn't found, please create it first", Severity.Error);
            return;
        }

        var profileCreate = new GameProfileCreateRequest
        {
            Name = _defaultProfile.FriendlyName,
            OwnerId = _loggedInUser.Id,
            GameId = matchingGame.Data.Id

        };
        var matchingProfile = await GameServerService.GetGameProfileByIdAsync(matchingGame.Data.DefaultGameProfileId);
        if (!matchingProfile.Succeeded || matchingProfile.Data is null)
        {
            var createProfileRequest = await GameServerService.CreateGameProfileAsync(profileCreate, _loggedInUser.Id);
            if (!createProfileRequest.Succeeded)
            {
                foreach (var message in createProfileRequest.Messages)
                {
                    Snackbar.Add(message, Severity.Error);
                }
                return;
            }

            var gameUpdate = new GameUpdateRequest {Id = matchingGame.Data.Id, DefaultGameProfileId = createProfileRequest.Data};
            var updateGameRequest = await GameService.UpdateAsync(gameUpdate.ToUpdate(), _loggedInUser.Id);
            if (!updateGameRequest.Succeeded)
            {
                foreach (var message in updateGameRequest.Messages)
                {
                    Snackbar.Add(message, Severity.Error);
                }
                return;
            }
            Snackbar.Add($"Created default game profile: [{createProfileRequest.Data}]{profileCreate.Name}", Severity.Success);
            matchingProfile = await GameServerService.GetGameProfileByIdAsync(createProfileRequest.Data);
            if (!matchingProfile.Succeeded || matchingProfile.Data is null)
            {
                Snackbar.Add("Failed to create default game profile", Severity.Error);
                return;
            }
            _defaultProfile.Id = matchingProfile.Data.Id;
        }

        var profileResources = await GameServerService.GetLocalResourcesByGameProfileIdAsync(matchingProfile.Data.Id);
        var desiredResources = new List<LocalResourceSlim>
        {
            _desiredResourceExecutable,
            _desiredResourceEngine,
            _desiredResourceGame,
            _desiredResourceServerSettings
        };

        foreach (var resource in desiredResources)
        {
            var matchingResource = profileResources.Data.FirstOrDefault(x => x.PathWindows == resource.PathWindows && x.Type == resource.Type);
            if (matchingResource is not null)
            {
                resource.Id = matchingResource.Id;
                continue;
            }

            var resourceCreate = resource.ToCreate();
            resourceCreate.GameProfileId = matchingProfile.Data.Id;

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
                    var itemUpdate = new ConfigurationItemUpdate {Id = matchingItem.Id, Value = configItem.Value};
                    var updateRequest = await GameServerService.UpdateConfigurationItemAsync(itemUpdate, _loggedInUser.Id);
                    if (!updateRequest.Succeeded)
                    {
                        updateRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                        return;
                    }

                    continue;
                }

                var itemCreate = configItem.ToCreate();
                itemCreate.LocalResourceId = resource.Id;
                var createRequest = await GameServerService.CreateConfigurationItemAsync(itemCreate, _loggedInUser.Id);
                if (createRequest.Succeeded) continue;

                createRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            }
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
        if (matchingGame.Data is null)
        {
            Snackbar.Add($"Game '{_desiredGame.FriendlyName}' wasn't found, please create it first", Severity.Error);
            return;
        }

        var gameServerCreateRequest = new GameServerCreateRequest
        {
            OwnerId = _loggedInUser.Id,
            HostId = _selectedHost.Id,
            GameId = matchingGame.Data.Id,
            ParentGameProfileId = Guid.Empty,
            Name = $"{_desiredGameServer.ServerName} - {DateTimeService.NowDatabaseTime.ToLongDateString()}",
            Password = _desiredGameServer.Password,
            PasswordRcon = _desiredGameServer.PasswordRcon,
            PasswordAdmin = _desiredGameServer.PasswordAdmin,
            PortGame = _desiredGameServer.PortGame,
            PortQuery = _desiredGameServer.PortQuery,
            PortRcon = _desiredGameServer.PortRcon,
            Modded = _desiredGameServer.Modded,
            Private = _desiredGameServer.Private
        };
        var createServerRequest = await GameServerService.CreateAsync(gameServerCreateRequest.ToCreate(), _loggedInUser.Id);
        if (!createServerRequest.Succeeded)
        {
            foreach (var message in createServerRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        Snackbar.Add($"Created game server, now installing! [{createServerRequest.Data}]{gameServerCreateRequest.Name}");
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
        var description = $"Test Host - {DateTimeService.NowFromTimeZone(_localTimeZone.Id).ToLongTimeString()}";
        var registerRequest = await HostService.RegistrationGenerateNew(new HostRegistrationCreateRequest
        {
            OwnerId = _loggedInUser.Id,
            Name = "Test Host",
            Description = description,
            AllowedPorts = ["30000-31000", "40000-41000", "50000-51000"]
        }, _loggedInUser.Id);
        if (!registerRequest.Succeeded)
        {
            foreach (var message in registerRequest.Messages)
            {
                Snackbar.Add(message, Severity.Error);
            }
            return;
        }

        _registrationToken = registerRequest.Data.RegisterUrl;
        var copyResult = await WebClientService.InvokeClipboardCopy(registerRequest.Data.RegisterUrl);
        if (!copyResult.Succeeded)
        {
            Snackbar.Add($"Generated host registration token but failed to copy it to your clipboard", Severity.Warning);
            return;
        }
        Snackbar.Add($"Generated host registration token and copied it to your clipboard!", Severity.Success);
    }

    private async Task GetSteamAppBuild()
    {
        if (_selectedGame is null)
        {
            Snackbar.Add("Please select a valid game", Severity.Warning);
            return;
        }

        if (_selectedGame.SteamToolId < 1000)
        {
            Snackbar.Add("Selected game has an invalid App Tool Id", Severity.Error);
            return;
        }

        var currentAppBuild = await SteamService.GetCurrentAppBuild(_selectedGame.SteamToolId);
        if (!currentAppBuild.Succeeded || currentAppBuild.Data is null)
        {
            currentAppBuild.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _steamAppInfo = currentAppBuild.Data;
        Snackbar.Add("Gathered App Id Build Info!");
    }

    private async Task CreateTshootRecord()
    {
        var tshootEntityType = (TroubleshootEntityType)(Enum.GetValues(typeof(TroubleshootEntityType))
            .GetValue(Random.Shared.Next(Enum.GetValues(typeof(TroubleshootEntityType)).Length)) ?? TroubleshootEntityType.Network);

        var tshootId = await TshootService.CreateTroubleshootRecord(DateTimeService, tshootEntityType, Guid.Empty,
            _loggedInUser.Id, $"Example failure troubleshooting record: {Guid.NewGuid()}", new Dictionary<string, string>
            {
                {"Desired Profile", _defaultProfile.FriendlyName},
                {"Desired Profile Id", _defaultProfile.Id.ToString()},
                {"Desired Game", _desiredGame.FriendlyName},
                {"Desired Game Id", _desiredGame.Id.ToString()},
                {"User Id", _loggedInUser.Id.ToString()},
                {"User", _loggedInUser.Username},
                {"Error", "Example error message for troubleshooting purposes"}
            });
        Snackbar.Add(ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data));
    }


    private async Task<TableData<FileStorageRecordSlim>> FileRecordReload(TableState state, CancellationToken token)
    {
        var foundFiles = await FileService.GetAllAsync();
        if (!foundFiles.Succeeded)
        {
            foundFiles.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return new TableData<FileStorageRecordSlim>();
        }

        _fileRecords = foundFiles.Data.ToArray();
        _totalFileRecords = _fileRecords.Count();

        _fileRecords = state.SortLabel switch
        {
            "FriendlyName" => _fileRecords.OrderByDirection(state.SortDirection, o => o.FriendlyName),
            "Description" => _fileRecords.OrderByDirection(state.SortDirection, o => o.Description),
            _ => _fileRecords
        };

        return new TableData<FileStorageRecordSlim> {TotalItems = _totalFileRecords, Items = _fileRecords};
    }

    private void RefreshFileRecords()
    {
        _fileRecordsTable.ReloadServerData();
        StateHasChanged();
    }

    private async Task UploadGameFile(IBrowserFile? file)
    {
        if (file is null)
        {
            return;
        }

        Snackbar.Add("Starting file upload!", Severity.Info);
        _fileUploading = true;
        var friendlyName = $"New_File_{DateTimeService.NowDatabaseTime.ToString(DataConstants.DateTime.FileNameFormat)}";
        var fileName = Guid.NewGuid().ToString();
        var uploadRequest = await FileService.CreateAsync(new FileStorageRecordCreateRequest
        {
            Format = FileStorageFormat.Binary,
            LinkedType = FileStorageType.Game,
            LinkedId = Guid.NewGuid(),
            FriendlyName = friendlyName,
            Filename = Guid.NewGuid().ToString(),
            Description = $"{friendlyName} @ {fileName}",
            Version = "v1.0.0"
        }, file.OpenReadStream(50_000_000_000), _loggedInUser.Id);
        if (!uploadRequest.Succeeded)
        {
            _fileUploading = false;
            uploadRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Finished file upload!", Severity.Success);
        _fileUploading = false;
        RefreshFileRecords();
        await Task.CompletedTask;
    }

    private async Task DownloadGameFile(Guid fileId)
    {
        var foundFile = await FileService.GetByIdAsync(fileId);
        if (foundFile.Data is null)
        {
            Snackbar.Add(ErrorMessageConstants.FileStorage.NotFound);
            return;
        }

        var fileContent = await File.ReadAllBytesAsync(foundFile.Data.GetLocalFilePath());
        var convertedContent = Convert.ToBase64String(fileContent);
        var downloadRequest = await WebClientService.InvokeFileDownload(convertedContent, foundFile.Data.FriendlyName, DataConstants.MimeTypes.Binary);
        if (downloadRequest.Succeeded)
        {
            downloadRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Finished downloading file!", Severity.Success);
    }

    private async Task DeleteFile(Guid fileId)
    {
        var deleteRequest = await FileService.DeleteAsync(fileId, _loggedInUser.Id);
        if (!deleteRequest.Succeeded)
        {
            deleteRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Successfully deleted file!", Severity.Success);
        RefreshFileRecords();
    }

    public async ValueTask DisposeAsync()
    {
        EventService.GameVersionUpdated -= GameVersionUpdated;
        EventService.GameServerStatusChanged -= GameServerStatusChanged;
        EventService.WeaverWorkStatusChanged -= WorkStatusChanged;
        _timer?.Dispose();
        await Task.CompletedTask;
    }
}