using Application.Models.Lifecycle;
using Application.Services.Lifecycle;

namespace GameWeaver.Pages.Admin;

public partial class TroubleshootRecordView : ComponentBase
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    [Inject] private ITroubleshootingRecordService TshootService { get; init; } = null!;
    [Inject] private ISerializerService Serializer { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    
    [Parameter] public Guid RecordId { get; set; }

    private TroubleshootingRecordSlim _viewingRecord = new();
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");

    private bool _invalidDataProvided;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                ParseParametersFromUri();
                await GetClientTimezone();
                await GetViewingTshootRecord();
                StateHasChanged();
            }
        }
        catch
        {
            _invalidDataProvided = true;
            StateHasChanged();
        }
    }

    private void ParseParametersFromUri()
    {
        var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
        var queryParameters = QueryHelpers.ParseQuery(uri.Query);

        if (!queryParameters.TryGetValue("recordId", out var queryRecordId)) return;
        
        var providedIdIsValid = Guid.TryParse(queryRecordId, out var parsedRecordId);
        if (!providedIdIsValid)
        {
            throw new InvalidDataException("Invalid RecordId provided for troubleshooting record view");
        }
            
        RecordId = parsedRecordId;
    }

    private async Task GetViewingTshootRecord()
    {
        _viewingRecord = (await TshootService.GetByIdAsync(RecordId)).Data!;
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
        {
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
        }

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.Admin.TroubleshootRecords);
    }
}