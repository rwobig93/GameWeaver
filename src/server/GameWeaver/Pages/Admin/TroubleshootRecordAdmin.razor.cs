using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.Lifecycle;
using Application.Services.Integrations;
using Application.Services.Lifecycle;

namespace GameWeaver.Pages.Admin;

public partial class TroubleshootRecordAdmin
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    [Inject] private ITroubleshootingRecordService TshootService { get; init; } = null!;
    [Inject] private ISerializerService Serializer { get; init; } = null!;
    [Inject] private IExcelService ExcelService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    
    private MudTable<TroubleshootingRecordSlim> _table = new();
    private IEnumerable<TroubleshootingRecordSlim> _pagedData = new List<TroubleshootingRecordSlim>();
    private string _searchString = "";
    private int _totalTrails;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");

    private bool _dense = true;
    private bool _hover = true;
    private bool _striped = true;
    private bool _bordered;
    private bool _canExportRecords;
    
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
        _canExportRecords = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.System.Troubleshooting.Export);
    }
    
    private async Task<TableData<TroubleshootingRecordSlim>> ServerReload(TableState state, CancellationToken token)
    {
        var trailResult = await TshootService.SearchPaginatedAsync(_searchString, state.Page, state.PageSize);
        if (!trailResult.Succeeded)
        {
            trailResult.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return new TableData<TroubleshootingRecordSlim>();
        }

        _pagedData = trailResult.Data.ToArray();
        _totalTrails = (await TshootService.GetCountAsync()).Data;

        _pagedData = state.SortLabel switch
        {
            "Id" => _pagedData.OrderByDirection(state.SortDirection, o => o.Id),
            "Timestamp" => _pagedData.OrderByDirection(state.SortDirection, o => o.Timestamp),
            "RecordId" => _pagedData.OrderByDirection(state.SortDirection, o => o.RecordId),
            "EntityType" => _pagedData.OrderByDirection(state.SortDirection, o => o.EntityType),
            "ChangedBy" => _pagedData.OrderByDirection(state.SortDirection, o => o.ChangedBy),
            "Message" => _pagedData.OrderByDirection(state.SortDirection, o => o.Message),
            _ => _pagedData
        };

        return new TableData<TroubleshootingRecordSlim>() {TotalItems = _totalTrails, Items = _pagedData};
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _table.ReloadServerData();
    }

    private void ViewRecord(Guid recordId)
    {
        var viewUserUri = QueryHelpers.AddQueryString(AppRouteConstants.Admin.TroubleshootRecordsView, "recordId", recordId.ToString());
        NavManager.NavigateTo(viewUserUri);
    }

    private async Task ExportToExcel()
    {
        if (!_canExportRecords) return;
        
        var convertedExcelWorkbook = await ExcelService.ExportBase64Async(
            _pagedData, dataMapping: new Dictionary<string, Func<TroubleshootingRecordSlim, object>>
        {
            { "Id", record => record.Id },
            { "Timestamp", record => record.Timestamp.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat) },
            { "RecordId", record => record.RecordId.ToString() },
            { "EntityType", record => record.EntityType.ToString() },
            { "ChangedById", record => record.ChangedBy.ToString() },
            { "Message", record => record.Message },
            { "Detail", record => Serializer.SerializeJson(record.Detail) }
        }, sheetName: "TroubleshootRecords");

        var fileName =
            $"TroubleshootRecords_{DateTimeService.NowDatabaseTime.ConvertToLocal(_localTimeZone).ToString(DataConstants.DateTime.DisplayFormat)}.xlsx";

        await WebClientService.InvokeFileDownload(convertedExcelWorkbook, fileName, DataConstants.MimeTypes.OpenXml);

        Snackbar.Add("Successfully exported Tshoot Records to Excel Workbook For Download", Severity.Success);
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }
}