using Application.Services.Integrations;

namespace GameWeaver.Components.Identity;

public partial class MfaCodeValidationDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string MfaKey { get; set; } = "";
    [Parameter] public string VerifyCodeMessage { get; set; } = "Please enter your MFA code";
    [Inject] private IMfaService MfaService { get; init; } = null!;

    private string _mfaCode = "";

    private void VerifyMfaCode()
    {
        var validToken = MfaService.IsPasscodeCorrect(_mfaCode, MfaKey, out _);
        if (!validToken)
        {
            Snackbar.Add("MFA Code entered is invalid, please try again", Severity.Error);
            return;
        }

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void MfaCodeTextfieldKeyDown(KeyboardEventArgs keyDown)
    {
        switch (keyDown.Key)
        {
            case "Enter":
                VerifyMfaCode();
                break;
            case "Escape":
                Cancel();
                break;
        }
    }

    private void Cancel()
    {
        MudDialog.Close(DialogResult.Cancel());
    }
}