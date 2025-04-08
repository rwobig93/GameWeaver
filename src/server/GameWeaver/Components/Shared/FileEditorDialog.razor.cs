using Application.Helpers.Integrations;
using BlazorMonaco.Editor;
using Domain.Enums.Integrations;
using Microsoft.JSInterop;

namespace GameWeaver.Components.Shared;

public partial class FileEditorDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Title { get; set; } = "File Editor";
    [Parameter] public string FileContent { get; set; } = string.Empty;
    [Parameter] public FileEditorLanguage Language { get; set; } = FileEditorLanguage.Json;
    [Parameter] public FileEditorTheme Theme { get; set; } = FileEditorTheme.Dark;
    [Parameter] public bool CanEdit { get; set; } = true;
    [Parameter] public bool ShowGameQuickActions { get; set; }

    [Inject] public IJSRuntime JsRuntime { get; init; } = null!;
    [Inject] public IWebClientService WebClientService { get; init; } = null!;

    private StandaloneCodeEditor _editor = null!;
    private bool _initialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.CompletedTask;
        }
    }

    private StandaloneEditorConstructionOptions EditorOptions(StandaloneCodeEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            AutomaticLayout = true,
            AutoDetectHighContrast = true,
            TrimAutoWhitespace = true,
            Language = Language.ToEditorValue(),
            Theme = Theme.ToEditorValue()
        };
    }

    private async Task EditorLanguageChanged(IEnumerable<FileEditorLanguage>? languages)
    {
        if (!_initialized) return;
        Language = languages?.FirstOrDefault() ?? Language;
        await Global.SetModelLanguage(JsRuntime, await _editor.GetModel(), Language.ToEditorValue());
    }

    private async Task EditorThemeChanged(IEnumerable<FileEditorTheme>? themes)
    {
        if (!_initialized) return;
        Theme = themes?.FirstOrDefault() ?? Theme;
        await Global.SetTheme(JsRuntime, Theme.ToEditorValue());
    }

    private async Task EditorInitialize()
    {
        _initialized = true;
        await _editor.SetValue(FileContent);
        await _editor.UpdateOptions(new EditorUpdateOptions {ReadOnly = !CanEdit});
    }

    private async Task CopyToClipboard(string text)
    {
        await WebClientService.InvokeClipboardCopy(text);
        Snackbar.Add("Text copied to your clipboard!", Severity.Success);
    }

    private async Task Confirm()
    {
        if (!CanEdit)
        {
            MudDialog.Cancel();
            return;
        }

        var updatedContent = await _editor.GetValue();
        MudDialog.Close(DialogResult.Ok(updatedContent));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}