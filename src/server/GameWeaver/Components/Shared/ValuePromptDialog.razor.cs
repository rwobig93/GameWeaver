namespace GameWeaver.Components.Shared;

public partial class ValuePromptDialog
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.Info;
    [Parameter] public Color IconColor { get; set; } = Color.Error;
    [Parameter] public Color TextColor { get; set; } = Color.Default;
    [Parameter] public string Title { get; set; } = "Please provide the thing";
    [Parameter] public string FieldLabel { get; set; } = "The Thing";
    [Parameter] public string ConfirmButtonText { get; set; } = "Confirm";
    [Parameter] public int IconWidthPixels { get; set; } = 75;
    [Parameter] public int IconHeightPixels { get; set; } = 75;

    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";
    private string _returnValue = "";

    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(_returnValue));
    }
    
    private void Cancel()
    {
        MudDialog.Cancel();
    }


    private void TextFieldKeyDown(KeyboardEventArgs keyArgs)
    {
        if (keyArgs.Key == "Enter")
            Confirm();
    }
}