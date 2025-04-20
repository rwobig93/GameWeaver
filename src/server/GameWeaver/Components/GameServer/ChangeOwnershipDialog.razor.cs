using Application.Mappers.Identity;
using Application.Responses.v1.Identity;

namespace GameWeaver.Components.GameServer;

public partial class ChangeOwnershipDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public Guid OwnerId { get; set; } = Guid.Empty;
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.PersonSearch;
    [Parameter] public Color IconColor { get; set; } = Color.Warning;
    [Parameter] public Color TextColor { get; set; } = Color.Default;
    [Parameter] public string Title { get; set; } = "Change Ownership";
    [Parameter] public string ConfirmButtonText { get; set; } = "Change Owner";
    [Parameter] public int IconWidthPixels { get; set; } = 75;
    [Parameter] public int IconHeightPixels { get; set; } = 75;

    [Inject] public IAppUserService AppUserService { get; init; } = null!;


    private List<UserBasicResponse> _users = [];
    private UserBasicResponse? _selectedUser;
    private UserBasicResponse _currentOwner = new() { Username = "Unknown" };
    private string StyleString => $"width: {IconWidthPixels}px; height: {IconHeightPixels}px;";


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateUsers();
            await UpdateCurrentOwner();

            StateHasChanged();
        }
    }

    private async Task UpdateUsers(string searchText = "")
    {
        var response = await AppUserService.SearchPaginatedAsync(searchText, 1, 100);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _users = response.Data.ToResponses();
    }

    private async Task UpdateCurrentOwner()
    {
        var response = await AppUserService.GetByIdAsync(OwnerId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
        }

        _currentOwner = response.Data?.ToResponse() ?? new UserBasicResponse { Username = "Unknown" };
    }

    private async Task<IEnumerable<UserBasicResponse>> FilterUsers(string filterText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(filterText) || filterText.Length < 3)
        {
            return _users;
        }

        await UpdateUsers(filterText);
        return _users;
    }

    private void Confirm()
    {
        if (_selectedUser is null)
        {
            Snackbar.Add("An owner hasn't been selected but one is required", Severity.Error);
            return;
        }

        MudDialog.Close(DialogResult.Ok(_selectedUser.Id));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}