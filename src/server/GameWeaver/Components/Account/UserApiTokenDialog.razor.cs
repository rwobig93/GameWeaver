using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.Identity.UserExtensions;
using Domain.Enums.Identity;

namespace GameWeaver.Components.Account;

public partial class UserApiTokenDialog
{
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;

    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public Guid ApiTokenId { get; set; } = Guid.Empty;

    private bool _creatingNewToken;
    private string _saveButtonText = "Create API Token";
    private Guid _currentUserId = Guid.Empty;
    private UserApiTokenTimeframe _tokenTimeframe = UserApiTokenTimeframe.OneYear;
    private AppUserExtendedAttributeSlim _apiToken = new();

    private bool _canGenerateTokens;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetCurrentUser();
            await GetPermissions();
            await ValidateTokenAction();
            StateHasChanged();
        }
    }

    private async Task GetCurrentUser()
    {
        var foundUserId = await CurrentUserService.GetCurrentUserId();
        if (foundUserId is null)
            return;

        _currentUserId = Guid.Parse(foundUserId.ToString()!);
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canGenerateTokens = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Api.GenerateToken);
    }

    private async Task ValidateTokenAction()
    {
        if (!_canGenerateTokens) return;
        
        var existingTokenRequest = await UserService.GetExtendedAttributeByIdAsync(ApiTokenId);
        if (!existingTokenRequest.Succeeded || existingTokenRequest.Data is null)
        {
            if (ApiTokenId != Guid.Empty)
                Snackbar.Add("Token ID provided doesn't exist, you can create a new one though", Severity.Error);

            _creatingNewToken = true;
            _apiToken.Description = $"New Token - {DateTimeService.NowDatabaseTime.ToFriendlyDisplayMilitaryTimezone()}";
            await Task.CompletedTask;
            return;
        }

        _apiToken = existingTokenRequest.Data;
        _saveButtonText = "Update API Token";
        _creatingNewToken = false;
    }

    private async Task Save()
    {
        if (!_canGenerateTokens) return;

        // Creating a new token
        if (_creatingNewToken)
        {
            var generateRequest = await AccountService.GenerateUserApiToken(_currentUserId, _tokenTimeframe, _apiToken.Description);
            if (!generateRequest.Succeeded)
            {
                generateRequest.Messages.ForEach(x => Snackbar.Add($"Failed to generate token: {x}", Severity.Error));
                return;
            }

            Snackbar.Add("Successfully created API Token!", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
            return;
        }
        
        // Updating an existing token - we only allow updating the description
        var tokenUpdateRequest = await UserService.UpdateExtendedAttributeAsync(_apiToken.Id, null, _apiToken.Description);
        if (!tokenUpdateRequest.Succeeded)
        {
            tokenUpdateRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Successfully updated API Token!", Severity.Success);
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}