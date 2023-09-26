namespace GameWeaver.Pages.Identity;

public partial class ConfirmEmail
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    
    [Parameter] public string UserId { get; set; } = "";
    [Parameter] public string ConfirmationCode { get; set; } = "";
    
    [Inject] private IAppAccountService AccountService { get; set; } = null!;
    
    private bool IsCodeValid { get; set; }
    private bool HasValidationRan { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ParseParametersFromUri();
            await StartEmailConfirmation();
            StateHasChanged();
        }

        await Task.CompletedTask;
    }

    private void ParseParametersFromUri()
    {
        var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
        var queryParameters = QueryHelpers.ParseQuery(uri.Query);
        
        if (queryParameters.TryGetValue("userId", out var userId))
            UserId = userId!;
        if (queryParameters.TryGetValue("confirmationCode", out var confirmationCode))
            ConfirmationCode = confirmationCode!;
    }

    private async Task StartEmailConfirmation()
    {
        HasValidationRan = true;
        if (string.IsNullOrWhiteSpace(ConfirmationCode))
        {
            Snackbar.Add("The code provided is invalid", Severity.Error);
            IsCodeValid = false;
            return;
        }

        var isGuid = Guid.TryParse(UserId, out var convertedUserId);
        if (!isGuid)
        {
            Snackbar.Add("The code provided is invalid, please contact the administrator", Severity.Error);
            return;
        }
        
        var confirmationResponse = await AccountService.ConfirmEmailAsync(convertedUserId, ConfirmationCode);
        if (!confirmationResponse.Succeeded)
        {
            confirmationResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        IsCodeValid = true;
        confirmationResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Success));
    }
}