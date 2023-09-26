using Application.Helpers.Identity;
using Application.Responses.Identity;

namespace GameWeaver.Pages.Identity;

public partial class ConfirmPasswordReset
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    [Parameter] public string UserId { get; set; } = "";
    [Parameter] public string ConfirmationCode { get; set; } = "";
    
    [Inject] private IAppAccountService AccountService { get; set; } = null!;
    
    private bool IsCodeValid { get; set; }
    private bool HasValidationRan { get; set; }
    private bool PasswordChanged { get; set; }
    private Guid ConvertedUserId { get; set; } = Guid.Empty;
    private string DesiredPassword { get; set; } = "";
    private string ConfirmPassword { get; set; } = "";
    private readonly PasswordRequirementsResponse _passwordRequirements = AccountHelpers.GetPasswordRequirements();
    
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private InputType _passwordConfirmInput = InputType.Password;
    private string _passwordConfirmInputIcon = Icons.Material.Filled.VisibilityOff;
    

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ParseParametersFromUri();
            ValidateProvidedInformation();
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

    private void ValidateProvidedInformation()
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

        ConvertedUserId = convertedUserId;
        IsCodeValid = true;
    }

    private void TogglePasswordVisibility()
    {
        if (_passwordInputIcon == Icons.Material.Filled.VisibilityOff)
        {
            _passwordInput = InputType.Text;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            return;
        }
        
        _passwordInput = InputType.Password;
        _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    }

    private void ToggleConfirmPasswordVisibility()
    {
        if (_passwordConfirmInputIcon == Icons.Material.Filled.VisibilityOff)
        {
            _passwordConfirmInput = InputType.Text;
            _passwordConfirmInputIcon = Icons.Material.Filled.Visibility;
            return;
        }
        
        _passwordConfirmInput = InputType.Password;
        _passwordConfirmInputIcon = Icons.Material.Filled.VisibilityOff;
    }

    private async Task ChangePassword()
    {
        var confirmationResponse = await AccountService.ForgotPasswordConfirmationAsync(
            ConvertedUserId, ConfirmationCode, DesiredPassword, ConfirmPassword);
        if (!confirmationResponse.Succeeded)
        {
            confirmationResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        confirmationResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Success));
        PasswordChanged = true;
    }
    
    private IEnumerable<string> ValidatePasswordRequirements(string content)
    {
        var passwordIssues = AccountHelpers.GetAnyIssuesWithPassword(content);
        if (!string.IsNullOrEmpty(content) && passwordIssues.Any())
            yield return passwordIssues.First();
    }
    
    private IEnumerable<string> ValidatePasswordsMatch(string content)
    {
        if (!string.IsNullOrEmpty(content) &&
            !string.IsNullOrWhiteSpace(DesiredPassword) &&
            content != DesiredPassword)
            yield return "Desired & Confirm Passwords Don't Match";
    }
}