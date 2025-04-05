using Application.Models.GameServer.Game;

namespace GameWeaver.Components.GameServer;

public partial class GameWidget : ComponentBase
{
    [Parameter] public GameSlim Game { get; set; } = new();
    [Parameter] public bool ShowName { get; set; }
    [Parameter] public string CssDisplay { get; set; } = "game-card-lift";
    [Parameter] public bool Vertical { get; set; } = false;
    [Parameter] public int WidthPx { get; set; } = 293;  // 385
    [Parameter] public int HeightPx { get; set; } = 137;  // 180
    [Parameter] public bool GamerMode { get; set; } = false;

    [Inject] public IWebClientService WebClientService { get; set; } = null!;
    [Inject] public HttpClient HttpClient { get; set; } = null!;

    private string _imageUrl = string.Empty;
    private bool _imageExists = true;
    private bool _imageLoaded = false;
    private string CardWidth => $"{WidthPx}px";
    private string CardHeight => $"{HeightPx}px";


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateImage();
            UpdateThemedElements();
        }
    }

    private async Task UpdateImageUrl()
    {
        if (!Vertical && !string.IsNullOrWhiteSpace(Game.UrlLogo))
        {
            if (await UrlExists(Game.UrlLogo))
            {
                _imageExists = true;
                _imageUrl = Game.UrlLogo;
                return;
            }
        }

        if (Vertical && Game.SteamGameId != 0)
        {
            if (await UrlExists($"https://steamcdn-a.akamaihd.net/steam/apps/{Game.SteamGameId}/library_600x900.jpg"))
            {
                _imageExists = true;
                _imageUrl = $"https://steamcdn-a.akamaihd.net/steam/apps/{Game.SteamGameId}/library_600x900.jpg";
                return;
            }
        }

        _imageExists = false;
        _imageUrl = Vertical ? "/images/gameserver/game-default-vertical.jpg" : "/images/gameserver/game-default-horizontal.jpg";
    }

    private void ViewGame()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.Games.ViewId(Game.Id));
    }

    private async Task<bool> UrlExists(string url)
    {
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
        return response.IsSuccessStatusCode;
    }

    public async Task UpdateImage()
    {
        await UpdateImageUrl();
        _imageLoaded = true;
        StateHasChanged();
    }

    private void UpdateThemedElements()
    {
        if (!GamerMode)
        {
            return;
        }

        CssDisplay += " border-rainbow";
        StateHasChanged();
    }
}