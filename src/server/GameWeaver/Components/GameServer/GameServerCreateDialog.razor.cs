using Application.Constants.Communication;
using Application.Helpers.Lifecycle;
using Application.Mappers.Identity;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.Host;
using Application.Requests.GameServer.GameServer;
using Application.Responses.v1.Identity;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Enums.Lifecycle;

namespace GameWeaver.Components.GameServer;

public partial class GameServerCreateDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.AddToQueue;
    [Parameter] public Color IconColor { get; set; } = Color.Success;
    [Parameter] public Color TextColor { get; set; } = Color.Default;
    [Parameter] public string Title { get; set; } = "New Gameserver";
    [Parameter] public string ConfirmButtonText { get; set; } = "Create";
    [Parameter] public int IconWidthPixels { get; set; } = 75;
    [Parameter] public int IconHeightPixels { get; set; } = 75;

    [Inject] public IGameServerService GameServerService { get; init; } = null!;
    [Inject] public ITroubleshootingRecordService TshootService { get; init; } = null!;
    [Inject] public IAppUserService AppUserService { get; init; } = null!;
    [Inject] public IHostService HostService { get; init; } = null!;
    [Inject] public IGameService GameService { get; init; } = null!;
    [Inject] public IRunningServerState ServerState { get; init; } = null!;


    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";
    private Guid _loggedInUserId = Guid.Empty;
    private List<UserBasicResponse> _users = [];
    private List<HostSlim> _hosts = [];
    private List<GameSlim> _games = [];
    private List<GameProfileSlim> _gameProfiles = [];
    private UserBasicResponse _selectedOwner = new() {Id = Guid.Empty, Username = "Unknown"};
    private HostSlim _selectedHost = new() {Id = Guid.Empty, Hostname = "Unknown"};
    private GameSlim _selectedGame = new() {Id = Guid.Empty, FriendlyName = "Unknown"};
    private GameProfileSlim _selectedParentProfile = new() {Id = Guid.Empty, FriendlyName = "None"};
    private readonly GameServerCreateRequest _createRequest = new();
    private bool _showPortConfig;
    
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private InputType _adminPasswordInput = InputType.Password;
    private string _adminPasswordInputIcon = Icons.Material.Filled.VisibilityOff;
    private InputType _rconPasswordInput = InputType.Password;
    private string _rconPasswordInputIcon = Icons.Material.Filled.VisibilityOff;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetAllUsers();
            await GetCurrentUser();
            await GetHosts();
            await GetGames();
            await GetGameProfiles();

            StateHasChanged();
        }
    }

    private async Task GetCurrentUser()
    {
        _loggedInUserId = await CurrentUserService.GetCurrentUserId() ?? Guid.Empty;
        _selectedOwner = _users.FirstOrDefault(x => x.Id == _loggedInUserId) ?? new UserBasicResponse {Username = "Unknown"};
    }

    private async Task GetAllUsers()
    {
        var response = await AppUserService.GetAllAsync();
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _users = response.Data.Where(x => x.Id != Guid.Empty && x.Id != ServerState.SystemUserId).ToResponses();
    }

    private async Task GetHosts()
    {
        var response = await HostService.GetAllAsync();
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _hosts = response.Data.ToList();
    }

    private async Task GetGames()
    {
        var response = await GameService.GetAllAsync();
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        _games = response.Data.ToList();
    }

    private async Task GetGameProfiles()
    {
        if (_selectedGame.Id == Guid.Empty)
        {
            return;
        }
        
        var response = await GameServerService.GetGameProfilesByGameIdAsync(_selectedGame.Id);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        _gameProfiles = response.Data.ToList();
    }

    private async Task<IEnumerable<UserBasicResponse>> FilterUsers(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return _users;
        }

        await Task.CompletedTask;

        return _users.Where(x =>
            x.Username.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.Id.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task<IEnumerable<GameSlim>> FilterGames(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return _games;
        }
        
        await Task.CompletedTask;
        
        return _games.Where(x =>
            x.FriendlyName.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.SteamName.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.Id.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.SteamGameId.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.SteamToolId.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task<IEnumerable<HostSlim>> FilterHosts(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return _hosts;
        }
        
        await Task.CompletedTask;
        
        return _hosts.Where(x =>
            x.FriendlyName.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.Hostname.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.Id.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.Description.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task<IEnumerable<GameProfileSlim>> FilterProfiles(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return _gameProfiles;
        }
        
        await Task.CompletedTask;
        
        return _gameProfiles.Where(x =>
            x.FriendlyName.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.OwnerId.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.Id.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase));
    }
    
    private async Task CreateGameServer()
    {
        if (_selectedGame.Id == Guid.Empty)
        {
            Snackbar.Add("A valid game wasn't selected", Severity.Error);
            return;
        }

        if (_selectedHost.Id == Guid.Empty)
        {
            Snackbar.Add("A valid host wasn't selected", Severity.Error);
            return;
        }

        if (_selectedOwner.Id == Guid.Empty)
        {
            Snackbar.Add("A valid owner wasn't selected", Severity.Error);
            return;
        }
        
        if (_loggedInUserId == Guid.Empty)
        {
            var tshootId = await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.GameServers, Guid.Empty, _loggedInUserId,
                "Failed to create new game server", new Dictionary<string, string>
                {
                    {"ServerName", _createRequest.Name},
                    {"OwnerId", _createRequest.OwnerId.ToString()},
                    {"HostId", _createRequest.HostId.ToString()},
                    {"GameId", _createRequest.GameId.ToString()},
                    {"ParentGameProfileId", _createRequest.ParentGameProfileId.ToString() ?? string.Empty},
                    {"PortGame", _createRequest.PortGame.ToString()},
                    {"PortPeer", _createRequest.PortPeer.ToString()},
                    {"PortQuery", _createRequest.PortQuery.ToString()},
                    {"PortRcon", _createRequest.PortRcon.ToString()},
                    {"Modded", _createRequest.Modded.ToString()},
                    {"Private", _createRequest.Private.ToString()},
                    {"Error", "Logged in user Id is empty, this shouldn't happen, we can't generate a game server without knowing who is submitting the request"}
                });
            Snackbar.Add(ErrorMessageConstants.Generic.ContactAdmin, Severity.Error);
            Snackbar.Add(ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data), Severity.Error);
            return;
        }
        
        _createRequest.OwnerId = _selectedOwner.Id;
        _createRequest.HostId = _selectedHost.Id;
        _createRequest.GameId = _selectedGame.Id;
        _createRequest.ParentGameProfileId = _selectedParentProfile.Id == Guid.Empty ? null : _selectedParentProfile.Id;
        
        var response = await GameServerService.CreateAsync(_createRequest, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        MudDialog.Close(DialogResult.Ok(response.Data));
    }

    private void TogglePasswordVisibility()
    {
        if (_passwordInputIcon == Icons.Material.Filled.VisibilityOff)
        {
            _adminPasswordInput = InputType.Text;
            _adminPasswordInputIcon = Icons.Material.Filled.Visibility;
            return;
        }

        _passwordInput = InputType.Password;
        _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    }

    private void ToggleAdminPasswordVisibility()
    {
        if (_adminPasswordInputIcon == Icons.Material.Filled.VisibilityOff)
        {
            _adminPasswordInput = InputType.Text;
            _adminPasswordInputIcon = Icons.Material.Filled.Visibility;
            return;
        }

        _adminPasswordInput = InputType.Password;
        _adminPasswordInputIcon = Icons.Material.Filled.VisibilityOff;
    }

    private void ToggleRconPasswordVisibility()
    {
        if (_rconPasswordInputIcon == Icons.Material.Filled.VisibilityOff)
        {
            _rconPasswordInput = InputType.Text;
            _rconPasswordInputIcon = Icons.Material.Filled.Visibility;
            return;
        }

        _rconPasswordInput = InputType.Password;
        _rconPasswordInputIcon = Icons.Material.Filled.VisibilityOff;
    }
    
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}