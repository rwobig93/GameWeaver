using Application.Models.GameServer.LocalResource;

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

    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";
    private readonly LocalResourceSlim _newLocalResource = new();
    private const int TooltipDelay = 500;


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