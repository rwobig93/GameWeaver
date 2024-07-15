using Application.Models.GameServer.Game;

namespace GameWeaver.Components.GameServer;

public partial class GameWidget : ComponentBase
{
    [Parameter] public GameSlim Game { get; set; } = new();
    [Parameter] public string CssDisplay { get; set; } = "game-card-new";
    [Parameter] public int WidthPx { get; set; } = 385;
    [Parameter] public int HeightPx { get; set; } = 180;
    [Parameter] public bool Vertical { get; set; }

    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (Vertical)
            {
                WidthPx = 600;
                HeightPx = 900;
            }
            else
            {
                WidthPx = 385;
                HeightPx = 180;
            }
        }

        await Task.CompletedTask;
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
}