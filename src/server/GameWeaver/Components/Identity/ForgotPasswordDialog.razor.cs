using Application.Constants.Communication;
using Application.Helpers.Identity;
using Application.Requests.v1.Identity.User;
using Application.Services.Lifecycle;

namespace GameWeaver.Components.Identity;

public partial class ForgotPasswordDialog
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;

    private string EmailAddress { get; set; } = "";
    private bool PageIsLoading { get; set; }

    private async Task SendPasswordReset()
    {
        PageIsLoading = true;
        var validEmailAddress = AccountHelpers.IsValidEmailAddress(EmailAddress);
        if (!validEmailAddress)
        {
            Snackbar.Add(ErrorMessageConstants.InvalidValueError);
            PageIsLoading = false;
            return;
        }
        
        var emailExists = await DoesEmailExist();
        if (!emailExists)
        {
            Snackbar.Add(ErrorMessageConstants.GenericErrorContactAdmin);
            PageIsLoading = false;
            return;
        }

        var resetResponse = await AccountService.ForgotPasswordAsync(new ForgotPasswordRequest
        {
            Email = EmailAddress
        });
        if (!resetResponse.Succeeded)
        {
            resetResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            PageIsLoading = false;
            return;
        }
        
        resetResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Success));
        PageIsLoading = false;
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Close();
    }

    private async Task<bool> DoesEmailExist()
    {
        var foundUser = await UserService.GetByEmailAsync(EmailAddress);
        return foundUser.Data is not null;
    }

    private void DebugFillEmail()
    {
        EmailAddress = "wingman@wobigtech.net";
    }
}