using Application.Constants.Communication;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Requests.Integrations;
using Application.Responses.v1.Identity;
using Application.Services.GameServer;
using Application.Services.Integrations;
using Application.Services.Lifecycle;
using Domain.Enums.Integrations;
using Domain.Enums.Lifecycle;
using Microsoft.AspNetCore.Components.Forms;

namespace GameWeaver.Components.Game;

public partial class GameVersionCreateDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Title { get; set; } = "Add a new version";
    [Parameter] public string ConfirmButtonText { get; set; } = "Create Version";
    [Parameter] public Guid GameId { get; set; } = Guid.Empty;

    [Inject] public IGameService GameService { get; init; } = null!;
    [Inject] public ITroubleshootingRecordService TshootService { get; init; } = null!;
    [Inject] private IFileStorageRecordService FileService { get; init; } = null!;

    private readonly FileStorageRecordCreateRequest _versionRequest = new();
    private UserBasicResponse _loggedInUser = new();
    private bool _uploadingFile;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // TODO: Validate GameId is valid then assign to _gameRequest.LinkedId
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

    private async Task UploadFileSelected(IBrowserFile? uploadFile)
    {
        if (uploadFile is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_versionRequest.Version))
        {
            Snackbar.Add("Version is required before uploading", Severity.Error);
            return;
        }

        if (uploadFile.Size > 100_000_000_000)
        {
            Snackbar.Add($"File is over the max size of 100GB: {uploadFile.Name}");
            return;
        }

        var gameVersionFiles = await FileService.GetByLinkedIdAsync(GameId);
        if (!gameVersionFiles.Succeeded)
        {
            gameVersionFiles.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        var matchingVersionFile = gameVersionFiles.Data.FirstOrDefault(x => x.Version == _versionRequest.Version);
        if (matchingVersionFile is not null)
        {
            Snackbar.Add("Provided version already exists for this game", Severity.Error);
            return;
        }

        await UploadVersionFile(uploadFile);
    }

    private async Task UploadVersionFile(IBrowserFile uploadFile)
    {
        Snackbar.Add("Uploading your game version file, this may take a while...", Severity.Info);
        _uploadingFile = true;
        StateHasChanged();

        var fileContent = await uploadFile.GetContent(100_000_000_000);
        if (!fileContent.Succeeded || fileContent.Data is null)
        {
            _uploadingFile = false;
            fileContent.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            StateHasChanged();
            return;
        }

        // TODO: Look at better option for file content streaming, due to the file sizes we will need to stream the file to storage
        //   rather than loading all of it into memory
        _uploadingFile = true;
        var uploadRequest = await FileService.CreateAsync(new FileStorageRecordCreateRequest
        {
            Format = FileStorageFormat.Binary,
            LinkedType = FileStorageType.Game,
            LinkedId = GameId,
            FriendlyName = _versionRequest.FriendlyName,
            Filename = Guid.NewGuid().ToString(),
            Description = _versionRequest.Description,
            Version = _versionRequest.Version,
            SizeBytes = uploadFile.Size
        }, uploadFile.OpenReadStream(50_000_000_000), _loggedInUser.Id);
        if (!uploadRequest.Succeeded)
        {
            _uploadingFile = false;
            uploadRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            StateHasChanged();
            return;
        }

        _uploadingFile = false;
        Snackbar.Add($"Successfully uploaded game version {_versionRequest.Version}!", Severity.Success);
        StateHasChanged();
    }

    private async Task CreateGameVersion()
    {
        if (string.IsNullOrWhiteSpace(_versionRequest.FriendlyName))
        {
            Snackbar.Add("Game name cannot be empty", Severity.Error);
            return;
        }

        if (_loggedInUser.Id == Guid.Empty)
        {
            var tshootId = await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.Games, Guid.Empty, _loggedInUser.Id,
                "Failed to create new game version", new Dictionary<string, string>
                {
                    {"GameId", _versionRequest.LinkedId.ToString()},
                    {"VersionName", _versionRequest.FriendlyName},
                    {"Description", _versionRequest.Description},
                    {"Version", _versionRequest.Version},
                    {"VersionFormat", _versionRequest.Format.ToString()},
                    {"Error", "Logged in user Id is empty, this shouldn't happen, we can't create a new game version without knowing who is submitting the request"}
                });
            Snackbar.Add(ErrorMessageConstants.Generic.ContactAdmin, Severity.Error);
            Snackbar.Add(ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data), Severity.Error);
            return;
        }

        // TODO: Handle version creation separately from file upload

        // TODO: Handle game version creation from URL vs file upload

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}