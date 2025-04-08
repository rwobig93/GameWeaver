using Domain.Enums.Integrations;

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

    public static async Task<DialogResult> FileEditorDialog(this IDialogService dialogService, string fileName, string content,
        FileEditorLanguage language = FileEditorLanguage.Plaintext, bool canEdit = true, bool showGameQuickActions = false)
    {
        var dialogOptions = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, CloseOnEscapeKey = true, FullWidth = true};
        var dialogParameters = new DialogParameters
        {
            {"Title", fileName}, {"FileContent", content}, {"Language", language}, {"CanEdit", canEdit}, {"ShowGameQuickActions", showGameQuickActions}
        };
        var dialog = await dialogService.ShowAsync<FileEditorDialog>(null, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        return dialogResult ?? DialogResult.Cancel();
    }
}