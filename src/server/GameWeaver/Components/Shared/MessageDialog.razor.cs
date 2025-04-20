namespace GameWeaver.Components.Shared;

public partial class MessageDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string? Icon { get; set; }
    [Parameter] public Color IconColor { get; set; } = Color.Error;
    [Parameter] public Color TextColor { get; set; } = Color.Default;
    [Parameter] public string Title { get; set; } = "Informational Message";
    [Parameter] public string Content { get; set; } = "This message brought to you by Gameweaver!";
    [Parameter] public int IconWidthPixels { get; set; } = 75;
    [Parameter] public int IconHeightPixels { get; set; } = 75;
    [Parameter] public string ButtonText { get; set; } = "Got it";

    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";


    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }
}