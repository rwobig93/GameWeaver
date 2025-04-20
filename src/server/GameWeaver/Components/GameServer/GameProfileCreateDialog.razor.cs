using Application.Constants.Communication;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Helpers.Web;
using Application.Mappers.GameServer;
using Application.Mappers.Identity;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.Host;
using Application.Requests.GameServer.GameProfile;
using Application.Requests.GameServer.GameServer;
using Application.Responses.v1.Identity;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Enums.Lifecycle;

namespace GameWeaver.Components.GameServer;

public partial class GameProfileCreateDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Title { get; set; } = "New Configuration Profile";
    [Parameter] public string ConfirmButtonText { get; set; } = "Create Profile";

    [Inject] public IGameServerService GameServerService { get; init; } = null!;
    [Inject] public IGameService GameService { get; init; } = null!;


    private Guid _loggedInUserId = Guid.Empty;
    private List<GameSlim> _games = [];
    private GameSlim _selectedGame = new() {Id = Guid.Empty, FriendlyName = "None"};
    private readonly GameProfileCreateRequest _createRequest = new() { Name = string.Empty };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateCurrentUser();
            await UpdateGames();
            StateHasChanged();
        }
    }

    private async Task UpdateCurrentUser()
    {
        _loggedInUserId = await CurrentUserService.GetCurrentUserId() ?? Guid.Empty;
    }

    private async Task UpdateGames(string searchText = "")
    {
        var response = await GameService.SearchPaginatedAsync(searchText, 1, 100);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _games = response.Data.ToList();
    }

    private async Task<IEnumerable<GameSlim>> FilterGames(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText) || filterText.Length < 3)
        {
            return _games;
        }

        await UpdateGames(filterText);
        return _games;
    }

    private void GenerateRandomName()
    {
        _createRequest.Name = NameHelpers.GenerateNameLong(true);
    }

    private async Task CreateGameProfile()
    {
        if (_selectedGame.Id == Guid.Empty)
        {
            Snackbar.Add("A game wasn't selected and is required", Severity.Error);
            return;
        }

        _createRequest.OwnerId = _loggedInUserId;
        var response = await GameServerService.CreateGameProfileAsync(_createRequest, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        MudDialog.Close(DialogResult.Ok(response.Data));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}