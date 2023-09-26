﻿using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Models.Identity.User;

namespace GameWeaver.Pages.Account;

public partial class AccountSettings
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    
    private AppUserFull CurrentUser { get; set; } = new();
    private bool _processingEmailChange;
    private bool _canChangeEmail;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetCurrentUser();
            await GetPermissions();
            StateHasChanged();
        }
    }

    private async Task GetCurrentUser()
    {
        var userId = await CurrentUserService.GetCurrentUserId();
        if (userId is null)
            return;

        CurrentUser = (await UserService.GetByIdFullAsync((Guid) userId)).Data!;
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canChangeEmail = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Users.ChangeEmail);
    }

    private async Task UpdateAccount()
    {
        var updatedAccount = CurrentUser.ToUpdate();
        var requestResult = await UserService.UpdateAsync(updatedAccount, CurrentUser.Id);
        if (!requestResult.Succeeded)
        {
            requestResult.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Account successfully updated!");
        StateHasChanged();
    }

    private async Task ChangeEmail()
    {
        if (!_canChangeEmail) return;
        
        var dialogParameters = new DialogParameters()
        {
            {"Title", "Confirm New Email Address"},
            {"FieldLabel", "New Email Address"}
        };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium, CloseOnEscapeKey = true };
        var newEmailPrompt = await DialogService.Show<ValuePromptDialog>("Confirm New Email", dialogParameters, dialogOptions).Result;
        if (newEmailPrompt.Canceled || string.IsNullOrWhiteSpace((string?)newEmailPrompt.Data))
            return;

        var newEmailAddress = (string)newEmailPrompt.Data;
        _processingEmailChange = true;
        StateHasChanged();
        var emailChangeRequest = await AccountService.InitiateEmailChange(CurrentUser.Id, newEmailAddress);
        if (!emailChangeRequest.Succeeded)
        {
            _processingEmailChange = false;
            StateHasChanged();
            emailChangeRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _processingEmailChange = false;
        StateHasChanged();
        Snackbar.Add(emailChangeRequest.Messages.First(), Severity.Success);
    }
}