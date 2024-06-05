using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.Identity.Role;
using Application.Services.Integrations;
using GameWeaver.Components.Identity;

namespace GameWeaver.Pages.Admin;

public partial class RoleAdmin
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    [Inject] private IAppRoleService RoleService { get; init; } = null!;
    [Inject] private IExcelService ExcelService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    
    private MudTable<AppRoleSlim> _table = new();
    private IEnumerable<AppRoleSlim> _pagedData = new List<AppRoleSlim>();
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private string _searchString = "";
    private int _totalRoles;
    private bool _dense = true;
    private bool _hover = true;
    private bool _striped = true;
    private bool _bordered;

    private bool _canCreateRoles;
    private bool _canExportRoles;

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
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canCreateRoles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Roles.Create);
        _canExportRoles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Identity.Roles.Export);
    }
    
    private async Task<TableData<AppRoleSlim>> ServerReload(TableState state)
    {
        var rolesResult = await RoleService.SearchPaginatedAsync(_searchString, state.Page, state.PageSize);
        if (!rolesResult.Succeeded)
        {
            rolesResult.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return new TableData<AppRoleSlim>();
        }

        _pagedData = rolesResult.Data.ToArray();
        _totalRoles = (await RoleService.GetCountAsync()).Data;

        _pagedData = state.SortLabel switch
        {
            "Id" => _pagedData.OrderByDirection(state.SortDirection, o => o.Id),
            "Name" => _pagedData.OrderByDirection(state.SortDirection, o => o.Name),
            _ => _pagedData
        };
        
        return new TableData<AppRoleSlim>() {TotalItems = _totalRoles, Items = _pagedData};
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _table.ReloadServerData();
    }

    private void ViewRole(Guid roleId)
    {
        var viewRoleUri = QueryHelpers.AddQueryString(AppRouteConstants.Admin.RoleView, "roleId", roleId.ToString());
        NavManager.NavigateTo(viewRoleUri);
    }

    private async Task CreateRole()
    {
        if (!_canCreateRoles) return;
        
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var createRoleDialog = await DialogService.Show<RoleCreateDialog>("Create New Role", dialogOptions).Result;
        if (createRoleDialog.Canceled)
            return;

        var createdRoleId = (Guid) createRoleDialog.Data;
        var newRoleViewUrl = QueryHelpers.AddQueryString(AppRouteConstants.Admin.RoleView, "roleId", createdRoleId.ToString());
        NavManager.NavigateTo(newRoleViewUrl);
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
        if (!_canExportRoles) return;
        
        var convertedExcelWorkbook = await ExcelService.ExportBase64Async(
            _pagedData, dataMapping: new Dictionary<string, Func<AppRoleSlim, object>>
            {
                { "Id", role => role.Id },
                { "Name", role => role.Name },
                { "Description", role => role.Description },
                { "CreatedBy", role => role.CreatedBy.ToString() },
                { "CreatedOn", role => role.CreatedOn.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) },
                { "LastModifiedBy", role => role.LastModifiedBy.GetFromNullable() },
                { "LastModifiedOn", role => role.LastModifiedOn?.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) ?? "Never" }
            }, sheetName: "Roles");

        var fileName =
            $"SelectedRoles_{DateTimeService.NowDatabaseTime.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat)}.xlsx";

        await WebClientService.InvokeFileDownload(convertedExcelWorkbook, fileName, DataConstants.MimeTypes.OpenXml);

        Snackbar.Add("Successfully exported selected Roles to Excel Workbook For Download", Severity.Success);
    }

    private async Task ExportAllToExcel()
    {
        if (!_canExportRoles) return;

        var allRoles = await RoleService.GetAllAsync();

        if (!allRoles.Succeeded)
        {
            allRoles.Messages.ForEach(x => Snackbar.Add(x));
            return;
        }
        
        var convertedExcelWorkbook = await ExcelService.ExportBase64Async(
            allRoles.Data, dataMapping: new Dictionary<string, Func<AppRoleSlim, object>>
            {
                { "Id", role => role.Id },
                { "Name", role => role.Name },
                { "Description", role => role.Description },
                { "CreatedBy", role => role.CreatedBy.ToString() },
                { "CreatedOn", role => role.CreatedOn.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) },
                { "LastModifiedBy", role => role.LastModifiedBy.GetFromNullable() },
                { "LastModifiedOn", role => role.LastModifiedOn?.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) ?? "Never" }
            }, sheetName: "Roles");

        var fileName =
            $"AllRoles_{DateTimeService.NowDatabaseTime.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat)}.xlsx";

        await WebClientService.InvokeFileDownload(convertedExcelWorkbook, fileName, DataConstants.MimeTypes.OpenXml);

        Snackbar.Add("Successfully exported all Roles to Excel Workbook For Download", Severity.Success);
    }
}