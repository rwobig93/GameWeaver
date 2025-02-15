using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.Identity.User;
using Application.Services.Integrations;
using Domain.Enums.Identity;
using GameWeaver.Components.Identity;

namespace GameWeaver.Pages.Admin;

public partial class UserAdmin
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private IAppUserService UserService { get; init; } = null!;
    [Inject] private IExcelService ExcelService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    
    private MudTable<AppUserSlim> _table = new();
    private IEnumerable<AppUserSlim> _pagedData = new List<AppUserSlim>();
    private HashSet<AppUserSlim> _selectedItems = [];
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private string _searchString = "";
    private int _totalUsers;

    private Guid _currentUserId;
    private bool _canEnableUsers;
    private bool _canDisableUsers;
    private bool _canResetPasswords;
    private bool _allowUserSelection;
    private bool _canAdminServiceAccounts;
    private bool _canExportUsers;
    private bool _canForceLogin;

    private bool _dense = true;
    private bool _hover = true;
    private bool _striped = true;
    private bool _bordered;
    private bool _filterLockedOut;
    private bool _filterDisabled;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await GetClientTimezone();
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = await CurrentUserService.GetCurrentUserPrincipal();
        if (currentUser is null)
        {
            return;
        }
        
        _currentUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canResetPasswords = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Users.ResetPassword);
        _canDisableUsers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Users.Disable);
        _canEnableUsers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Users.Enable);
        _canAdminServiceAccounts = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.ServiceAccounts.Admin);
        _canExportUsers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Users.Export);
        _canForceLogin = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Users.ForceLogin);

        _allowUserSelection = _canResetPasswords || _canDisableUsers || _canEnableUsers || _canForceLogin;
    }
    
    private async Task<TableData<AppUserSlim>> ServerReload(TableState state, CancellationToken token)
    {
        var usersResult = await UserService.SearchPaginatedAsync(_searchString, state.Page, state.PageSize);
        if (!usersResult.Succeeded)
        {
            usersResult.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return new TableData<AppUserSlim>();
        }

        _pagedData = usersResult.Data.ToArray();
        _totalUsers = (await UserService.GetCountAsync()).Data;

        if (_filterDisabled)
            _pagedData = _pagedData.Where(x => x.AuthState == AuthState.Disabled);
        if (_filterLockedOut)
            _pagedData = _pagedData.Where(x => x.AuthState == AuthState.LockedOut);

        _pagedData = state.SortLabel switch
        {
            "Id" => _pagedData.OrderByDirection(state.SortDirection, o => o.Id),
            "Username" => _pagedData.OrderByDirection(state.SortDirection, o => o.Username),
            "Enabled" => _pagedData.OrderByDirection(state.SortDirection, o => o.AuthState),
            "AccountType" => _pagedData.OrderByDirection(state.SortDirection, o => o.AccountType),
            "Notes" => _pagedData.OrderByDirection(state.SortDirection, o => o.Notes),
            _ => _pagedData
        };
        
        return new TableData<AppUserSlim>() {TotalItems = _totalUsers, Items = _pagedData};
    }

    private async Task SearchText(string text)
    {
        _searchString = text;
        await _table.ReloadServerData();
        _selectedItems = [];
        StateHasChanged();
    }

    private async Task ReloadSearch()
    {
        await SearchText(_searchString);
    }

    private async Task ClearSearch()
    {
        _filterDisabled = false;
        _filterLockedOut = false;
        await SearchText("");
    }

    private async Task EnableAccounts()
    {
        if (!_canEnableUsers)
        {
            Snackbar.Add("You don't have permission to enable accounts, how'd you initiate this request!?", Severity.Error);
            return;
        }

        foreach (var account in _selectedItems)
        {
            var result = await AccountService.SetAuthState(account.Id, AuthState.Enabled);
            if (!result.Succeeded)
                result.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            else
            {
                result.Messages.ForEach(x => Snackbar.Add(x, Severity.Success));
                await SearchText(_searchString);
            }
        }
    }

    private async Task DisableAccounts()
    {
        if (!_canDisableUsers)
        {
            Snackbar.Add("You don't have permission to disable accounts, how'd you initiate this request!?", Severity.Error);
            return;
        }

        foreach (var account in _selectedItems)
        {
            var result = await AccountService.SetAuthState(account.Id, AuthState.Disabled);
            if (!result.Succeeded)
                result.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            else
            {
                result.Messages.ForEach(x => Snackbar.Add(x, Severity.Success));
                await SearchText(_searchString);
            }
        }
    }

    private async Task ForcePasswordResets()
    {
        if (!_canResetPasswords)
        {
            Snackbar.Add("You don't have permission to reset passwords, how'd you initiate this request!?", Severity.Error);
            return;
        }

        foreach (var account in _selectedItems)
        {
            if (account.AccountType != AccountType.User)
            {
                Snackbar.Add($"Account {account.Username} is a service account, can't force a password reset {account.AccountType} accounts",
                    Severity.Error);
                continue;
            }
            
            var result = await AccountService.ForceUserPasswordReset(account.Id, _currentUserId);
            if (!result.Succeeded)
                result.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            else
                result.Messages.ForEach(x => Snackbar.Add(x, Severity.Success));
        }
    }

    private async Task ForceLogin()
    {
        if (!_canForceLogin)
        {
            Snackbar.Add("You don't have permission to reset passwords, how'd you initiate this request!?", Severity.Error);
            return;
        }

        List<string> errorMessages = [];

        foreach (var account in _selectedItems)
        {
            if (account.AccountType != AccountType.User)
            {
                Snackbar.Add($"Account {account.Username} is a service account, can't force a login for {account.AccountType} accounts",
                    Severity.Error);
                continue;
            }
            
            var result = await AccountService.ForceUserLogin(account.Id, _currentUserId);
            if (!result.Succeeded)
            {
                errorMessages.AddRange(result.Messages);
            }
        }

        if (errorMessages.Count != 0)
        {
            errorMessages.ForEach(x => Snackbar.Add(x, Severity.Error));
        }

        Snackbar.Add("Finished clearing device session for selected accounts", Severity.Success);
    }

    private void ViewUser(Guid userId)
    {
        var viewUserUri = QueryHelpers.AddQueryString(AppRouteConstants.Admin.UserView, "userId", userId.ToString());
        NavManager.NavigateTo(viewUserUri);
    }

    private async Task CreateServiceAccount()
    {
        if (!_canAdminServiceAccounts)
        {
            Snackbar.Add("You don't have permission to create accounts, how'd you initiate this request!?", Severity.Error);
            return;
        }
        
        var createParameters = new DialogParameters() { {"ServiceAccountId", Guid.Empty} };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var createAccountDialog = await DialogService.ShowAsync<ServiceAccountAdminDialog>(
            "Create Service Account", createParameters, dialogOptions);
        var dialogResult = await createAccountDialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var createdPassword = (string?) dialogResult.Data;
        if (string.IsNullOrWhiteSpace(createdPassword))
        {
            Snackbar.Add("Something happened and we couldn't retrieve the password for this account, please contact the administrator",
                Severity.Error);
            await ReloadSearch();
            return;
        }
        
        var copyParameters = new DialogParameters()
        {
            {"Title", "Please copy the account password and save it somewhere safe"},
            {"FieldLabel", "Service Account Password"},
            {"TextToDisplay", new string('*', createdPassword.Length)},
            {"TextToCopy", createdPassword}
        };
        await DialogService.ShowAsync<CopyTextDialog>("Service Account Password", copyParameters, dialogOptions);
        await ReloadSearch();
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private async Task ExportSelectedToExcel()
    {
        if (!_canExportUsers) return;
        
        var convertedExcelWorkbook = await ExcelService.ExportBase64Async(
            _pagedData, dataMapping: new Dictionary<string, Func<AppUserSlim, object>>
            {
                { "Id", user => user.Id },
                { "AccountType", user => user.AccountType.ToString() },
                { "AuthState", user => user.AuthState.ToString() },
                { "CreatedBy", user => user.CreatedBy.ToString() },
                { "CreatedOn", user => user.CreatedOn.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) },
                { "LastModifiedBy", user => user.LastModifiedBy.GetFromNullable() },
                { "LastModifiedOn", user => user.LastModifiedOn?.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) ?? "Never" },
                { "Notes", user => user.Notes ?? "" }
            }, sheetName: "Users");

        var fileName =
            $"SelectedUsers_{DateTimeService.NowDatabaseTime.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat)}.xlsx";

        await WebClientService.InvokeFileDownload(convertedExcelWorkbook, fileName, DataConstants.MimeTypes.OpenXml);

        Snackbar.Add("Successfully exported all Users to Excel Workbook For Download", Severity.Success);
    }

    private async Task ExportAllToExcel()
    {
        if (!_canExportUsers) return;

        var allUsers = await UserService.GetAllAsync();

        if (!allUsers.Succeeded)
        {
            allUsers.Messages.ForEach(x => Snackbar.Add(x));
            return;
        }
        
        var convertedExcelWorkbook = await ExcelService.ExportBase64Async(
            allUsers.Data, dataMapping: new Dictionary<string, Func<AppUserSlim, object>>
            {
                { "Id", user => user.Id },
                { "AccountType", user => user.AccountType.ToString() },
                { "AuthState", user => user.AuthState.ToString() },
                { "CreatedBy", user => user.CreatedBy.ToString() },
                { "CreatedOn", user => user.CreatedOn.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) },
                { "LastModifiedBy", user => user.LastModifiedBy.GetFromNullable() },
                { "LastModifiedOn", user => user.LastModifiedOn?.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) ?? "Never" },
                { "Notes", user => user.Notes ?? "" }
            }, sheetName: "Users");

        var fileName =
            $"AllUsers_{DateTimeService.NowDatabaseTime.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat)}.xlsx";

        await WebClientService.InvokeFileDownload(convertedExcelWorkbook, fileName, DataConstants.MimeTypes.OpenXml);

        Snackbar.Add("Successfully exported all Users to Excel Workbook For Download", Severity.Success);
    }
}