using Domain.Enums.Identity;
using Domain.Enums.Integrations;
using GameWeaver.Components.Identity;

namespace GameWeaver.Helpers;

public static class UiHelpers
{
    public static async Task<DialogResult> ConfirmDialog(this IDialogService dialogService, string title, string content)
    {
        var dialogOptions = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialogParameters = new DialogParameters { {"Title", title}, {"Content", content} };

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

    public static async Task<DialogResult> DynamicPermissionsAddDialog(this IDialogService dialogService, string title, Guid entityId, DynamicPermissionGroup group,
        bool canPermissionEntity, bool isForRolesNotUsers = true)
    {
        var dialogOptions = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialogParameters = new DialogParameters
        {
            {"EntityId", entityId}, {"Group", group}, {"CanPermissionEntity", canPermissionEntity}, {"IsForRolesNotUsers", isForRolesNotUsers}
        };

        var dialog = await dialogService.ShowAsync<DynamicPermissionAddDialog>(title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        return dialogResult ?? DialogResult.Cancel();
    }
}