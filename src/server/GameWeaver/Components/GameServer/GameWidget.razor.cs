using Application.Models.GameServer.Game;

namespace GameWeaver.Components.GameServer;

public partial class GameWidget : ComponentBase
{
    [Parameter] public GameSlim Game { get; set; } = new();
    [Parameter] public bool ShowName { get; set; }
    [Parameter] public string CssDisplay { get; set; } = "game-card-lift";
    [Parameter] public bool Vertical { get; set; }
    [Parameter] public int WidthPx { get; set; } = 264;  // 385
    [Parameter] public int HeightPx { get; set; } = 125;  // 180

    [Inject] public IWebClientService WebClientService { get; set; } = null!;

    private ElementReference _gameImage;
    private bool _imageExists = true;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.CompletedTask;
        }
        
        await UpdateImage();
    }

    private string GetUrl()
    {
        // Could look at converting app id to vertical card:
        // https://steamcdn-a.akamaihd.net/steam/apps/<app_id>/library_600x900.jpg
        if (!Vertical && !string.IsNullOrWhiteSpace(Game.UrlLogo))
        {
            return Game.UrlLogo;
        }

        if (Vertical && Game.SteamGameId != 0)
        {
            return $"https://steamcdn-a.akamaihd.net/steam/apps/{Game.SteamGameId}/library_600x900.jpg";
        }
        
        return Vertical ? "/images/gameserver/game-default-vertical.jpg" : "/images/gameserver/game-default-horizontal.jpg";
    }

    private async Task ButtonClick()
    {
        Snackbar.Add($"View game: {Game.FriendlyName}", Severity.Success);
        await Task.CompletedTask;
    }

    private async Task UpdateImage()
    {
        if (Vertical)
        {
            WidthPx = 176;
            HeightPx = 264;
        }
        else
        {
            WidthPx = 390;
            HeightPx = 137;
        }

        var fallbackUrl = Vertical ? "/images/gameserver/game-default-vertical.jpg" : "/images/gameserver/game-default-horizontal.jpg";

        _imageExists = (await WebClientService.GetImageUrlEnsured(_gameImage, fallbackUrl)).Data;
        
        StateHasChanged();
    }
}