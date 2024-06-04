using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.Lifecycle;
using Application.Services.Integrations;
using Application.Services.Lifecycle;

namespace GameWeaver.Pages.Admin;

public partial class AuditTrailAdmin
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    [Inject] private IAuditTrailService AuditService { get; init; } = null!;
    [Inject] private ISerializerService Serializer { get; init; } = null!;
    [Inject] private IExcelService ExcelService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    
    private MudTable<AuditTrailSlim> _table = new();
    private IEnumerable<AuditTrailSlim> _pagedData = new List<AuditTrailSlim>();
    private string _searchString = "";
    private int _totalTrails;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");

    private bool _dense = true;
    private bool _hover = true;
    private bool _striped = true;
    private bool _bordered;
    private bool _canExportTrails;
    
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
        _canExportTrails = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.Audit.Export);
    }
    
    private async Task<TableData<AuditTrailSlim>> ServerReload(TableState state)
    {
        var trailResult = await AuditService.SearchPaginatedAsync(_searchString, state.Page, state.PageSize);
        if (!trailResult.Succeeded)
        {
            trailResult.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return new TableData<AuditTrailSlim>();
        }

        _pagedData = trailResult.Data.ToArray();
        _totalTrails = (await AuditService.GetCountAsync()).Data;

        _pagedData = state.SortLabel switch
        {
            "Id" => _pagedData.OrderByDirection(state.SortDirection, o => o.Id),
            "Timestamp" => _pagedData.OrderByDirection(state.SortDirection, o => o.Timestamp),
            "RecordId" => _pagedData.OrderByDirection(state.SortDirection, o => o.RecordId),
            "Action" => _pagedData.OrderByDirection(state.SortDirection, o => o.Action),
            "TableName" => _pagedData.OrderByDirection(state.SortDirection, o => o.TableName),
            "ChangedByUsername" => _pagedData.OrderByDirection(state.SortDirection, o => o.ChangedByUsername),
            _ => _pagedData
        };

        return new TableData<AuditTrailSlim>() {TotalItems = _totalTrails, Items = _pagedData};
    }

    private void OnSearch(string text)
    {
        // TODO: Searching by Audit Id doesn't filter to the Id provided
        // TODO: Add a different page for viewing troubleshooting logs, here we should be looking at all trails < 500 since t-shooting is >= 500
        _searchString = text;
        _table.ReloadServerData();
    }

    private void ViewTrail(Guid trailId)
    {
        var viewUserUri = QueryHelpers.AddQueryString(AppRouteConstants.Admin.AuditTrailView, "trailId", trailId.ToString());
        NavManager.NavigateTo(viewUserUri);
    }

    private async Task ExportToExcel()
    {
        if (!_canExportTrails) return;
        
        var convertedExcelWorkbook = await ExcelService.ExportBase64Async(
            _pagedData, dataMapping: new Dictionary<string, Func<AuditTrailSlim, object>>
        {
            { "Id", auditTrail => auditTrail.Id },
            { "Timestamp", auditTrail => auditTrail.Timestamp.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) },
            { "RecordId", auditTrail => auditTrail.RecordId.ToString() },
            { "Action", auditTrail => auditTrail.Action.ToString() },
            { "Type", auditTrail => auditTrail.TableName },
            { "ChangedById", auditTrail => auditTrail.ChangedBy.ToString() },
            { "ChangedByUsername", auditTrail => auditTrail.ChangedByUsername },
            { "Before", auditTrail => Serializer.SerializeJson(auditTrail.Before) },
            { "After", auditTrail => Serializer.SerializeJson(auditTrail.After) }
        }, sheetName: "AuditTrails");

        var fileName =
            $"AuditTrails_{DateTimeService.NowDatabaseTime.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat)}.xlsx";

        await WebClientService.InvokeFileDownload(convertedExcelWorkbook, fileName, DataConstants.MimeTypes.OpenXml);

        Snackbar.Add("Successfully exported Audit Trails to Excel Workbook For Download", Severity.Success);
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }
}