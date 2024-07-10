using Microsoft.AspNetCore.Components;

namespace GameWeaver.Components.GameServer;

public partial class GameWidget : ComponentBase
{
    [Parameter] public bool New { get; set; }
    [Parameter] public bool Horizontal { get; set; } = true;
    [Parameter] public string Url { get; set; } = "";

    private string GetUrl()
    {
        if (!string.IsNullOrWhiteSpace(Url))
        {
            return Url;
        }

        if (Horizontal)
        {
            return "https://wobig.tech/downloads/images/game_placeholder_horizontal.jpg";
        }

        return "https://wobig.tech/downloads/images/game_placeholder_verticle.jpg";
    }

    private async Task ButtonClick()
    {
        Snackbar.Add("Clicked!", Severity.Success);
        await Task.CompletedTask;
    }
}