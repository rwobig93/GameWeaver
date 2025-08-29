namespace GameWeaver.Components.Shared;

public partial class ConfirmationDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.Delete;
    [Parameter] public Color IconColor { get; set; } = Color.Error;
    [Parameter] public Color TextColor { get; set; } = Color.Default;
    [Parameter] public string Title { get; set; } = "Are you sure you want to do this!?";
    [Parameter] public string Content { get; set; } = "Stuff and things";
    [Parameter] public int IconWidthPixels { get; set; } = 75;
    [Parameter] public int IconHeightPixels { get; set; } = 75;
    [Parameter] public string ConfirmText { get; set; } = "Confirm";
    [Parameter] public string CancelText { get; set; } = "Cancel";

    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";


    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}