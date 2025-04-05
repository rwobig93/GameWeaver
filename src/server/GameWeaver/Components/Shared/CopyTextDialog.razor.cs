namespace GameWeaver.Components.Shared;

public partial class CopyTextDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.ContentCopy;
    [Parameter] public Color IconColor { get; set; } = Color.Error;
    [Parameter] public Color TextColor { get; set; } = Color.Default;
    [Parameter] public string Title { get; set; } = "Please copy this text, it's probably important";
    [Parameter] public string FieldLabel { get; set; } = "The Copy Thing";
    [Parameter] public string TextToDisplay { get; set; } = @"¯\_(ツ)_/¯";
    [Parameter] public string TextToCopy { get; set; } = @"¯\_(ツ)_/¯";
    [Parameter] public int IconWidthPixels { get; set; } = 75;
    [Parameter] public int IconHeightPixels { get; set; } = 75;

    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";


    private async Task CopyToClipboard()
    {
        var copyRequest = await WebClientService.InvokeClipboardCopy(TextToCopy);
        if (!copyRequest.Succeeded)
        {
            copyRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Successfully copied to your clipboard!", Severity.Success);
    }

    private void Close()
    {
        MudDialog.Cancel();
    }
}