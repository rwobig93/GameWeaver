using Application.Models.GameServer.Game;
using Application.Models.GameServer.LocalResource;
using Application.Services.GameServer;
using Domain.Enums.GameServer;

namespace GameWeaver.Components.GameServer;

public partial class LocalResourceAddDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.AddToQueue;
    [Parameter] public Color IconColor { get; set; } = Color.Success;
    [Parameter] public Color TextColor { get; set; } = Color.Default;
    [Parameter] public string Title { get; set; } = "New Local Resource";
    [Parameter] public string ConfirmButtonText { get; set; } = "Add";
    [Parameter] public int IconWidthPixels { get; set; } = 75;
    [Parameter] public int IconHeightPixels { get; set; } = 75;
    [Parameter] public Guid GameProfileId { get; set; }
    [Parameter] public ResourceType ResourceType { get; set; }

    [Inject] public IGameService GameService { get; set; } = null!;
    [Inject] public IGameServerService GameServerService { get; set; } = null!;

    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";
    private readonly LocalResourceSlim _newLocalResource = new();
    private GameSlim _game = new() {Id = Guid.Empty};
    private const int TooltipDelay = 500;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UpdateResourceOnStartup();
            await GetGame();
            StateHasChanged();
        }
    }

    private async Task GetGame()
    {
        var gameProfile = await GameServerService.GetGameProfileByIdAsync(GameProfileId);
        if (!gameProfile.Succeeded || gameProfile.Data is null)
        {
            gameProfile.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            Cancel();
            return;
        }

        var profileGame = await GameService.GetByIdAsync(gameProfile.Data.GameId);
        if (!profileGame.Succeeded || profileGame.Data is null)
        {
            profileGame.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            Cancel();
            return;
        }

        _game = profileGame.Data;
    }

    private void UpdateResourceOnStartup()
    {
        _newLocalResource.Type = ResourceType;
    }

    private void InjectDynamicValue(string value)
    {
        _newLocalResource.Args += $" {value}";
    }

    private void Submit()
    {
        _newLocalResource.Id = Guid.CreateVersion7();
        _newLocalResource.GameProfileId = GameProfileId;
        MudDialog.Close(DialogResult.Ok(_newLocalResource));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}