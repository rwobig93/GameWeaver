using Domain.Enums.Identity;
using Domain.Enums.Integrations;
using GameWeaver.Components.GameServer;
using GameWeaver.Components.Identity;

namespace GameWeaver.Helpers;

public static class UiHelpers
{
    public static async Task<DialogResult> ConfirmDialog(this IDialogService dialogService, string title, string content, string confirmButtonText = "Confirm",
        string cancelButtonText = "Cancel")
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters
        {
            {"Title", title}, {"Content", content}, {"ConfirmText", confirmButtonText}, {"CancelText", cancelButtonText}
        };

        var dialog = await dialogService.ShowAsync<ConfirmationDialog>(title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        return dialogResult ?? DialogResult.Cancel();
    }

    public static async Task<DialogResult> FileEditorDialog(this IDialogService dialogService, string fileName, string content,
        FileEditorLanguage language = FileEditorLanguage.Plaintext, bool canEdit = true, bool showGameQuickActions = false)
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, CloseOnEscapeKey = true, FullWidth = true};
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
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters
        {
            {"EntityId", entityId}, {"Group", group}, {"CanPermissionEntity", canPermissionEntity}, {"IsForRolesNotUsers", isForRolesNotUsers}
        };

        var dialog = await dialogService.ShowAsync<DynamicPermissionAddDialog>(title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        return dialogResult ?? DialogResult.Cancel();
    }

    public static async Task<DialogResult> ChangeOwnershipDialog(this IDialogService dialogService, string title, Guid currentOwnerId, string confirmButtonText)
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters
        {
            {"Title", title}, {"OwnerId", currentOwnerId}, {"ConfirmButtonText", confirmButtonText}
        };

        var dialog = await dialogService.ShowAsync<ChangeOwnershipDialog>(title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        return dialogResult ?? DialogResult.Cancel();
    }

    public static async Task<DialogResult> CreateGameProfileDialog(this IDialogService dialogService, string confirmButtonText = "Create Profile",
        string title = "New Configuration Profile")
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters
        {
            {"Title", title}, {"ConfirmButtonText", confirmButtonText}
        };

        var dialog = await dialogService.ShowAsync<GameProfileCreateDialog>(title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        return dialogResult ?? DialogResult.Cancel();
    }

    public static async Task<DialogResult> MessageDialog(this IDialogService dialogService, string title, string content, string buttonText = "Got it", string? icon = null,
        Color iconColor = Color.Info, Color textColor = Color.Default, int iconWidthPx = 75, int iconHeightPx = 75)
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters
        {
            {"Title", title}, {"Content", content}, {"ButtonText", buttonText}, {"Icon", icon}, {"IconColor", iconColor}, {"TextColor", textColor},
            {"IconWidthPixels", iconWidthPx}, {"IconHeightPixels", iconHeightPx}
        };

        var dialog = await dialogService.ShowAsync<ConfirmationDialog>(title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        return dialogResult ?? DialogResult.Cancel();
    }

    public static async Task<DialogResult> CreateGameDialog(this IDialogService dialogService, string confirmButtonText = "Create Game", string title = "New Game")
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters
        {
            {"Title", title}, {"ConfirmButtonText", confirmButtonText}
        };

        var dialog = await dialogService.ShowAsync<GameCreateDialog>(title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        return dialogResult ?? DialogResult.Cancel();
    }
}