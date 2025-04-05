namespace GameWeaver.Helpers;

public static class UiHelpers
{
    public static async Task<DialogResult> ConfirmDialog(this IDialogService dialogService, string title, string content)
    {
        var dialogParameters = new DialogParameters { {"Title", title}, {"Content", content} };
        var dialogOptions = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };

        var dialog = await dialogService.ShowAsync<ConfirmationDialog>(title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        return dialogResult ?? DialogResult.Cancel();
    }
}