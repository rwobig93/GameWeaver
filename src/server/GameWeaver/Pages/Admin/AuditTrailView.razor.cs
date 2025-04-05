using Application.Models.Lifecycle;
using Application.Services.Lifecycle;

namespace GameWeaver.Pages.Admin;

public partial class AuditTrailView
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Inject] private IAuditTrailService AuditService { get; init; } = null!;
    [Inject] private ISerializerService Serializer { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    [Parameter] public Guid TrailId { get; set; }

    private AuditTrailSlim _viewingTrail = new();
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
                await GetViewingAuditTrail();
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

        if (!queryParameters.TryGetValue("trailId", out var queryTrailId)) return;

        var providedIdIsValid = Guid.TryParse(queryTrailId, out var parsedTrailId);
        if (!providedIdIsValid)
            throw new InvalidDataException("Invalid TrailId provided for audit trail view");

        TrailId = parsedTrailId;
    }

    private async Task GetViewingAuditTrail()
    {
        _viewingTrail = (await AuditService.GetByIdAsync(TrailId)).Data!;
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.Admin.AuditTrails);
    }
}