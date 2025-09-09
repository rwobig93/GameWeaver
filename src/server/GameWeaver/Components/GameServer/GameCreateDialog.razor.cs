using Application.Constants.Communication;
using Application.Helpers.Lifecycle;
using Application.Mappers.GameServer;
using Application.Requests.GameServer.Game;
using Application.Responses.v1.Identity;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Enums.GameServer;
using Domain.Enums.Lifecycle;

namespace GameWeaver.Components.GameServer;

public partial class GameCreateDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Title { get; set; } = "Create A New Game";
    [Parameter] public string ConfirmButtonText { get; set; } = "Create Game";

    [Inject] public IGameService GameService { get; init; } = null!;
    [Inject] public ITroubleshootingRecordService TshootService { get; init; } = null!;

    private GameCreateRequest _gameRequest = new();
    private UserBasicResponse _loggedInUser = new();


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetLoggedInUser();
        }
    }

    private async Task GetLoggedInUser()
    {
        var user = await CurrentUserService.GetCurrentUserBasic();
        if (user is null)
        {
            StateHasChanged();
            return;
        }

        _loggedInUser = user;
        StateHasChanged();
    }

    private async Task CreateGame()
    {
        if (string.IsNullOrWhiteSpace(_gameRequest.Name))
        {
            Snackbar.Add("Game name cannot be empty", Severity.Error);
            return;
        }

        if (_gameRequest.SourceType is GameSource.Steam && string.IsNullOrWhiteSpace(_gameRequest.SteamToolId.ToString()))
        {
            Snackbar.Add("Steam games require a tool id", Severity.Error);
            return;
        }

        if (_gameRequest is {SupportsWindows: false, SupportsLinux: false, SupportsMac: false})
        {
            Snackbar.Add("At least one platform must be supported", Severity.Error);
            return;
        }

        if (_loggedInUser.Id == Guid.Empty)
        {
            var tshootId = await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Games, Guid.Empty, _loggedInUser.Id,
                "Failed to create new game server", new Dictionary<string, string>
                {
                    {"GameName", _gameRequest.Name},
                    {"Description", _gameRequest.Description},
                    {"SteamToolId", _gameRequest.SteamToolId.ToString()},
                    {"SteamGameId", _gameRequest.SteamGameId.ToString()},
                    {"SupportsWindows", _gameRequest.SupportsWindows.ToString()},
                    {"SupportsLinux", _gameRequest.SupportsLinux.ToString()},
                    {"SupportsMac", _gameRequest.SupportsMac.ToString()},
                    {"Error", "Logged in user Id is empty, this shouldn't happen, we can't generate a game server without knowing who is submitting the request"}
                });
            Snackbar.Add(ErrorMessageConstants.Generic.ContactAdmin, Severity.Error);
            Snackbar.Add(ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data), Severity.Error);
            return;
        }

        // TODO: Version URL & Manual file uploads will occur on the game view page rather than this dialog
        var response = await GameService.CreateAsync(_gameRequest.ToCreate(), _loggedInUser.Id);
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