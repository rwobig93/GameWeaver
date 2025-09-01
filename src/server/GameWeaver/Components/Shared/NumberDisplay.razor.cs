namespace GameWeaver.Components.Shared;

public partial class NumberDisplay : ComponentBase
{
    [Parameter] public int Number { get; set; }
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string ClickUrl { get; set; } = "";
    [Parameter] public string ImageUrl { get; set; } = "/images/gameserver/game-default-horizontal.jpg";
    [Parameter] public string CssDisplay { get; set; } = "";
    [Parameter] public int WidthPx { get; set; } = 195; // 293
    [Parameter] public int HeightPx { get; set; } = 91; // 137
    [Parameter] public bool GamerMode { get; set; }

    private string CardWidth => $"{WidthPx}px";
    private string CardHeight => $"{HeightPx}px";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UpdateThemedElements();
            await Task.CompletedTask;
        }
    }

    private void GoToUrl()
    {
        if (string.IsNullOrWhiteSpace(ClickUrl))
        {
            return;
        }

        NavManager.NavigateTo(ClickUrl);
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