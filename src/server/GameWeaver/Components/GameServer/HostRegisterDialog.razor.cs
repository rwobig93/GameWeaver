
using Application.Constants.Communication;
using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.Identity;
using Application.Requests.GameServer.Host;
using Application.Responses.v1.Identity;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Domain.Enums.Lifecycle;

namespace GameWeaver.Components.GameServer;

public partial class HostRegisterDialog : ComponentBase
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.PowerSettingsNew;
    [Parameter] public Color IconColor { get; set; } = Color.Success;
    [Parameter] public Color TextColor { get; set; } = Color.Default;
    [Parameter] public string Title { get; set; } = "Generate New Host Registration";
    [Parameter] public string ConfirmButtonText { get; set; } = "Generate";
    [Parameter] public int IconWidthPixels { get; set; } = 75;
    [Parameter] public int IconHeightPixels { get; set; } = 75;

    [Inject] public IHostService HostService { get; init; } = null!;
    [Inject] public ITroubleshootingRecordService TshootService { get; init; } = null!;
    [Inject] public IAppUserService AppUserService { get; init; } = null!;


    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";
    private Guid _loggedInUserId = Guid.Empty;
    private List<UserBasicResponse> _users = [];
    private UserBasicResponse _selectedOwner = new() {Username = "Unknown"};
    private readonly HostRegistrationCreateRequest _registerRequest = new();
    private string _allowedPortsRaw = string.Empty;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetAllUsers();
            await GetCurrentUser();
                
            StateHasChanged();
        }
    }

    private async Task GetCurrentUser()
    {
        _loggedInUserId = await CurrentUserService.GetCurrentUserId() ?? Guid.Empty;
        _selectedOwner = _users.FirstOrDefault(x => x.Id == _loggedInUserId) ?? new UserBasicResponse {Username = "Unknown"};
    }

    private async Task GetAllUsers()
    {
        var response = await AppUserService.GetAllAsync();
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _users = response.Data.ToResponses();
    }

    private async Task<IEnumerable<UserBasicResponse>> FilterUsers(string filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return _users;
        }

        await Task.CompletedTask;

        return _users.Where(x =>
            x.Username.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
            x.Id.ToString().Contains(filterText, StringComparison.InvariantCultureIgnoreCase));
    }

    private void RecommendPorts()
    {
        _allowedPortsRaw = "40000-44000";
    }
    
    private async Task GenerateRegistration()
    {
        if (string.IsNullOrWhiteSpace(_registerRequest.Description))
        {
            Snackbar.Add("Description is empty and must have a unique value", Severity.Error);
            return;
        }
        
        var allowedPortsConverted = _allowedPortsRaw.Split(",");
        var parsedPorts = NetworkHelpers.GetPortsFromRangeList(allowedPortsConverted);
        if (parsedPorts.Count < 3)
        {
            Snackbar.Add("Allowed ports provided doesn't have enough valid ports (3), please try again", Severity.Error);
            return;
        }

        _registerRequest.AllowedPorts = allowedPortsConverted.ToList();
        
        if (_loggedInUserId == Guid.Empty)
        {
            var tshootId = await TshootService.CreateTroubleshootRecord(DateTimeService, TroubleshootEntityType.HostRegistrations, Guid.Empty, _loggedInUserId,
                "Failed to create host for registration", new Dictionary<string, string>
                {
                    {"HostName", _registerRequest.Name},
                    {"HostDescription", _registerRequest.Description},
                    {"HostOwnerId", _registerRequest.OwnerId.ToString()},
                    {"HostAllowedPorts", _registerRequest.AllowedPorts.ToString() ?? "[]"},
                    {"Error", "Logged in user Id is empty, this shouldn't happen, we can't generate a registration without knowing who is submitting the request"}
                });
            Snackbar.Add(ErrorMessageConstants.Generic.ContactAdmin, Severity.Error);
            Snackbar.Add(ErrorMessageConstants.Troubleshooting.RecordId(tshootId.Data), Severity.Error);
            return;
        }

        var response = await HostService.RegistrationGenerateNew(_registerRequest, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        MudDialog.Close(DialogResult.Ok(response.Data.RegisterUrl));
    }
    
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}