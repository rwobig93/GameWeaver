using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.LocalResource;

namespace GameWeaver.Components.GameServer;

public partial class ConfigAddDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.AddToQueue;
    [Parameter] public Color IconColor { get; set; } = Color.Success;
    [Parameter] public Color TextColor { get; set; } = Color.Default;
    [Parameter] public string Title { get; set; } = "New Configuration Item";
    [Parameter] public string ConfirmButtonText { get; set; } = "Add";
    [Parameter] public int IconWidthPixels { get; set; } = 75;
    [Parameter] public int IconHeightPixels { get; set; } = 75;
    [Parameter] public LocalResourceSlim ReferenceResource { get; set; } = null!;

    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";
    private readonly ConfigurationItemSlim _newConfigItem = new();
    

    private void Submit()
    {
        _newConfigItem.Id = Guid.NewGuid();
        _newConfigItem.LocalResourceId = ReferenceResource.Id;
        MudDialog.Close(DialogResult.Ok(_newConfigItem));
    }
    
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}